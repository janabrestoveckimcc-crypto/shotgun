# Shotgun Keyboard

**Jedna tipka. Jedan shotgun pucanj.** Preuzmi Windows dodatak i tipkaj u Wordu, WhatsAppu, ChatGPT-u ili drugim aplikacijama. Dodatak radi u pozadini, bez zasebnog prozora za tipkanje. Dostupni su i Chrome/Edge ekstenzija te Android tipkovnica. Tri originalna lokalna efekta: Pump action, Double barrel i Tactical. Brzi pritisci preklapaju zvukove; prethodni pucanj se ne prekida.

**[Stranica za preuzimanje](https://janabrestoveckimcc-crypto.github.io/shotgun/)** · **[Windows EXE](https://janabrestoveckimcc-crypto.github.io/shotgun/downloads/ShotgunKeyboard.exe)** · **[Ekstenzija ZIP](https://janabrestoveckimcc-crypto.github.io/shotgun/downloads/shotgun-keyboard-extension.zip)** · **[Android APK](https://janabrestoveckimcc-crypto.github.io/shotgun/downloads/shotgun-keyboard.apk)**

## Što radi gdje

| Verzija | Područje rada | Zahtjevi |
|---|---|---|
| Web/PWA | Ova web-aplikacija; fizička, zaslonska i mobilna tipkovnica | Suvremeni preglednik, prvi klik na Uključi zvuk |
| Chrome/Edge ekstenzija | Obične HTTP/HTTPS web-stranice i njihovi okviri | Desktop Chromium 116+, ručna instalacija |
| Windows | Tipkanje u uobičajenim Windows aplikacijama | Windows 10/11, .NET Framework 4.x, audio izlaz |
| Android | Polja za tekst u drugim aplikacijama dok je Shotgun odabrana tipkovnica | Android 8+, ručna instalacija APK-a |
| iPhone/iPad | Web-aplikacija, može se dodati na početni zaslon | Safari, aktivna stranica |

Ekstenzije i web-stranice ne mogu slušati tipkovnicu u svim aplikacijama operativnog sustava. Zato su Windows aplikacija i Android IME zasebni paketi. Na macOS-u i Linuxu koristi web ili ekstenziju. Za iOS nije napravljena nativna tipkovnica.

## Instalacija

### Web i iPhone

1. Opcionalno otvori [web-demo](https://janabrestoveckimcc-crypto.github.io/shotgun/demo.html), klikni **Uključi zvuk** i tipkaj u tekstualno polje. Zaslonske tipke također rade. Za zvuk u drugim aplikacijama preuzmi odgovarajući dodatak.
2. Odaberi zvuk i glasnoću. **Esc** utišava zvuk. Brojač vrijedi za trenutačnu sesiju.
3. Android: u izborniku preglednika dodaj aplikaciju na početni zaslon. iPhone: Safari → Dijeli → Dodaj na početni zaslon.
4. Nakon prvog potpunog učitavanja jezgra aplikacije radi i offline. Instalacijski ZIP/APK paketi nisu offline spremljeni.

Zvuk se automatski utišava kad stranica ode u pozadinu. Mobilni preglednik izlaže događaje unosa, a ne svaki fizički dodir softverske tipkovnice: obična slova i brisanje proizvode pucanj, dok predikcija, diktiranje, zamjene i neke IME kombinacije ovise o pregledniku. Za točnu kontrolu svakog dodira na Androidu koristi nativnu tipkovnicu.

### Chrome ili Edge

1. Preuzmi i raspakiraj **shotgun-keyboard-extension.zip**.
2. Otvori `chrome://extensions` ili `edge://extensions`.
3. Uključi **Developer mode** / **Način rada za razvojne programere**.
4. Klikni **Load unpacked** / **Učitaj raspakirano** i odaberi mapu koja sadrži `manifest.json`.
5. Osvježi već otvorene web-stranice. Prikvači ekstenziju, otvori je i klikni **Uključi zvuk**.

Ekstenzija po zadanim postavkama ne svira dok držiš tipku; ponavljanje se može uključiti. HTML polja za lozinke su utišana. Interne stranice poput `chrome://`, trgovina ekstenzija i pojedine zaštićene stranice nisu podržane. Nije objavljena u Chrome Web Storeu. Za uklanjanje odaberi Remove na stranici ekstenzija.

### Windows

1. Preuzmi **ShotgunKeyboard.exe** i spremi ga u trajnu mapu. Svi zvukovi su ugrađeni u jednu datoteku; ZIP je također dostupan uz iste upute.
2. Dvoklikni **ShotgunKeyboard.exe** i nastavi tipkati u svojim aplikacijama. Prvi put je zvuk uključen na 35%. Nije potreban Node.js ni administrator. Dodatak radi u pozadini; postavke otvori desnim klikom na ikonu u Windows programskoj traci, možda pod strelicom ∧.
3. **Ctrl + Alt + M** uključuje ili utišava zvuk u bilo kojoj aplikaciji koja propušta globalne događaje tipkovnice.
4. Zatvaranje prozora ostavlja ikonu uz sat. Desni klik → **Izlaz** potpuno zaustavlja aplikaciju.
5. Za uklanjanje prvo u postavkama isključi **Pokreni pri prijavi u Windows** ako si to uključio. Odaberi Izlaz i izbriši EXE. Opcionalno ukloni postavke iz `%LOCALAPPDATA%\ShotgunKeyboard`.

EXE nije digitalno potpisan. Postavke zvuka spremaju se lokalno. Automatsko pokretanje je isključeno dok ga sam ne uključiš u postavkama. UAC, zaključani zaslon i neke privilegirane aplikacije nisu podržani. Windows verzija ne može pouzdano razlikovati polja za lozinke pa tipkanje u njima također svira; po potrebi utišaj kraticom. Sintetički/injektirani događaji se ignoriraju.

### Android

1. Preuzmi **shotgun-keyboard.apk** i otvori datoteku. Ako Android zatraži, dopusti instalaciju tom izvoru.
2. Otvori **Shotgun Keyboard** → **Uključi tipkovnicu**. U Android postavkama uključi Shotgun Keyboard.
3. Vrati se u aplikaciju → **Odaberi tipkovnicu** → Shotgun Keyboard.
4. Tipkaj u bilo kojoj aplikaciji s uobičajenim poljem za tekst. Tipka **ZVUK ON/OFF** utišava. **GLOBE** otvara odabir tipkovnice.
5. Glasnoću i zvuk postavi u aplikaciji. Za uklanjanje prijeđi na drugu tipkovnicu, zatim deinstaliraj Shotgun Keyboard.

Jednostavan QWERTY raspored ima č, ć, ž, š i đ, simbole, Shift, brojčani raspored i tipku za potvrdu/pretragu. Nema predikcije, swipe unosa, glasovnog unosa ni rječnika. APK je razvojna verzija potpisana razvojnim ključem, nije Play Store izdanje. Ako se CI ključ izgubi, za iduću instalaciju može biti potrebno prvo ukloniti staru verziju. Glasnoća ovisi i o Android glasnoći i načinu zvuka.

**Koristi jednu uključenu verziju odjednom.** Windows aplikacija i ekstenzija istodobno mogu dati dva pucnja za istu tipku. Isto vrijedi za Android IME i zvuk ove web-aplikacije.

## Privatnost

- Nema poslužitelja za aplikacijske podatke, računa, analitike, mikrofona ni telemetrije. GitHub Pages uobičajeno poslužuje statičke datoteke.
- Upisani tekst web-igraonice ostaje u memoriji stranice i nestaje nakon osvježavanja; samo postavke zvuka koriste localStorage.
- Ekstenzija šalje samo interni signal `{ target: 'background', type: 'shot' }`. Ne šalje tipku, tekst ni URL. `storage` služi postavkama, `offscreen` reprodukciji, a content scripts su potrebni za događaje na web-stranicama.
- Windows pamti samo trenutno pritisnute virtualne tipke za detekciju držanja i kratice. Nema logiranja, datoteka s tekstom ni mrežnog koda.
- Android IME predaje pritisnuto slovo aktivnom editoru. Ne čita okolni tekst, ne pohranjuje ga i nema `INTERNET` dozvolu. Androidovo opće upozorenje za tipkovnice primjenjuje se pri omogućavanju svakog IME-a.

## Lokalni razvoj

Potreban je Node.js 22+. Web i ekstenzija nemaju npm ovisnosti.

```sh
git clone https://github.com/janabrestoveckimcc-crypto/shotgun.git shotgun-keyboard
cd shotgun-keyboard
npm test
npm run build
npm start
```

Otvori `http://127.0.0.1:4173`. Izgrađena ekstenzija je u `release/extension`. Lokalni build ne proizvodi APK i Windows ZIP automatski; svi paketi nastaju u CI objavi.

Windows paket, iz PowerShella na Windowsu:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-windows.ps1
```

Android: JDK 17, Android SDK 35 i Gradle 8.9. Otvori mapu `android` u Android Studiju ili:

```sh
gradle -p android assembleDebug
```

`scripts/generate-assets.mjs` stvara originalne PCM WAV efekte i PNG ikone. Zvukovi nemaju ovisnost o vanjskim snimkama. Za primjetno drukčiju boju pucnja prilagodi generator.

## GitHub Pages i preuzimanja

U **Settings → Pages → Source** odaberi **GitHub Actions**. Svaki push na `main` pokreće `.github/workflows/deploy.yml`: gradi Windows EXE i Android APK, provjerava JS, pakira ekstenziju i objavljuje web s tri preuzimanja. Status i eventualne greške vidljive su u kartici **Actions**.

Sve putanje su relativne pa web radi i ispod `/shotgun/`. Android APK izravno na webu je razvojni paket. Za javnu trgovinu potrebno je posebno izdanje s vlasnikovim trajnim potpisnim ključem.

## Provjera

`npm test` provjerava jedan pucanj po događaju, deduplikaciju keydown/beforeinput, držanje tipke, mobilni IME, utišavanje, privatnost poruka ekstenzije i granice audio uzoraka. Windows build provjerava kompilaciju i strukturu svih WAV datoteka. Stvarne Android/iPhone uređaje i ponašanje svake vanjske aplikacije treba provjeriti na tim uređajima; uspješna kompilacija sama po sebi to ne potvrđuje.

Implementacija slijedi [Chrome offscreen API](https://developer.chrome.com/docs/extensions/reference/api/offscreen), [Android InputMethodService](https://developer.android.com/develop/ui/views/touch-and-input/creating-input-method), [Windows LowLevelKeyboardProc](https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc) i [GitHub Pages workflows](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages).
