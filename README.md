# Shotgun Keyboard

**Jedna tipka. Jedan shotgun pucanj.** Dostupni su Chrome/Edge ekstenzija za web-stranice i razvojna Android tipkovnica. Windows paket povučen je nakon što ga je antivirus blokirao pri lokalnoj provjeri; zvuk u desktop Wordu, WhatsAppu i Codexu trenutačno nije dostupan. Tri originalna lokalna efekta: Pump action, Double barrel i Tactical. Brzi pritisci preklapaju zvukove; prethodni pucanj se ne prekida.

**[Stranica za preuzimanje](https://janabrestoveckimcc-crypto.github.io/shotgun/)** · **[Ekstenzija ZIP](https://janabrestoveckimcc-crypto.github.io/shotgun/downloads/shotgun-keyboard-extension.zip)** · **[Android APK](https://janabrestoveckimcc-crypto.github.io/shotgun/downloads/shotgun-keyboard.apk)**

## Što radi gdje

| Verzija | Područje rada | Zahtjevi |
|---|---|---|
| Web/PWA | Ova web-aplikacija; fizička, zaslonska i mobilna tipkovnica | Suvremeni preglednik, prvi klik na Uključi zvuk |
| Chrome/Edge ekstenzija | Obične HTTP/HTTPS web-stranice i njihovi okviri | Desktop Chromium 116+, ručna instalacija |
| Windows | Preuzimanje povučeno nakon antivirusne blokade | Potrebna analiza i ponovna provjera |
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

Paket za Chrome Web Store 1.0.1 pripremljen je kao [Chrome ZIP](https://janabrestoveckimcc-crypto.github.io/shotgun/downloads/shotgun-keyboard-chrome-1.0.1.zip). Mapa `store` sadrži opis, razloge za dozvole, upute recenzentima, ikonu i slike za objavu. `store/UPLOAD.txt` opisuje preostale korake. To je pripremljen paket, a ne potvrda objave ili Googleova odobrenja. [Politika privatnosti ekstenzije](https://janabrestoveckimcc-crypto.github.io/shotgun/privacy.html) dostupna je javno.

### Windows

**Paket je povučen.** Pri lokalnom pokušaju pokretanja Windows je prijavio da datoteka sadrži virus ili potencijalno neželjen softver. Uzrok detekcije još nije utvrđen. EXE i Windows ZIP više se ne objavljuju kroz GitHub Pages. Ne zaobilazi antivirusnu blokadu. Izvorni kod ostaje dostupan za analizu; uspješan raniji CI build nije potvrda ispravnosti ili sigurnosti paketa.

Razvojna implementacija sada koristi Windows Raw Input (`WM_INPUT`, `RIDEV_INPUTSINK`), bez globalnog keyboard hooka i bez upisa u automatsko pokretanje. Prikazuje postavke pri pokretanju, prekidač zvuka, brojač pucnjeva za trenutačnu sesiju i gumb za potpuno zatvaranje. Nova verzija kompilira se, ali lokalna antivirusna blokada ostaje; primanje fizičkih tipki i reprodukcija kroz druge aplikacije još nisu potvrđeni. CI provjerava kod i ugrađene zvukove, bez objave Windows datoteka.

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
- Windows razvojni kod pamti samo trenutno pritisnute virtualne tipke po uređaju za detekciju držanja i kratice. Brojač pucnjeva ostaje u memoriji tijekom sesije. Nema logiranja tipki, datoteka s tekstom ni mrežnog koda.
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

Otvori `http://127.0.0.1:4173`. Izgrađena ekstenzija je u `release/extension`. Lokalni build ne proizvodi APK automatski; APK nastaje u CI objavi. Windows distribucija je pauzirana.

Windows razvojna skripta (distribucija i pokretanje paketa pauzirani su dok se detekcija ne analizira):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-windows.ps1
```

Android: JDK 17, Android SDK 35 i Gradle 8.9. Otvori mapu `android` u Android Studiju ili:

```sh
gradle -p android assembleDebug
```

`scripts/generate-assets.mjs` stvara originalne PCM WAV efekte i PNG ikone. Zvukovi nemaju ovisnost o vanjskim snimkama. Za primjetno drukčiju boju pucnja prilagodi generator.

## GitHub Pages i preuzimanja

U **Settings → Pages → Source** odaberi **GitHub Actions**. Svaki push na `main` pokreće `.github/workflows/deploy.yml`: gradi Android APK, provjerava JS, pakira ekstenziju i objavljuje web. Windows EXE i ZIP ne distribuiraju se. Status i eventualne greške vidljive su u kartici **Actions**.

Sve putanje su relativne pa web radi i ispod `/shotgun/`. Android APK izravno na webu je razvojni paket. Za javnu trgovinu potrebno je posebno izdanje s vlasnikovim trajnim potpisnim ključem.

## Provjera

`npm test` provjerava jedan pucanj po događaju, deduplikaciju keydown/beforeinput, držanje tipke, mobilni IME, utišavanje, privatnost poruka ekstenzije i granice audio uzoraka. Windows `--self-test` provjerava strukturu svih WAV datoteka, držanje i otpuštanje tipke, kraticu za utišavanje, odvojeno stanje uređaja i veličinu Raw Input zaglavlja. `--smoke-test` provjerava otvaranje audio izlaza i registraciju ulaza, bez potvrde primanja fizičkih pritisaka. Stvarne Android/iPhone uređaje i ponašanje svake vanjske aplikacije treba provjeriti na tim uređajima; uspješna kompilacija sama po sebi to ne potvrđuje.

Implementacija slijedi [Chrome offscreen API](https://developer.chrome.com/docs/extensions/reference/api/offscreen), [Android InputMethodService](https://developer.android.com/develop/ui/views/touch-and-input/creating-input-method), [Windows Raw Input](https://learn.microsoft.com/en-us/windows/win32/inputdev/about-raw-input) i [GitHub Pages workflows](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages).
