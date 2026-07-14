# Marriage Calculator — Claude Instructions

**READ FIRST, before any task:**

1. `.agent/agent.md` — core instruction set: architecture, folder structure, coding standards, build/verification commands, spec-driven development rules.
2. `.agent/memory.md` — session memory / business rules.
3. `spec/requirement.md` and `spec/plan.md` — active specification and plan status. All work is spec-driven; update `plan.md` as phases advance.

## Quick facts (do not assume otherwise)

- Backend: ASP.NET Core 8 Web API (`MarriageCalculator/MarriageCalculator.API`)
- Database: MongoDB at `192.168.0.229` (migrating from SQL Server)
- Mobile: Native Android/Kotlin (`MarriageCalculator/Android`)
- Shared models: `MarriageCalculator.Core` — pure .NET 8, NO dependencies
- **Firebase is Auth (Google sign-in) + FCM notifications ONLY — never the backend or database. Do not propose Cloud Functions or Firestore.**
- App is NOT on Google Play Store yet (direct APK distribution)
- `archive.MarriageCalculator.MAUI` is frozen — do not modify
- New docs go in `documentations/`; no loose .md files in code folders
- After code changes: run the build command for that project (see agent.md §5)
