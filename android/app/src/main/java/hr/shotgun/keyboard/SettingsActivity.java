package hr.shotgun.keyboard;

import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.os.Bundle;
import android.provider.Settings;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.*;

public final class SettingsActivity extends Activity {
  @Override public void onCreate(Bundle state) {
    super.onCreate(state);
    SharedPreferences prefs = getSharedPreferences("shotgun", MODE_PRIVATE);
    ScrollView scroll = new ScrollView(this);
    LinearLayout box = new LinearLayout(this); box.setOrientation(LinearLayout.VERTICAL); box.setPadding(32,56,32,40); box.setBackgroundColor(Color.rgb(245,245,239)); scroll.addView(box);
    TextView title = new TextView(this); title.setText("SHOTGUN\nKEYBOARD"); title.setTextSize(34); title.setTextColor(Color.rgb(34,37,31)); box.addView(title);
    TextView intro = new TextView(this); intro.setText("Jedna tipka. Jedan pucanj.\n\n1. Uključi tipkovnicu u Android postavkama.\n2. Odaberi Shotgun Keyboard kao aktivnu.\n3. Otvori bilo koju aplikaciju s poljem za tekst.\n"); intro.setTextSize(15); box.addView(intro);
    Button enable = new Button(this); enable.setText("1. Uključi tipkovnicu"); enable.setOnClickListener(v -> startActivity(new Intent(Settings.ACTION_INPUT_METHOD_SETTINGS))); box.addView(enable);
    Button select = new Button(this); select.setText("2. Odaberi tipkovnicu"); select.setOnClickListener(v -> ((InputMethodManager)getSystemService(INPUT_METHOD_SERVICE)).showInputMethodPicker()); box.addView(select);
    Switch sound = new Switch(this); sound.setText("Shotgun zvuk"); sound.setChecked(prefs.getBoolean("enabled", true)); sound.setPadding(0,28,0,20); sound.setOnCheckedChangeListener((v, checked) -> prefs.edit().putBoolean("enabled", checked).apply()); box.addView(sound);
    TextView volume = new TextView(this); volume.setText("Glasnoća: " + prefs.getInt("volume",35) + "%"); box.addView(volume);
    SeekBar slider = new SeekBar(this); slider.setMax(100); slider.setProgress(prefs.getInt("volume",35)); slider.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
      public void onProgressChanged(SeekBar bar,int progress,boolean user) { prefs.edit().putInt("volume",progress).apply(); volume.setText("Glasnoća: " + progress + "%"); }
      public void onStartTrackingTouch(SeekBar bar) {} public void onStopTrackingTouch(SeekBar bar) {}
    }); box.addView(slider);
    Spinner type = new Spinner(this); type.setAdapter(new ArrayAdapter<String>(this,android.R.layout.simple_spinner_dropdown_item,new String[]{"Pump action","Double barrel","Tactical"})); type.setSelection(prefs.getInt("sound",0)); type.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
      public void onItemSelected(AdapterView<?> p,View v,int position,long id) { prefs.edit().putInt("sound",position).apply(); }
      public void onNothingSelected(AdapterView<?> p) {}
    }); box.addView(type);
    EditText test = new EditText(this); test.setHint("Isprobaj tipkovnicu ovdje…"); test.setMinLines(2); box.addView(test);
    TextView privacy = new TextView(this); privacy.setText("\nNema internetske dozvole, snimanja teksta, analitike ni predikcije. Android prikazuje opće upozorenje za svaku novu tipkovnicu. Ova aplikacija tekst samo predaje aktivnom polju.\n\nTipka GLOBE mijenja tipkovnicu. ZVUK uključuje ili utišava pucnjeve. Za povratak na svoju tipkovnicu koristi odabir tipkovnice u Androidu.\n\nOvo je jednostavna QWERTY tipkovnica s hrvatskim slovima. Ne sadrži swipe unos ili automatske ispravke."); privacy.setTextSize(12); privacy.setPadding(0,20,0,0); box.addView(privacy);
    box.setOnApplyWindowInsetsListener((v,insets) -> { box.setPadding(32,Math.max(32,insets.getSystemWindowInsetTop()),32,Math.max(32,insets.getSystemWindowInsetBottom())); return insets; });
    setContentView(scroll);
  }
}
