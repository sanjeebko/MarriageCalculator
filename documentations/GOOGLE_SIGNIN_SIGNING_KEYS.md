# Google Sign-In and Signing Certificates

Google Sign-In is bound to the **signing certificate** of the installed APK. If the certificate that signed the running build is not registered against the Firebase Android app, sign-in fails for that build only — the backend, the client ID and the code are all irrelevant to this failure.

This bites specifically on Play because **three different certificates** can be involved.

## The three certificates

| Certificate | Signs what | Where its SHA-1 comes from |
|---|---|---|
| **Debug** | `assembleDebug` on a developer machine | `~/.android/debug.keystore` |
| **Upload** | What we upload to Play (`marriagecalculatorr-upload.keystore`) | our keystore |
| **Play App Signing** | **What users actually install from the Store** | Play Console — generated and held by Google |

Play App Signing **re-signs** the uploaded bundle with Google's own key before delivering it. So the app a Store user installs is *not* signed with our upload key.

> **The failure mode this causes:** register only the upload key and Google Sign-In works perfectly in local builds, in internal testing, and in every manual check — then fails for every single Store user. Nothing in the code differs.

Every certificate whose builds need Google Sign-In must be registered in Firebase.

## Registered SHA-1 fingerprints

| Certificate | SHA-1 | Registered in Firebase |
|---|---|---|
| Upload key | `5760b27e907518130b48c924606dcfefb88e3479` | Yes — verified working on a locally signed release build |
| Play App Signing | _record it here once read from Play Console_ | **Must be confirmed** |
| Debug | _per developer machine_ | As needed for local work |

### Reading the Play App Signing SHA-1

Play Console → your app → **Test and release → Setup → App signing** → **App signing key certificate** → copy the SHA-1.

### Adding it to Firebase

Firebase Console → **Project settings** → **Your apps** → the `np.com.sanjeeb.marriagecalculator` Android app → **Add fingerprint** → paste the SHA-1 → Save.

No `google-services.json` change is needed for SHA-1 additions; the fingerprint is checked server-side.

### Reading our upload key's SHA-1

```bash
keytool -list -v \
  -keystore C:/workspace/keystore/mc/marriagecalculatorr-upload.keystore \
  -alias "$MC_ANDROID_KEY_ALIAS"
```

Or from a built APK:

```bash
"$ANDROID_HOME/build-tools/<version>/apksigner" verify --print-certs app-prod-release.apk
```

## Diagnosing a sign-in failure

Since #139 the app shows a persistent inline error rather than a Toast, and logs the exception under the `LoginScreen` tag. Match on the status code:

| Symptom | Means | Fix |
|---|---|---|
| `[10] DEVELOPER_ERROR` | Signing certificate or client ID is not registered | Add that certificate's SHA-1 to Firebase |
| `[16] Account reauth failed` | The **device's** Google account needs re-authentication | Re-add the Google account on the device. Not a certificate problem |
| `NoCredentialException` | No Google account on the device | Add one in Settings |

`[16]` is easy to misread as a certificate problem — it is not. It was observed on an emulator after repeated sign-in attempts across differently-signed builds, and affected the unmodified baseline build identically.

## Verifying after a Play release

Install the app **from the Play Store** (internal testing track counts — it is signed by the app signing key, unlike a locally installed APK) and sign in with Google. A locally built APK cannot prove this path, because it carries the upload key rather than Google's.
