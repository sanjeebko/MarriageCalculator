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

> **For this app the upload key and the Play app signing key are the same certificate.**
> Play Console's *App signing key certificate* shows the same SHA-1 and SHA-256 as our
> upload keystore (confirmed 2026-10-04). Google is not re-signing Store builds with a
> separate key, so the three-certificate trap described above does not currently apply —
> there is one certificate to register, and it is registered.
>
> This stops being true if Play App Signing is ever enrolled with a Google-generated key,
> or if the upload key is rotated. Re-check this table if either happens.

| Certificate | SHA-1 | Registered in Firebase |
|---|---|---|
| Upload key **— also the Play app signing key** | `57:60:B2:7E:90:75:18:13:0B:48:C9:24:60:6D:CF:EF:B8:8E:34:79` | Yes — confirmed present in Firebase, and verified working on a locally signed release build |
| Debug | _per developer machine_ | As needed for local work |

SHA-256 of the same certificate: `34:E3:8A:02:B5:A6:F4:2D:15:B8:13:A6:32:E3:61:16:69:1F:B8:4F:FE:84:B7:6C:64:BF:3B:0C:44:61:49:A9`

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

Install the app **from the Play Store** (internal testing track counts) and sign in with Google.

While the app signing key and the upload key remain the same certificate, a locally signed release build does exercise the same signature the Store delivers, so local verification is meaningful. That equivalence is a property of the current setup, not a general rule — if a Google-generated app signing key is ever introduced, only a Store install can prove the Store path.
