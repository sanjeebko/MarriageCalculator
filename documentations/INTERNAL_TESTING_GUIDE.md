# Internal Test Release & Distribution Runbook

This guide covers everything required to sign, bundle, deploy, and distribute **AAA Marriage Calculator** (v1.0.0) for internal test distribution via the **Google Play Console Internal Testing track** and direct tester APK sideloading.

---

## 1. App Configuration & Metadata Checklist

| Property | Value | Notes |
| :--- | :--- | :--- |
| **Package / Application ID** | `np.com.sanjeeb.marriagecalculator` | Matches `google-services.json` |
| **App Name** | `AAA Marriage Calculator` | Defined in `res/values/strings.xml` |
| **Version Code** | `4` | Increment for subsequent releases |
| **Version Name** | `1.0.3` | In `app/build.gradle.kts` & About dialog |
| **Target SDK** | `36` (Android 16) | Meets Google Play requirement (API 36+) |
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

## 3. Product Flavors & Environments

The app supports 3 distinct product flavors for different stages of testing:

| Flavor | Target Backend API | Build Variant | Output Artifact | Typical Use Case |
| :--- | :--- | :--- | :--- | :--- |
| **`local`** | `http://10.0.2.2:5000/api/` | `localDebug` / `localRelease` | `app-local-debug.apk` | Fast local iteration against .NET API running locally |
| **`dev`** | `http://192.168.1.159/api/` | `devDebug` / `devRelease` | `app-dev-debug.apk` | Testing against Kubernetes dev cluster on home LAN |
| **`prod`** | `https://mcapi.sanjeebojha.com.np/api/` | `prodDebug` / `prodRelease` | `app-prod-release.aab` | Google Play Internal Testing & public verification |

---

## 4. Building Artifacts

### Step 4.1: Build Signed Android App Bundle for Google Play (`prodRelease`)
To build the signed AAB bundle for the internal test release:
```bash
cd MarriageCalculator/Android
./gradlew bundleProdRelease
```

*(Optional)* If you ever need to override the backend API URL for this build:
```bash
./gradlew bundleProdRelease -PAPI_BASE_URL="https://mcapi.sanjeebojha.com.np/api/"
```

**Output Artifact**:
- `MarriageCalculator/Android/app/build/outputs/bundle/prodRelease/app-prod-release.aab`

### Step 4.2: Build Standalone Signed APK (`prodRelease`)
To build a standalone signed APK for direct tester sideloading:
```bash
cd MarriageCalculator/Android
./gradlew assembleProdRelease
```

**Output Artifact**:
- `MarriageCalculator/Android/app/build/outputs/apk/prod/release/app-prod-release.apk`

### Step 4.3: Build Local / Dev Testing APKs
```bash
# Local environment (running against local API on host)
./gradlew assembleLocalDebug

# Dev environment (running against Kubernetes dev cluster)
./gradlew assembleDevDebug
```

---

## 5. Google Play Console Setup: Internal Testing Track

The **Internal Testing** track allows up to 100 invited testers to install and test updates within seconds of uploading, with zero Play Store review delays.

### Step 5.1: Create the Application
1. Go to [Google Play Console](https://play.google.com/console).
2. Click **Create app**:
   - **App name**: `AAA Marriage Calculator`
   - **Default language**: English (United States)
   - **App or game**: Game
   - **Free or paid**: Free
3. Accept the declarations and click **Create app**.

### Step 5.2: Enable Google Play App Signing
1. Navigate to **Release > Setup > App signing**.
2. Choose **Use Google-generated key** or upload your private key.
3. Once enabled, copy the **SHA-1 certificate fingerprint** from the **App signing key certificate** and the **Upload key certificate**.

### Step 5.3: Add SHA-1 to Firebase & Google Cloud (For Google Sign-In)
For Google Sign-In to work on tester devices installed from Google Play:
1. Open the [Firebase Console](https://console.firebase.google.com/) > Project `marriagecalculator-197bd`.
2. Go to **Project Settings > General > Your apps > np.com.sanjeeb.marriagecalculator**.
3. Click **Add fingerprint** and paste:
   - Your local debug SHA-1 (already present)
   - Your `upload-keystore.jks` SHA-1
   - The **Google Play App Signing SHA-1** (critical for Play Store installs)
4. Download the updated `google-services.json` if new OAuth client IDs are generated.

### Step 5.4: Create an Internal Test Release
1. In Play Console, navigate to **Testing > Internal testing**.
2. Click **Create new release**.
3. Upload `app-prod-release.aab` (located at `MarriageCalculator/Android/app/build/outputs/bundle/prodRelease/app-prod-release.aab`).
4. Set Release name: `1.0.2 (3)`.
5. Enter Release notes:
   ```
   AAA Marriage Calculator v1.0.2:
   - Joined Games Dashboard: View active and past games you are participating in with read-only score tracking
   - Player Creation: Added live avatar photo preview when selecting images
   - Game Rules: Dublee option now restricted to games with 4 or more players with corrected terminology
   - Backend: Authentication audit logging and OAuth account resolution improvements
   ```
6. Click **Next**, review summary, and click **Start rollout to Internal testing**.

---

## 6. Inviting Testers & Distribution

1. In Play Console, under **Internal testing**, switch to the **Testers** tab.
2. Create an email list (e.g. `marriage-calculator-internal-testers`) and add tester Gmail addresses.
3. Copy the **Join on the web** or **Join on Android** invitation link.
4. Share the link with your testers. Once they accept the invite, they can download the app directly from the Google Play Store on their Android devices.

---

## 7. Tester Smoke-Test Checklist

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
