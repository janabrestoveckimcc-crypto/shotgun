using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ShotgunKeyboard {
  static class Program {
    [STAThread] static int Main(string[] args) {
      if (args.Length > 0 && args[0] == "--self-test") {
        try { foreach (string name in new[] { "pump", "double", "tactical" }) AudioMixer.ReadWave(name); KeyboardState.SelfTest(); return 0; }
        catch { return 1; }
      }
      if (args.Length > 0 && args[0] == "--smoke-test") {
        try { using (var audio = new AudioMixer()) using (var input = new RawKeyboardInput()) { for (int i=0; i<15; i++) { Application.DoEvents(); Thread.Sleep(10); } } return 0; }
        catch { return 1; }
      }
      bool created;
      using (var mutex = new Mutex(true, "Local\\ShotgunKeyboard-v1", out created)) {
        if (!created) { MessageBox.Show("Shotgun Keyboard je vec pokrenut. Otvori njegove postavke preko ikone u Windows programskoj traci."); return 0; }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try { using (var form = new KeyboardForm()) Application.Run(form); }
        catch (Exception error) { MessageBox.Show(error.Message, "Shotgun Keyboard", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1; }
      }
      return 0;
    }
  }
  sealed class KeyboardForm : Form {
    readonly AudioMixer audio;
    readonly RawKeyboardInput input;
    readonly NotifyIcon tray;
    readonly CheckBox enabled = new CheckBox();
    readonly CheckBox repeat = new CheckBox();
    readonly ComboBox sound = new ComboBox();
    readonly Label volumeLabel = new Label();
    readonly ToolStripMenuItem toggle = new ToolStripMenuItem("Ukljuci zvuk");
    readonly string settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShotgunKeyboard", "settings.ini");
    readonly TrackBar volume = new TrackBar { Minimum = 0, Maximum = 100, Value = 35, TickStyle = TickStyle.None };
    readonly Label status = new Label();
    long shots;
    bool quitting;
    bool ready;
    public KeyboardForm() {
      Text = "Shotgun Keyboard - postavke"; ClientSize = new Size(440, 480); ShowInTaskbar = true;
      FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
      BackColor = Color.FromArgb(245,245,239); ForeColor = Color.FromArgb(34,37,31);
      Font = new Font("Segoe UI", 10); StartPosition = FormStartPosition.CenterScreen;
      Icon = SystemIcons.Application;
      var title = new Label { Text = "SHOTGUN KEYBOARD", Font = new Font("Segoe UI", 19, FontStyle.Bold), Location = new Point(24,22), AutoSize = true };
      var subtitle = new Label { Text = "Jedna tipka. Jedan pucanj.", Location = new Point(25,66), AutoSize = true };
      enabled.Text = "Ukljuci zvuk u Windows aplikacijama"; enabled.SetBounds(25,108,370,30);
      enabled.Appearance = Appearance.Button; enabled.TextAlign = ContentAlignment.MiddleCenter;
      enabled.BackColor = Color.FromArgb(245,255,96); enabled.FlatStyle = FlatStyle.Flat;
      enabled.CheckedChanged += delegate { ApplyEnabled(); };
      volumeLabel.Text = "Glasnoca: 35%"; volumeLabel.SetBounds(25,156,340,22);
      volume.SetBounds(18,180,380,30);
      volume.ValueChanged += delegate { if (audio != null) audio.Volume = volume.Value / 100f; volumeLabel.Text = "Glasnoca: " + volume.Value + "%"; SaveSettings(); };
      sound.DropDownStyle = ComboBoxStyle.DropDownList; sound.Items.AddRange(new object[] { "Pump action", "Double barrel", "Tactical" });
      sound.SetBounds(25,222,220,30); sound.SelectedIndex = 0;
      var preview = new Button { Text = "Isprobaj zvuk", BackColor = Color.White };
      preview.SetBounds(260,221,135,30); preview.Click += delegate { Fire(); };
      repeat.Text = "Ponavljaj dok drzim tipku"; repeat.SetBounds(25,267,340,25);
      status.SetBounds(25,302,390,25);
      var note = new Label { Text = "Ctrl + Alt + M: ukljuci / utisaj\nTipkaj u svojim aplikacijama, ovaj prozor mozes sakriti.\nUtisaj browser ekstenziju da ne dobijes dva pucnja.", Location = new Point(25,342), Size = new Size(390,64), Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100,110,90) };
      var background = new Button { Text = "Radi u pozadini" }; background.SetBounds(25,422,185,32); background.Click += delegate { Hide(); };
      var exit = new Button { Text = "Zaustavi i zatvori" }; exit.SetBounds(225,422,185,32); exit.Click += delegate { quitting = true; Close(); };
      Controls.AddRange(new Control[] { title, subtitle, enabled, volumeLabel, volume, sound, preview, repeat, status, note, background, exit });
      audio = new AudioMixer();
      var menu = new ContextMenuStrip();
      menu.Items.Add("Otvori", null, delegate { ShowWindow(); });
      toggle.Click += delegate { enabled.Checked = !enabled.Checked; }; menu.Items.Add(toggle);
      menu.Items.Add("Izlaz", null, delegate { quitting = true; Close(); });
      tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "Shotgun Keyboard - utisano", ContextMenuStrip = menu, Visible = true };
      tray.DoubleClick += delegate { ShowWindow(); };
      sound.SelectedIndexChanged += delegate { SaveSettings(); };
      repeat.CheckedChanged += delegate { SaveSettings(); };
      input = new RawKeyboardInput();
      input.Pressed += delegate(bool isRepeat) {
        if (enabled.Checked && (!isRepeat || repeat.Checked)) { Fire(); shots++; UpdateStatus(); }
      };
      input.Toggle += delegate { enabled.Checked = !enabled.Checked; };
      FormClosing += delegate(object sender, FormClosingEventArgs e) {
        if (!quitting && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); }
      };
      LoadSettings(); ready = true; ApplyEnabled();
      UpdateStatus();
    }
    void UpdateStatus() { status.Text = (enabled.Checked ? "UKLJUCENO" : "UTISANO") + "  |  Pucnjevi od pokretanja: " + shots; }
    void LoadSettings() {
      enabled.Checked = true;
      try {
        if (!File.Exists(settingsFile)) return;
        string[] values = File.ReadAllLines(settingsFile);
        foreach (string line in values) {
          string[] pair = line.Split('='); int number;
          if (pair.Length != 2) continue;
          if (pair[0] == "enabled") enabled.Checked = pair[1] == "true";
          if (pair[0] == "repeat") repeat.Checked = pair[1] == "true";
          if (pair[0] == "volume" && int.TryParse(pair[1],out number)) volume.Value = Math.Max(0,Math.Min(100,number));
          if (pair[0] == "sound" && int.TryParse(pair[1],out number)) sound.SelectedIndex = Math.Max(0,Math.Min(2,number));
        }
      } catch {}
    }
    void SaveSettings() {
      if (!ready) return;
      try { Directory.CreateDirectory(Path.GetDirectoryName(settingsFile)); File.WriteAllLines(settingsFile,new[] { "enabled=" + enabled.Checked.ToString().ToLowerInvariant(), "repeat=" + repeat.Checked.ToString().ToLowerInvariant(), "volume=" + volume.Value, "sound=" + sound.SelectedIndex }); } catch {}
    }
    void ShowWindow() { Show(); WindowState = FormWindowState.Normal; Activate(); }
    void Fire() { audio.Play(sound.SelectedIndex); }
    void ApplyEnabled() {
      if (audio == null || tray == null) return;
      if (!enabled.Checked) audio.Stop();
      enabled.Text = enabled.Checked ? "Zvuk ukljucen - klik za utisavanje" : "Ukljuci zvuk u Windows aplikacijama";
      tray.Text = enabled.Checked ? "Shotgun Keyboard - ukljuceno" : "Shotgun Keyboard - utisano";
      toggle.Text = enabled.Checked ? "Utisaj zvuk" : "Ukljuci zvuk";
      UpdateStatus();
      SaveSettings();
    }
    protected override void Dispose(bool disposing) {
      if (disposing) { if (input != null) input.Dispose(); if (tray != null) { tray.Visible = false; tray.Dispose(); } if (audio != null) audio.Dispose(); }
      base.Dispose(disposing);
    }
  }
  // State lasts only until key-up/device removal. No characters, text or history
  // are collected. Each physical keyboard has its own repeat state.
  sealed class KeyboardState {
    public enum Result { None, Press, Repeat, Toggle }
    readonly bool[] down = new bool[256];
    public Result Accept(int key, int makeCode, int flags) {
      if (key <= 0 || key >= 255 || makeCode == 255) return Result.None;
      if (key == 0x10) key = makeCode == 0x36 ? 0xA1 : 0xA0;
      if (key == 0x11) key = (flags & 2) != 0 ? 0xA3 : 0xA2;
      if (key == 0x12) key = (flags & 2) != 0 ? 0xA5 : 0xA4;
      if ((flags & 1) != 0) { down[key] = false; return Result.None; }
      bool repeated = down[key]; down[key] = true;
      if (key == 0x4D && (down[0xA2] || down[0xA3]) && (down[0xA4] || down[0xA5])) return repeated ? Result.None : Result.Toggle;
      return repeated ? Result.Repeat : Result.Press;
    }
    public static void SelfTest() {
      var state = new KeyboardState();
      Expect(state.Accept(65,30,0),Result.Press);
      Expect(state.Accept(65,30,0),Result.Repeat);
      Expect(state.Accept(65,30,1),Result.None);
      Expect(state.Accept(65,30,0),Result.Press);
      Expect(state.Accept(255,0,0),Result.None);
      state.Accept(17,29,0); state.Accept(18,56,2);
      Expect(state.Accept(77,50,0),Result.Toggle);
      Expect(state.Accept(77,50,0),Result.None);
      state.Accept(77,50,1); state.Accept(18,56,3);
      Expect(state.Accept(77,50,0),Result.Press);
      var secondKeyboard = new KeyboardState();
      Expect(secondKeyboard.Accept(65,30,0),Result.Press);
      Expect(Marshal.SizeOf(typeof(RawKeyboardInput.Header)), IntPtr.Size == 8 ? 24 : 16);
    }
    static void Expect<T>(T actual, T expected) { if (!EqualityComparer<T>.Default.Equals(actual,expected)) throw new InvalidOperationException("Keyboard state self-test failed."); }
  }
  // Receive Windows WM_INPUT notifications in our own message-only window.
  // No global hook, injection, key suppression, polling or registry writes.
  sealed class RawKeyboardInput : NativeWindow, IDisposable {
    [StructLayout(LayoutKind.Sequential)] struct Device { public ushort Page, Usage; public uint Flags; public IntPtr Target; }
    [StructLayout(LayoutKind.Sequential)] internal struct Header { public uint Type, Size; public IntPtr Device; public UIntPtr Parameter; }
    [StructLayout(LayoutKind.Sequential)] struct Keyboard { public ushort MakeCode, Flags, Reserved, Key; public uint Message, Extra; }
    readonly Dictionary<IntPtr,KeyboardState> devices = new Dictionary<IntPtr,KeyboardState>();
    readonly uint headerSize = (uint)Marshal.SizeOf(typeof(Header));
    bool registered;
    public event Action<bool> Pressed;
    public event Action Toggle;
    public RawKeyboardInput() {
      CreateHandle(new CreateParams { Caption = "Shotgun Keyboard input", Parent = new IntPtr(-3) });
      var device = new Device { Page = 1, Usage = 6, Flags = 0x2100, Target = Handle }; // INPUTSINK | DEVNOTIFY
      if (!RegisterRawInputDevices(new[] { device },1,(uint)Marshal.SizeOf(typeof(Device)))) {
        int error = Marshal.GetLastWin32Error(); DestroyHandle();
        throw new System.ComponentModel.Win32Exception(error,"Windows nije omogucio primanje tipki.");
      }
      registered = true;
    }
    protected override void WndProc(ref Message message) {
      try {
        if (message.Msg == 0xFF) ReadInput(message.LParam);
        if (message.Msg == 0xFE && message.WParam.ToInt32() == 2) devices.Remove(message.LParam);
      } finally { base.WndProc(ref message); }
    }
    void ReadInput(IntPtr input) {
      uint size = 0;
      if (GetRawInputData(input,0x10000003,IntPtr.Zero,ref size,headerSize) == uint.MaxValue || size < headerSize + 16 || size > 65536) return;
      IntPtr buffer = Marshal.AllocHGlobal((int)size);
      try {
        uint read = GetRawInputData(input,0x10000003,buffer,ref size,headerSize);
        if (read == uint.MaxValue || read < headerSize + 16) return;
        var header = (Header)Marshal.PtrToStructure(buffer,typeof(Header));
        if (header.Type != 1 || header.Size > read) return;
        var key = (Keyboard)Marshal.PtrToStructure(IntPtr.Add(buffer,(int)headerSize),typeof(Keyboard));
        KeyboardState state;
        if (!devices.TryGetValue(header.Device,out state)) { state = new KeyboardState(); devices.Add(header.Device,state); }
        KeyboardState.Result result = state.Accept(key.Key,key.MakeCode,key.Flags);
        if (result == KeyboardState.Result.Toggle) { if (Toggle != null) Toggle(); }
        else if (result != KeyboardState.Result.None && Pressed != null) Pressed(result == KeyboardState.Result.Repeat);
      } finally { Marshal.FreeHGlobal(buffer); }
    }
    public void Dispose() {
      if (registered) {
        RegisterRawInputDevices(new[] { new Device { Page = 1, Usage = 6, Flags = 1, Target = IntPtr.Zero } },1,(uint)Marshal.SizeOf(typeof(Device)));
        registered = false;
      }
      devices.Clear();
      if (Handle != IntPtr.Zero) DestroyHandle();
    }
    [DllImport("user32.dll", SetLastError=true)] static extern bool RegisterRawInputDevices([In] Device[] devices, uint count, uint size);
    [DllImport("user32.dll", SetLastError=true)] static extern uint GetRawInputData(IntPtr input, uint command, IntPtr data, ref uint size, uint headerSize);
  }
  // A single waveOut device with a small streaming ring. Voices overlap without
  // stopping previous shots. The keyboard callback never performs audio IO.
  sealed class AudioMixer : IDisposable {
    [StructLayout(LayoutKind.Sequential, Pack=2)] struct WaveFormat { public ushort tag, channels; public uint rate, bytes; public ushort align, bits, extra; }
    [StructLayout(LayoutKind.Sequential)] struct WaveHeader { public IntPtr data; public uint length, recorded; public UIntPtr user; public uint flags, loops; public IntPtr next; public UIntPtr reserved; }
    sealed class Voice { public float[] data; public int position; }
    const int Frames = 256, Count = 4;
    readonly IntPtr[] data = new IntPtr[Count], headers = new IntPtr[Count];
    readonly List<Voice> voices = new List<Voice>();
    readonly object gate = new object();
    readonly float[][] samples;
    readonly Thread worker;
    IntPtr device;
    volatile bool running;
    public volatile float Volume = .35f;
    public static float[] ReadWave(string name) {
      byte[] bytes;
      using (Stream stream = typeof(AudioMixer).Assembly.GetManifestResourceStream("ShotgunKeyboard.Audio." + name + ".wav")) {
        if (stream == null) throw new InvalidDataException("Nedostaje zvuk: " + name);
        using (var memory = new MemoryStream()) { stream.CopyTo(memory); bytes = memory.ToArray(); }
      }
      if (bytes.Length < 44 || System.Text.Encoding.ASCII.GetString(bytes,0,4) != "RIFF" || BitConverter.ToUInt16(bytes,20) != 1 || BitConverter.ToUInt16(bytes,22) != 1 || BitConverter.ToUInt32(bytes,24) != 44100 || BitConverter.ToUInt16(bytes,34) != 16) throw new InvalidDataException("Neispravan WAV zvuk: " + name);
      var output = new float[(bytes.Length - 44) / 2];
      for (int i = 0; i < output.Length; i++) output[i] = BitConverter.ToInt16(bytes,44 + i*2) / 32768f;
      return output;
    }
    public AudioMixer() {
      samples = new[] { ReadWave("pump"), ReadWave("double"), ReadWave("tactical") };
      var format = new WaveFormat { tag = 1, channels = 1, rate = 44100, bytes = 88200, align = 2, bits = 16, extra = 0 };
      Check(waveOutOpen(out device, new UIntPtr(0xFFFFFFFF), ref format, IntPtr.Zero, IntPtr.Zero, 0));
      try {
        for (int i = 0; i < Count; i++) {
          data[i] = Marshal.AllocHGlobal(Frames * 2); headers[i] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WaveHeader)));
          var header = new WaveHeader { data = data[i], length = Frames * 2 };
          Marshal.StructureToPtr(header, headers[i], false);
          Check(waveOutPrepareHeader(device, headers[i], Marshal.SizeOf(typeof(WaveHeader))));
        }
        running = true; worker = new Thread(Mix) { IsBackground = true, Name = "Shotgun audio", Priority = ThreadPriority.AboveNormal }; worker.Start();
      } catch { Dispose(); throw; }
    }
    public void Play(int sound) { lock (gate) { if (voices.Count >= 24) voices.RemoveAt(0); voices.Add(new Voice { data = samples[Math.Max(0, Math.Min(2,sound))] }); } }
    public void Stop() { lock (gate) voices.Clear(); }
    void Mix() {
      var buffer = new short[Frames]; var queued = new bool[Count];
      while (running) {
        for (int b = 0; b < Count; b++) {
          var header = (WaveHeader)Marshal.PtrToStructure(headers[b], typeof(WaveHeader));
          if (queued[b] && (header.flags & 1) == 0) continue;
          lock (gate) {
            for (int i = 0; i < Frames; i++) {
              float sum = 0;
              for (int v = voices.Count - 1; v >= 0; v--) {
                Voice voice = voices[v];
                if (voice.position >= voice.data.Length) { voices.RemoveAt(v); continue; }
                sum += voice.data[voice.position++];
              }
              buffer[i] = (short)(Math.Tanh(sum * Volume) * 30000);
            }
          }
          Marshal.Copy(buffer,0,data[b],Frames);
          if (waveOutWrite(device,headers[b],Marshal.SizeOf(typeof(WaveHeader))) != 0) { running = false; break; }
          queued[b] = true;
        }
        Thread.Sleep(2);
      }
    }
    public void Dispose() {
      running = false; if (worker != null && worker.IsAlive) worker.Join();
      if (device != IntPtr.Zero) waveOutReset(device);
      for (int i = 0; i < Count; i++) {
        if (headers[i] != IntPtr.Zero) { if (device != IntPtr.Zero) waveOutUnprepareHeader(device,headers[i],Marshal.SizeOf(typeof(WaveHeader))); Marshal.FreeHGlobal(headers[i]); headers[i] = IntPtr.Zero; }
        if (data[i] != IntPtr.Zero) { Marshal.FreeHGlobal(data[i]); data[i] = IntPtr.Zero; }
      }
      if (device != IntPtr.Zero) { waveOutClose(device); device = IntPtr.Zero; }
    }
    static void Check(uint result) { if (result != 0) throw new InvalidOperationException("Audio uredaj nije dostupan (" + result + ")."); }
    [DllImport("winmm.dll")] static extern uint waveOutOpen(out IntPtr device, UIntPtr id, ref WaveFormat format, IntPtr callback, IntPtr instance, uint flags);
    [DllImport("winmm.dll")] static extern uint waveOutPrepareHeader(IntPtr device, IntPtr header, int size);
    [DllImport("winmm.dll")] static extern uint waveOutUnprepareHeader(IntPtr device, IntPtr header, int size);
    [DllImport("winmm.dll")] static extern uint waveOutWrite(IntPtr device, IntPtr header, int size);
    [DllImport("winmm.dll")] static extern uint waveOutReset(IntPtr device);
    [DllImport("winmm.dll")] static extern uint waveOutClose(IntPtr device);
  }
}
