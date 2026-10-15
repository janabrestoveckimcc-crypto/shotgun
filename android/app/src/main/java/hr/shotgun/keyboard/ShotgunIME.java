package hr.shotgun.keyboard;

import android.content.SharedPreferences;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.inputmethodservice.InputMethodService;
import android.media.AudioAttributes;
import android.media.SoundPool;
import android.os.Handler;
import android.os.Looper;
import android.text.InputType;
import android.view.*;
import android.view.inputmethod.*;
import android.widget.*;
import java.util.HashSet;
import java.util.Set;

public final class ShotgunIME extends InputMethodService {
  private SoundPool audio;
  private final int[] sounds = new int[3];
  private final Set<Integer> ready = new HashSet<>();
  private SharedPreferences prefs;
  private LinearLayout keyboard;
  private boolean shifted, symbols, numeric;
  private final Handler handler = new Handler(Looper.getMainLooper());
  private Runnable deleting;
  @Override public void onCreate() {
    super.onCreate(); prefs = getSharedPreferences("shotgun", MODE_PRIVATE);
    audio = new SoundPool.Builder().setMaxStreams(20).setAudioAttributes(new AudioAttributes.Builder().setUsage(AudioAttributes.USAGE_ASSISTANCE_SONIFICATION).setContentType(AudioAttributes.CONTENT_TYPE_SONIFICATION).build()).build();
    audio.setOnLoadCompleteListener((pool,id,status) -> { if (status == 0) ready.add(id); });
    sounds[0] = audio.load(this,R.raw.pump,1); sounds[1] = audio.load(this,R.raw.double_barrel,1); sounds[2] = audio.load(this,R.raw.tactical,1);
  }
  @Override public boolean onEvaluateFullscreenMode() { return false; }
  @Override public View onCreateInputView() {
    keyboard = new LinearLayout(this); keyboard.setOrientation(LinearLayout.VERTICAL); keyboard.setPadding(dp(4),dp(6),dp(4),dp(8)); keyboard.setBackgroundColor(Color.rgb(38,43,34)); render(); return keyboard;
  }
  @Override public void onStartInput(EditorInfo info,boolean restarting) {
    super.onStartInput(info,restarting); shifted = false; symbols = false;
    int kind = info.inputType & InputType.TYPE_MASK_CLASS;
    numeric = kind == InputType.TYPE_CLASS_NUMBER || kind == InputType.TYPE_CLASS_PHONE || kind == InputType.TYPE_CLASS_DATETIME;
    if (keyboard != null) render();
  }
  private int dp(int value) { return Math.round(getResources().getDisplayMetrics().density * value); }
  private void fire() {
    int id = sounds[Math.max(0,Math.min(2,prefs.getInt("sound",0)))];
    if (!prefs.getBoolean("enabled",true) || !ready.contains(id)) return;
    float volume = prefs.getInt("volume",35)/100f;
    audio.play(id,volume,volume,1,0,1f);
  }
  private LinearLayout row() {
    LinearLayout row = new LinearLayout(this); row.setOrientation(LinearLayout.HORIZONTAL); keyboard.addView(row,new LinearLayout.LayoutParams(-1,dp(getResources().getConfiguration().orientation == 2 ? 38 : 48))); return row;
  }
  private void key(LinearLayout row,String text,float weight,Runnable action) {
    Button button = new Button(this); button.setText(text); button.setAllCaps(false); button.setTextSize(text.length()>3 ? 9 : 16); button.setTextColor(Color.rgb(240,245,224)); button.setTypeface(Typeface.MONOSPACE); button.setPadding(0,0,0,0); button.setMinWidth(0); button.setMinimumWidth(0); button.setSoundEffectsEnabled(false);
    GradientDrawable background = new GradientDrawable(); background.setColor(Color.rgb(70,78,59)); background.setCornerRadius(dp(5)); background.setStroke(dp(1),Color.rgb(92,103,75)); button.setBackground(background);
    LinearLayout.LayoutParams size = new LinearLayout.LayoutParams(0,-1,weight); size.setMargins(dp(2),dp(3),dp(2),dp(3)); row.addView(button,size);
    button.setOnClickListener(v -> { fire(); action.run(); });
    if (text.equals("⌫")) {
      button.setOnLongClickListener(v -> {
        fire();
        deleting = new Runnable() { public void run() { delete(); handler.postDelayed(this,70); } }; handler.post(deleting); return true;
      });
      button.setOnTouchListener((v,event) -> { if (event.getActionMasked()==MotionEvent.ACTION_UP || event.getActionMasked()==MotionEvent.ACTION_CANCEL) stopDeleting(); return false; });
    }
  }
  private void commit(String text) {
    InputConnection connection = getCurrentInputConnection();
    if (connection != null) connection.commitText(text,1);
    if (shifted && !symbols) { shifted=false; render(); }
  }
  private void delete() {
    InputConnection connection = getCurrentInputConnection();
    if (connection != null) {
      // DEL lets the editor handle selections and Unicode graphemes without reading text.
      connection.sendKeyEvent(new KeyEvent(KeyEvent.ACTION_DOWN,KeyEvent.KEYCODE_DEL));
      connection.sendKeyEvent(new KeyEvent(KeyEvent.ACTION_UP,KeyEvent.KEYCODE_DEL));
    }
  }
  private void enter() {
    InputConnection connection = getCurrentInputConnection(); EditorInfo info = getCurrentInputEditorInfo();
    if (connection == null || info == null) return;
    int action = info.imeOptions & EditorInfo.IME_MASK_ACTION;
    if ((info.imeOptions & EditorInfo.IME_FLAG_NO_ENTER_ACTION)==0 && action != EditorInfo.IME_ACTION_NONE && action != EditorInfo.IME_ACTION_UNSPECIFIED) connection.performEditorAction(action);
    else connection.commitText("\n",1);
  }
  private void render() {
    keyboard.removeAllViews();
    LinearLayout tools = row();
    key(tools,"GLOBE",1,() -> ((InputMethodManager)getSystemService(INPUT_METHOD_SERVICE)).showInputMethodPicker());
    key(tools,prefs.getBoolean("enabled",true)?"ZVUK ON":"ZVUK OFF",2,() -> { prefs.edit().putBoolean("enabled",!prefs.getBoolean("enabled",true)).apply(); render(); });
    key(tools,"⌄",1,() -> requestHideSelf(0));
    String[] rows = numeric ? new String[]{"123","456","789","+0.,-"} : symbols ? new String[]{"1234567890","@#€%&*()!?","+-=/:;_\"'","[]{}<>\\|~"} : new String[]{"qwertyuiop","asdfghjkl","zxcvbnm","čćžšđ"};
    for (String letters : rows) {
      LinearLayout line = row();
      for (int i=0;i<letters.length();i++) { String character = String.valueOf(letters.charAt(i)); if (shifted && !symbols) character = character.toUpperCase(java.util.Locale.ROOT); final String value = character; key(line,value,1,() -> commit(value)); }
    }
    LinearLayout bottom = row();
    if (!numeric) {
      key(bottom,shifted?"⇧ ON":"⇧",1.1f,() -> { shifted=!shifted; render(); });
      key(bottom,symbols?"ABC":"?123",1.2f,() -> { symbols=!symbols; render(); });
    }
    key(bottom,"RAZMAK",3,() -> commit(" ")); key(bottom,"⌫",1.3f,() -> delete()); key(bottom,"↵",1.3f,() -> enter());
  }
  private void stopDeleting() { if (deleting != null) { handler.removeCallbacks(deleting); deleting=null; } }
  @Override public void onFinishInput() { stopDeleting(); super.onFinishInput(); }
  @Override public void onDestroy() { stopDeleting(); if (audio != null) audio.release(); super.onDestroy(); }
}
