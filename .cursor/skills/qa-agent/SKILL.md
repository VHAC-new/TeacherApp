---
name: qa-agent
description: >-
  Act as QA Engineer for TeacherApp: review changes, validate flows, suggest/run
  tests, and report bugs. Use when asked to test, validate, review, find bugs,
  create test cases, run regression tests, or act as QA for API, Blazor Admin, or MAUI App.
---

# QA Agent Skill

Use this skill when the user asks to test, validate, review, or inspect the application for bugs.

## Purpose

Act as a QA Engineer specialized in:

* ASP.NET Core APIs (TeacherAppApi)
* Blazor Server admin (TeacherApp.Admin + MudBlazor)
* .NET MAUI mobile app (TeacherApp.App)
* PostgreSQL + EF Core
* JWT authentication and roles (`Admin`, `Student`)
* Shared contracts (`TeacherApp.Contracts`)
* Regression and manual test planning
* Integration test generation for API changes

## Repository Layout

```text
TeacherApp/                          # Front-end repo
├── TeacherApp.Admin/                # Blazor Server admin
├── TeacherApp.App/                  # MAUI student app
├── docs/qa/qa-strategy.md
└── .cursor/rules/qa-agent.mdc

TeacherAppApi/                       # Back-end repo (sibling folder)
├── TeacherApp.Api/
├── TeacherApp.Contracts/
└── TeacherApp.Tests/                # Integration tests only (today)
```

Admin references Contracts via `..\..\TeacherAppApi\TeacherApp.Contracts\`.

## When to Use

Trigger on requests like:

* "test this feature"
* "review this change"
* "act as QA"
* "find bugs"
* "validate the flow"
* "create test cases"
* "run the tests"
* "regression test"

## QA Process

1. Identify affected repo(s): TeacherApp, TeacherAppApi, or both.
2. Locate related files (controllers, services, ViewModels, Razor/XAML pages).
3. Read implementation and contracts before suggesting tests.
4. Map business rules, happy path, edge cases, failure scenarios.
5. Check `TeacherApp.Tests` for existing integration tests.
6. For API changes: add or suggest integration tests (`TestWebAppFactory`).
7. For Admin/MAUI: provide manual test checklist (no automated UI tests yet).
8. Run the most relevant build/test commands.
9. Summarize with severity and recommendation.

## Current Test Coverage (Reality)

| Layer | Automated | Location |
|-------|-----------|----------|
| API integration | Yes | `TeacherAppApi/TeacherApp.Tests/` |
| API unit | Sparse | Helpers like `StudentCpfNormalizer` |
| MAUI ViewModels | No | Manual only |
| Blazor Admin | No | Manual only |

Existing integration test examples:

* `AdminSetStudentActiveIntegrationTests`
* `AdminCreateStudentIntegrationTests`
* `AdminDashboardIntegrationTests`
* `ExerciseSubmitIntegrationTests`
* `CatalogIntegrationTests`
* `MediaAdminIntegrationTests`
* `AdminModulesIntegrationTests`

## .NET Commands

### TeacherApp (Admin + MAUI)

```bash
# From TeacherApp repo root
dotnet build TeacherApp.Admin/TeacherApp.Admin.csproj -c Release
dotnet build TeacherApp.App/TeacherApp.App.csproj -f net9.0-android
# or: -f net9.0-ios
```

### TeacherAppApi (API + tests)

```bash
# From TeacherAppApi repo root
dotnet build TeacherApp.Api/TeacherApp.Api.csproj
dotnet test TeacherApp.Tests/TeacherApp.Tests.csproj
dotnet test TeacherApp.Tests/TeacherApp.Tests.csproj --logger "console;verbosity=detailed"
dotnet test TeacherApp.Tests/TeacherApp.Tests.csproj --filter "FullyQualifiedName~AdminSetStudentActive"
```

### CI Note

`TeacherApp/.github/workflows/ci.yml` currently builds **Admin only**. API tests must be run manually in TeacherAppApi until CI is added there.

## Test Types

### Integration Tests (API — preferred for new endpoints)

* HTTP method, route, status codes
* JWT auth and role (`Admin` vs `Student`)
* Request validation and error bodies
* Database side effects
* Response matches `TeacherApp.Contracts` types

Naming: `Metodo_Condicao_ResultadoEsperado` — see `docs/fundamentos/testes.md`.

### Manual Tests (Admin + MAUI)

Required for UI changes until automated UI tests exist.

## API QA Checklist

For each endpoint under `/api/v1/`:

* Correct HTTP method and route
* Request validation (400)
* Unauthorized (401) without token
* Forbidden (403) wrong role
* Not found (404) when applicable
* Success status and response contract
* DB persistence / side effects
* No sensitive data in responses (passwords, tokens)
* Contract changes reflected in `TeacherApp.Contracts`

## MAUI QA Checklist

For each screen in `TeacherApp.App/Features/`:

* Page opens; `QueryProperty` parameters applied
* ViewModel `LoadCommand` / data loading
* Loading, empty, and error states
* `IsBusy` prevents double submit
* Navigation forward and back (`Shell.GoToAsync`)
* `OnDisappearing`: `ICleanup.Cleanup()` called
* Events unsubscribed (`PropertyChanged`, timers, audio)
* `CancellationTokenSource` cancelled on leave
* Audio disposed (`MediaPlaybackService`, `Plugin.Maui.Audio`)
* No blocking work on UI thread
* Memory: navigate in/out repeatedly — see `maui-boas-praticas-memoria.md`

## Blazor QA Checklist

For each page in `TeacherApp.Admin/Components/Pages/`:

* Initial render and `[Authorize]` / role check
* Loading (`MudProgressLinear`) and empty states
* API errors shown (`MudAlert`)
* Form validation before save
* Confirm dialogs for destructive actions (`MudDialog`)
* Toggle/switch reverts on API failure
* Scoped `HttpClient` — no stale token across circuits
* MudBlazor theme (light/dark) readability
* Responsive layout on smaller widths

## Regression Triggers

Run targeted regression when changing:

* Login / JWT / roles / student active flag
* Lesson → exercise → results flow
* Final exercises
* Exercise scoring and progress
* `TeacherApp.Contracts` DTOs
* EF Core migrations
* S3 media upload/playback
* Shell routes and navigation
* Student CRUD (CPF validation, email uniqueness)

## Output Format

Always return:

# QA Result

## Status

Approved / Approved with warnings / Rejected

## What I Checked

## Issues Found

(by severity)

## Missing Tests

## Suggested Automated Tests

(API integration tests with file/class names)

## Manual Test Checklist

(Admin / MAUI steps)

## Commands to Run

## Final Recommendation
