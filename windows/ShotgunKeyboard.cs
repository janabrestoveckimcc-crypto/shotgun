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
        try { foreach (string name in new[] { "pump", "double", "tactical" }) AudioMixer.ReadWave(name); return 0; }
        catch { return 1; }
      }
      if (args.Length > 0 && args[0] == "--smoke-test") {
        try { using (var audio = new AudioMixer()) using (var hook = new KeyboardHook()) { Thread.Sleep(150); } return 0; }
        catch { return 1; }
      }
      bool created;
      using (var mutex = new Mutex(true, "Local\\ShotgunKeyboard-v1", out created)) {
        if (!created) { MessageBox.Show("Shotgun Keyboard je vec pokrenut. Potrazi ikonu uz sat."); return 0; }
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
    readonly KeyboardHook hook;
    readonly NotifyIcon tray;
    readonly CheckBox enabled = new CheckBox();
    readonly CheckBox repeat = new CheckBox();
    readonly ComboBox sound = new ComboBox();
    readonly Label volumeLabel = new Label();
    readonly ToolStripMenuItem toggle = new ToolStripMenuItem("Ukljuci zvuk");
    readonly string settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShotgunKeyboard", "settings.ini");
    readonly TrackBar volume = new TrackBar { Minimum = 0, Maximum = 100, Value = 35, TickStyle = TickStyle.None };
    readonly CheckBox startup = new CheckBox();
    bool quitting;
    bool ready;
    public KeyboardForm() {
      Text = "Shotgun Keyboard - postavke"; ClientSize = new Size(420, 425); ShowInTaskbar = false;
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
      startup.Text = "Pokreni pri prijavi u Windows"; startup.SetBounds(25,300,340,25);
      try { using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")) startup.Checked = key != null && key.GetValue("ShotgunKeyboard") != null; } catch {}
      startup.CheckedChanged += delegate {
        try { using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")) {
          if (startup.Checked) key.SetValue("ShotgunKeyboard", "\"" + Application.ExecutablePath + "\""); else key.DeleteValue("ShotgunKeyboard",false);
        } } catch (Exception error) { MessageBox.Show(error.Message,"Pokretanje s Windowsom"); }
      };
      var note = new Label { Text = "Ctrl + Alt + M: ukljuci / utisaj\nDodatak radi u pozadini i nakon zatvaranja postavki.\nDesni klik na ikonu > Izlaz za zaustavljanje.", Location = new Point(25,350), Size = new Size(380,60), Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100,110,90) };
      Controls.AddRange(new Control[] { title, subtitle, enabled, volumeLabel, volume, sound, preview, repeat, startup, note });
      audio = new AudioMixer();
      var menu = new ContextMenuStrip();
      menu.Items.Add("Otvori", null, delegate { ShowWindow(); });
      toggle.Click += delegate { enabled.Checked = !enabled.Checked; }; menu.Items.Add(toggle);
      menu.Items.Add("Izlaz", null, delegate { quitting = true; Close(); });
      tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "Shotgun Keyboard - utisano", ContextMenuStrip = menu, Visible = true };
      tray.DoubleClick += delegate { ShowWindow(); };
      sound.SelectedIndexChanged += delegate { SaveSettings(); };
      repeat.CheckedChanged += delegate { SaveSettings(); };
      hook = new KeyboardHook();
      hook.Pressed += delegate(bool isRepeat) { if (enabled.Checked && (!isRepeat || repeat.Checked)) Fire(); };
      hook.Toggle += delegate { enabled.Checked = !enabled.Checked; };
      FormClosing += delegate(object sender, FormClosingEventArgs e) {
        if (!quitting && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); }
      };
      LoadSettings(); ready = true; ApplyEnabled();
      tray.ShowBalloonTip(5000,"Shotgun Keyboard",enabled.Checked ? "Ukljuceno! Tipkaj u Wordu, WhatsAppu ili bilo kojoj aplikaciji. Ctrl + Alt + M utisava. Desni klik na ovu ikonu otvara postavke." : "Zvuk je utisan. Ctrl + Alt + M ukljucuje zvuk.",ToolTipIcon.Info);
    }
    protected override void SetVisibleCore(bool value) { if (!IsHandleCreated) { CreateHandle(); value = false; } base.SetVisibleCore(value); }
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
      SaveSettings();
    }
    protected override void Dispose(bool disposing) {
      if (disposing) { if (hook != null) hook.Dispose(); if (tray != null) { tray.Visible = false; tray.Dispose(); } if (audio != null) audio.Dispose(); }
      base.Dispose(disposing);
    }
  }
  sealed class KeyboardHook : IDisposable {
    delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
    readonly HookProc callback;
    readonly bool[] down = new bool[256];
    IntPtr handle;
    public event Action<bool> Pressed;
    public event Action Toggle;
    public KeyboardHook() {
      callback = OnKey;
      handle = SetWindowsHookEx(13, callback, GetModuleHandle(null), 0);
      if (handle == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Tipkovnica nije dostupna.");
    }
    IntPtr OnKey(int code, IntPtr wParam, IntPtr lParam) {
      if (code >= 0) {
        int key = Marshal.ReadInt32(lParam), flags = Marshal.ReadInt32(lParam, 8), message = wParam.ToInt32();
        if (key >= 0 && key < 256 && (flags & 0x10) == 0) {
          if (message == 0x100 || message == 0x104) {
            bool repeated = down[key]; down[key] = true;
            bool ctrl = down[0xA2] || down[0xA3] || down[0x11], alt = down[0xA4] || down[0xA5] || down[0x12];
            if (key == 0x4D && ctrl && alt) { if (!repeated && Toggle != null) Toggle(); }
            else if (Pressed != null) Pressed(repeated);
          } else if (message == 0x101 || message == 0x105) down[key] = false;
        }
      }
      return CallNextHookEx(handle, code, wParam, lParam);
    }
    public void Dispose() { if (handle != IntPtr.Zero) { UnhookWindowsHookEx(handle); handle = IntPtr.Zero; } }
    [DllImport("user32.dll", SetLastError=true)] static extern IntPtr SetWindowsHookEx(int id, HookProc proc, IntPtr module, uint thread);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr w, IntPtr l);
    [DllImport("kernel32.dll", CharSet=CharSet.Auto)] static extern IntPtr GetModuleHandle(string name);
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
