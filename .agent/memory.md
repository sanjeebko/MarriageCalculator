# Marriage Calculator - Application Domain Memory

This document stores application-specific and domain-specific knowledge for the Marriage Calculator project.

## 1. Domain Overview (Context)
**Marriage Calculator** is a utility for scoring the "Marriage" card game, popular in Nepal/South Asia.
*   **Core Mechanic**: Calculating scores based on "Maal" (Points), Game status (Winner/Runner), and "Foul" points.
*   **Scoring Algorithm**: "Central Collection" - Winner collects all penalties first, then distributes Maal payouts.
*   **Decks**: 3 Decks + 3 Manuk (Jokers).
*   **Cards Handled**: 21 Cards per player.

## 2. Key Business Rules & Game Logic
*   **Game Variations**:
    *   **Normal**: Unseen players count Maal to reduce penalty.
    *   **Kidnap**: Winner "steals" Unseen player's Maal points (add to Winner, subtract from Loser).
    *   **Murder**: Unseen player's Maal is voided (Zeroed). Winner does *not* get them.
*   **Scoring Components**:
    *   **Game Points**: Fixed penalty paid to Winner (e.g., 3 for Seen, 10 for Unseen).
    *   **Maal Points**: Variable exchange based on card combinations.
    *   **Payment**: Losers pay Winner; diff of Maal is settled between all "Seen" players.
*   **Money Management**:
    *   Session Settlement: "Freeze" and settle cash at any time.
    *   Currency: Real cash tracking (NPR, INR, USD, etc.).

## 3. Architecture & Tech Constraints
*   **Core**: Pure .NET 8. No DB/UI dependencies.
*   **Mobile**: Native Android (Kotlin). Replaces .NET MAUI.
*   **Backend**: .NET 8 API.
    *   **DB Migration**: Moving from SQL Server to **MongoDB** (Hosted at `192.168.0.229`).
    *   **Auth**: Firebase (Google Sign-In).
    *   **Social**: Friend System, Invite-only games.
    *   **Notifications**: FCM for "Nudge" feature (Offline push to Join/Resume).

## 4. Design & Aesthetic Patterns
*   **Theme**: **Dashain & Tihar Festival** (Vibrant, Culturally Rich).
*   **Palette**:
    *   Primary: Deep Red (Tika) & Marigold Orange.
    *   Background: Deep Night Blue or Rich Maroon.
    *   Accents: Gold (Wealth), Fresh Green (Jamara).
*   **Visual Language**:
    *   **Glassmorphism**: For cards and input areas.
    *   **3D/Glossy**: For Icons and interactive elements.
    *   **Cultural Motifs**: Mandalas (Seating), Diyos (Lamps), Kites, Dhaka patterns.
*   **UX Goals**: Speed (Faster than paper), Clarity (Who owes whom), Premium Feel.

## 5. Development Workflow
*   **Android**: Use `gradlew` for builds.
*   **API**: Standard `dotnet` CLI.
*   **Docker**: API is containerized.

## 6. Recent Feature Context
*   **Frontend Switch**: Dropped MAUI for Native Android.
*   **Database Switch**: Dropped SQL Server for MongoDB.
*   **Connectivity**: Offline (Guest) vs Online (Synced) modes.
*   **Seating**: Circular "Mandala" arrangement logic (Dealer rotation).


## 7. Environment Quirks
*   **MongoDB**: Hosted on LAN (`192.168.0.229`). Ensure API can reach this IP.
*   **Android**: New project in `MarriageCalculator/Android`.

## 8. Asset Generation Rules
*   **Images**: All generated images must be stored in `.agent/temp` for user approval before being applied to the actual project files.
