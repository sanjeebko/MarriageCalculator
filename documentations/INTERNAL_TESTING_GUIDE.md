# Internal Test Release & Distribution Runbook

This guide covers everything required to sign, bundle, deploy, and distribute **AAA Marriage Calculator** (v1.0.0) for internal test distribution via the **Google Play Console Internal Testing track** and direct tester APK sideloading.

---

## 1. App Configuration & Metadata Checklist

| Property | Value | Notes |
| :--- | :--- | :--- |
| **Package / Application ID** | `np.com.sanjeeb.marriagecalculator` | Matches `google-services.json` |
| **App Name** | `AAA Marriage Calculator` | Defined in `res/values/strings.xml` |
| **Version Code** | `1` | Increment for subsequent releases |
| **Version Name** | `1.0.0` | In `app/build.gradle.kts` & About dialog |
| **Target SDK** | `34` (Android 14) | Meets Google Play requirement (API 34+) |
| **Minimum SDK** | `26` (Android 8.0) | Covers ~96% of active Android devices |
| **Build Artifact** | `.aab` (Android App Bundle) | Required for Play Console upload |
| **Sideload Artifact** | `.apk` (APK Package) | For direct tester phone installation |

---

## 2. Release Signing Configuration

### Step 2.1: Generate an Upload Keystore (One-Time Setup)
If you don't already have an upload keystore for this app, generate one using Java `keytool` from your terminal:

```bash
keytool -genkey -v -keystore upload-keystore.jks -alias marriage-calculator-upload -keyalg RSA -keysize 2048 -validity 10000
```
> [!IMPORTANT]
> Store the generated `upload-keystore.jks` in a secure location and back it up safely. Keep your passwords secure.

### Step 2.2: Configure `keystore.properties`
1. In `MarriageCalculator/Android/`, create a file named `keystore.properties` (based on `keystore.properties.example`):
   ```properties
   storeFile=upload-keystore.jks
   storePassword=YOUR_KEYSTORE_PASSWORD
   keyAlias=marriage-calculator-upload
   keyPassword=YOUR_KEY_PASSWORD
   ```
2. Place `upload-keystore.jks` in `MarriageCalculator/Android/` (or provide an absolute/relative path in `storeFile`).
3. Note that `keystore.properties` and `*.jks` are in `.gitignore` to prevent committing secrets to git.

*(Optional)* In CI/CD environments, you can alternatively provide the credentials as environment variables:
`KEYSTORE_FILE`, `KEYSTORE_PASSWORD`, `KEY_ALIAS`, `KEY_PASSWORD`.

---

## 3. Building for Release

### Step 3.1: Build Signed Android App Bundle (AAB for Google Play)
To build the AAB with default local development API:
```bash
cd MarriageCalculator/Android
./gradlew bundleRelease
```

To build against your hosted/remote Kubernetes or staging API:
```bash
./gradlew bundleRelease -PAPI_BASE_URL="https://mcapi.sanjeebojha.com.np/api/"
```

**Output Artifact**:
- `MarriageCalculator/Android/app/build/outputs/bundle/release/app-release.aab`

### Step 3.2: Build Standalone Signed APK (For Direct Tester Sideloading)
To build a universal/standalone release APK for testers who cannot use the Play Store:
```bash
./gradlew assembleRelease -PAPI_BASE_URL="https://mcapi.sanjeebojha.com.np/api/"
```

**Output Artifact**:
- `MarriageCalculator/Android/app/build/outputs/apk/release/app-release.apk`

---

## 4. Google Play Console Setup: Internal Testing Track

The **Internal Testing** track allows up to 100 invited testers to install and test updates within seconds of uploading, with zero Play Store review delays.

### Step 4.1: Create the Application
1. Go to [Google Play Console](https://play.google.com/console).
2. Click **Create app**:
   - **App name**: `AAA Marriage Calculator`
   - **Default language**: English (United States)
   - **App or game**: Game
   - **Free or paid**: Free
3. Accept the declarations and click **Create app**.

### Step 4.2: Enable Google Play App Signing
1. Navigate to **Release > Setup > App signing**.
2. Choose **Use Google-generated key** or upload your private key.
3. Once enabled, copy the **SHA-1 certificate fingerprint** from the **App signing key certificate** and the **Upload key certificate**.

### Step 4.3: Add SHA-1 to Firebase & Google Cloud (For Google Sign-In)
For Google Sign-In to work on tester devices installed from Google Play:
1. Open the [Firebase Console](https://console.firebase.google.com/) > Project `marriagecalculator-197bd`.
2. Go to **Project Settings > General > Your apps > np.com.sanjeeb.marriagecalculator**.
3. Click **Add fingerprint** and paste:
   - Your local debug SHA-1 (already present)
   - Your `upload-keystore.jks` SHA-1
   - The **Google Play App Signing SHA-1** (critical for Play Store installs)
4. Download the updated `google-services.json` if new OAuth client IDs are generated.

### Step 4.4: Create an Internal Test Release
1. In Play Console, navigate to **Testing > Internal testing**.
2. Click **Create new release**.
3. Upload `app-release.aab`.
4. Set Release name: `1.0.0 (1)`.
5. Enter Release notes:
   ```
   Initial internal test build of AAA Marriage Calculator:
   - 2-6 player Marriage card game score calculation (Normal, Kidnap, Murder modes)
   - Real-time seating table and dealer management
   - Offline guest mode and Google Sign-In support
   - History and audit logging
   - 6 custom app themes
   ```
6. Click **Next**, review summary, and click **Start rollout to Internal testing**.

---

## 5. Inviting Testers & Distribution

1. In Play Console, under **Internal testing**, switch to the **Testers** tab.
2. Create an email list (e.g. `marriage-calculator-internal-testers`) and add tester Gmail addresses.
3. Copy the **Join on the web** or **Join on Android** invitation link.
4. Share the link with your testers. Once they accept the invite, they can download the app directly from the Google Play Store on their Android devices.

---

## 6. Tester Smoke-Test Checklist

Ask your internal testers to verify the following key features:

- [ ] **Launch & Splash Screen**: Opens with smooth obsidian Ace of Spades theme.
- [ ] **Sign-In Options**:
  - [ ] Google Sign-In works cleanly with avatar and profile display.
  - [ ] "Play as Guest" opens offline dashboard immediately without network requirement.
  - [ ] Privacy Policy and Terms of Service links open in default browser.
- [ ] **Game Setup & Customization**:
  - [ ] Create a 2, 3, 4, 5, or 6 player game.
  - [ ] Verify point rates, penalties, and game modes (Normal, Kidnap, Murder).
- [ ] **Table View & Seating**:
  - [ ] Circular seating table renders cleanly.
  - [ ] Dealer badge rotates clockwise after every round.
  - [ ] Re-arrange seats and Player Mapping dialog work.
- [ ] **Scoring Engine**:
  - [ ] Enter Seen/Unseen/Dublee states and Maal values.
  - [ ] Instant preview matches expected Central Collection calculation.
  - [ ] Scoreboard balance updates accurately ("Who owes whom").
- [ ] **History & Activity Log**:
  - [ ] Navigating to "History" displays timeline cards and date jump.
  - [ ] Same-day logins aggregate into expandable summary card.
- [ ] **More Apps & About Dialog**:
  - [ ] About dialog displays Version 1.0.0 and rules.
  - [ ] More Apps dialog displays authentic portfolio icons and launches external links.
- [ ] **App Theme Switcher**:
  - [ ] Switching between all 6 themes (Black & White, Tihar Night, High Contrast Dark, Midnight Frost, Marigold Day, Himalayan Mist) persists cleanly.
