# AI Agent Instructions for Marriage Calculator Solution

This document serves as the long-term memory and instruction manual for AI agents working on the Marriage Calculator solution. It outlines the architecture, coding standards, folder structure, and key constraints.

## 1. Solution Overview

**Marriage Calculator** is a digital scorer and calculator for the popular "Marriage" card game.

### Projects:
1.  **MarriageCalculator.API (`MarriageCalculator/MarriageCalculator.API`)**:
    *   **Role**: Backend API.
    *   **Tech Stack**: ASP.NET Core 8.
    *   **Database**: MongoDB (Target). Currently migrating from SQL Server.
    *   **Config**: MongoDB Connection to `192.168.0.229`.
    *   **Docs**: Swagger available at root `/` in Development.

2.  **MarriageCalculator.Android (`MarriageCalculator/Android`)**:
    *   **Role**: Mobile Client (Primary).
    *   **Tech Stack**: Native Android (Kotlin).
    *   **Function**: Native mobile application.
    *   **Build**: Gradle.

3.  **MarriageCalculator.Core (`MarriageCalculator/MarriageCalculator.Core`)**:
    *   **Role**: Shared Library.
    *   **Tech Stack**: Pure .NET 8 Class Library.
    *   **Function**: Shared Models (POCOs), Enums, and Business Logic Extensions.
    *   **Constraint**: NO dependencies. Pure C#.

4.  **[Archived] MarriageCalculator.MAUI**:
    *   Located in `archive.MarriageCalculator.MAUI`.
    *   Status: Frozen. Do not modify.

---

## 2. Architecture

**Pattern**: Clean Architecture with Shared Core.

*   **Core**:
    *   Central hub for Domain Models.
    *   Technology-agnostic. Used by API.

*   **Backend (API)**:
    *   **Transition**: Migrating from EF Core/SQL Server to MongoDB.
    *   **Database**: MongoDB hosted at `192.168.0.229`.

*   **Frontend (Android)**:
    *   **Tech**: Kotlin, Android Views (XML).
    *   **Navigation**: Intent-based / Jetpack Navigation.

---

## 3. Folder Structure

### Root (Workspace)
*   `spec/`: Contains files related to specification and plan status.
    *   `requirement.md`: The main specification document for the application.
    *   `plan.md`: The active implementation plan showing phases and status.
*   `documentations/`: Contains all other general markdown documentation files (previously in the `MarriageCalculator` folder).
*   `MarriageCalculator/`: Source code projects.

#### `MarriageCalculator/MarriageCalculator.API/`
*   `Controllers/`: REST Endpoints.
*   `Services/`: Business logic.
*   `Repositories/`: Data access patterns (MongoDB adapter to be added).

#### `MarriageCalculator/Android/` (New Mobile App)
*   `app/src/main/java/`: Kotlin source.
*   `app/src/main/res/`: Resources (XML Layouts).

#### `MarriageCalculator/MarriageCalculator.Core/`
*   `Models/`: Shared Entity definitions.
*   `Extensions/`: Shared logic.

---

## 4. Coding Standards

### C# (Backend/Core)
*   **Clean Core**: `Core` MUST remain pure .NET 8.
*   **Async/Await**: Use for all I/O.

### Android (Kotlin)
*   **Style**: Kotlin Standard Library conventions.
*   **UI**: XML Layouts / ViewBinding (if enabled).

---

## 5. Development Environment & Verification

### Build Commands
*   **API**: `dotnet build MarriageCalculator/MarriageCalculator.API`
*   **Core**: `dotnet build MarriageCalculator/MarriageCalculator.Core`
*   **Android**: `./gradlew assembleDebug` (run in `MarriageCalculator/Android/`)

### Database Config
*   **Server**: `192.168.0.229` (MongoDB)
*   Ensure network connectivity to this IP from the API container/host.

### Verification
*   **Mandatory**: After modifying code, you MUST run the build command for the respective project.
*   **Auto-Run**: Authorized.

---

## 6. Spec-Driven Development

*   **Approach**: All development must follow spec-driven development (SDD) principles. Code changes must be driven by specifications defined in [requirement.md](file:///f:/workspace/games/MC/MarriageCalculator/spec/requirement.md).
*   **Plan Management**: Track implementation progress in [plan.md](file:///f:/workspace/games/MC/MarriageCalculator/spec/plan.md). Always update this file as implementation phases advance.
*   **Documentation Rule**: All general project documentation `.md` files must be managed in the [documentations/](file:///f:/workspace/games/MC/MarriageCalculator/documentations) directory at the root. No loose `.md` files should be added directly in code folders like `MarriageCalculator/`.

---

## 7. Memory Management
*   **Read**: `.agent/memory.md` at session start.
*   **Write**: Update if business rules change.
