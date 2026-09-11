# BrightPath Learning Center — Scheduling & Clash Detection API

## Executive Summary

BrightPath Learning Center is a small tutoring centre that schedules roughly 200 families and 12 tutors out of one shared spreadsheet. That spreadsheet has no guardrails: a lesson can be typed in that double-books a student, a tutor, or a room, and nobody notices until a family is standing at the front desk.

This project is the first slice of the replacement: a **scheduling API that enforces clash detection at write time**. A lesson slot is unique on three axes — **student, tutor, and room** — and any overlapping booking is refused rather than accepted and cleaned up later. In the seed week, 13 of 34 lessons (38.2%) are touched by a clash or an overloaded tutor day. This service closes the three clash classes with a single rule:

> Two lessons conflict when their time slots overlap **and** they share a student, a tutor, or a room.

Slots are half-open, so back-to-back lessons are allowed (`startA < endB && startB < endA`). Cancelled lessons free their resources; no-shows do not.

The result is a single endpoint, `POST /lessons`, that validates the request, detects clashes against the existing schedule, and returns `201 Created` on success or `409 Conflict` with the exact lessons and resource types that collide.

### Design principles

- **One rule, one code path.** The clash predicate lives in one place (`Lesson.ConflictTypesWith`) and is the single source of truth for both "does it clash?" and "on what?".
- **Enforce, don't flag.** A conflicting booking is rejected with HTTP `409`, not silently accepted with a warning.
- **Clean Architecture.** Domain has no framework references; repositories are pure data access; business rules stay in Domain services.
- **Fail visibly.** Pre-existing violations in the seed data are imported rather than hidden, because a tool that cannot open Monday's real data is useless on Monday.

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Features](#features)
3. [Technology Stack](#technology-stack)
4. [Architecture](#architecture)
5. [Repository Layout](#repository-layout)
6. [Installation](#installation)
   - [Prerequisites](#prerequisites)
   - [Clone and Restore](#1-clone-and-restore)
   - [Apply the Database Migration](#2-apply-the-database-migration)
   - [Run the API](#3-run-the-api)
   - [Run with Docker](#optional-run-with-docker)
7. [Configuration](#configuration)
8. [API Reference](#api-reference)
9. [How Clash Detection Works](#how-clash-detection-works)
10. [Testing & Coverage](#testing--coverage)
11. [Development Workflow](#development-workflow)
12. [Design Decisions](#design-decisions)
13. [Known Limitations & Roadmap](#known-limitations--roadmap)

---

## Features

- **Clash detection on write** — rejects any lesson that overlaps an existing one on the same student, tutor, or room.
- **Server-generated lesson ids** — new lessons receive the next sequential `L###` id (e.g. `L035` after the seed week's `L034`); clients never supply ids.
- **Request validation** — FluentValidation rejects missing fields and durations outside `1..1440` minutes.
- **Rich conflict responses** — `409` responses list each colliding lesson and *why* (`student`, `tutor`, `room`).
- **UTC-normalized instants** — offsets are normalized to UTC on the way in and re-applied at the presentation layer.
- **Automatic seeding** — the real week export is embedded and loaded on first development start.
- **Interactive API docs** — OpenAPI plus a Scalar UI in development.

## Technology Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (C# 14), SDK pinned in `global.json` (`10.0.400`) |
| API style | ASP.NET Core Minimal APIs |
| Validation | FluentValidation |
| Persistence | EF Core 10 with SQLite (local development) |
| API docs | `Microsoft.AspNetCore.OpenApi` + Scalar |
| Testing | xUnit.v3, `Microsoft.AspNetCore.Mvc.Testing`, coverage via Coverlet + ReportGenerator |
| Architecture | Clean Architecture (`Api → Infrastructure → Domain`) |

## Architecture

```
                ┌────────────────────────────┐
                │  BrightPathLearningCenter  │
                │            .Api            │  Minimal APIs, middleware, DTOs,
                │  (endpoints, validators,   │  DI wiring, wire mappings
                │   request/response DTOs)   │
                └────────────┬───────────────┘
                             │ depends on
                             ▼
                ┌────────────────────────────┐
                │        .Api.Infrastructure │  EF Core DbContext, migrations,
                │  (DbContext, repositories, │  repository implementations,
                │     seed, migrations)      │  embedded CSV seeding
                └────────────┬───────────────┘
                             │ depends on
                             ▼
                ┌────────────────────────────┐
                │         .Api.Domain        │  Entities, value objects,
                │  (Lesson, TimeSlot, rules, │  domain services, repository
                │    factory, clash detector)│  interfaces — no framework refs
                └────────────────────────────┘
```

Dependency direction is strictly `Api → Infrastructure → Domain`. Domain never references Infrastructure or Api.

**Key components**

- `Lesson` / `TimeSlot` — domain records. `Lesson.ConflictTypesWith` is the single source of truth for conflicts.
- `LessonClashDetector` — orchestrates detection across a candidate and a set of lessons.
- `LessonFactory` — creates and restores lessons, normalizing instants to UTC.
- `LessonRepository` — thin EF Core data access (read all, next id, add).
- `CreateLessonRequestValidator` — FluentValidation rules for the request contract.

## Repository Layout

```
BrightPathLearningCenter/
├── src/
│   ├── BrightPathLearningCenter.Api/             # Entry point, endpoints, DI, DTOs, validators, Dockerfile
│   │   ├── Contracts/                            # CreateLessonRequest, LessonResponse, LessonConflictResponse
│   │   ├── Endpoints/                            # LessonEndpoints (POST /lessons)
│   │   ├── Mappings/                             # Domain → wire mappings
│   │   └── Validators/                           # FluentValidation
│   ├── BrightPathLearningCenter.Api.Domain/      # Entities, value objects, services + interfaces
│   │   ├── Models/                               # Lesson, TimeSlot, ConflictType, LessonStatus
│   │   ├── Services/                             # LessonFactory, LessonClashDetector
│   │   ├── Extensions/                           # LessonExtensions, TimeSlotExtensions
│   │   └── Abstractions/                         # ILessonRepository
│   └── BrightPathLearningCenter.Api.Infrastructure/
│       ├── Persistence/                          # SchedulingDbContext, factory
│       ├── Repositories/                         # LessonRepository
│       ├── Seed/                                 # LessonSeeder, LessonCsvParser, CSV exports
│       └── Migrations/                           # EF Core migrations
├── tests/
│   ├── BrightPathLearningCenter.Api.Tests/             # HTTP integration tests
│   ├── BrightPathLearningCenter.Api.Domain.Tests/      # Domain unit tests
│   └── BrightPathLearningCenter.Api.Infrastructure.Tests/  # Repository / seeding tests
├── AGENTS.md
├── DECISIONS.md
├── global.json
├── coverlet.runsettings
└── BrightPathLearningCenter.slnx
```

## Installation

### Prerequisites

- **.NET SDK 10.0.400** (or a 10.0.x feature band compatible with `global.json`). Verify with:

  ```bash
  dotnet --version
  ```

- **Git**.
- Optional: **Docker** (for the container workflow).

### 1. Clone and restore

```bash
git clone https://github.com/khoinguyxn/bright-path-learning-center.git
cd BrightPathLearningCenter

# Restore local tools (dotnet-ef, reportgenerator) from .config/dotnet-tools.json
dotnet tool restore

# Restore NuGet packages
dotnet restore
```

### 2. Apply the database migration

SQLite is used for local development. The connection string defaults to `Data Source=brightpath.db` (a file created in the API project folder).

```bash
dotnet ef database update \
  --project src/BrightPathLearningCenter.Api.Infrastructure \
  --startup-project src/BrightPathLearningCenter.Api
```

> In `Development`, the API also applies pending migrations automatically on startup, so this step is convenient but optional for local runs.

### 3. Run the API

```bash
dotnet watch --project src/BrightPathLearningCenter.Api run
```

The HTTP profile listens on `http://localhost:5217`; the HTTPS profile uses `https://localhost:7069` (see `Properties/launchSettings.json`).

On first development start the database is created, migrated, and seeded with the real week export (`lessons_export.csv`). Open the interactive API reference at:

- Scalar UI: `http://localhost:5217/scalar`
- OpenAPI document: `http://localhost:5217/openapi/v1.json`

### Optional: Run with Docker

Build the image from the repository root (the Dockerfile expects the repo root as build context):

```bash
docker build -f src/BrightPathLearningCenter.Api/Dockerfile -t brightpath-api .
docker run --rm -p 8080:8080 brightpath-api
```

The container exposes ports `8080` and `8081`. Note that the container runs in the `Production` environment, so migrations are **not** applied automatically; point it at an initialized database or run migrations explicitly.

## Configuration

Configuration is read from `appsettings.json`, `appsettings.{Environment}.json`, environment variables, and user secrets.

| Key | Default | Description |
|---|---|---|
| `ConnectionStrings:Scheduling` | `Data Source=brightpath.db` | EF Core SQLite connection string. |
| `ASPNETCORE_ENVIRONMENT` | `Development` (via launch profile) | Enables OpenAPI/Scalar, auto-migration, and seeding. |
| `Logging:LogLevel` | `Information` | Standard ASP.NET Core logging. |

**Secrets.** Do not commit real connection strings, API keys, or tokens. Use user secrets locally:

```bash
dotnet user-secrets init --project src/BrightPathLearningCenter.Api
dotnet user-secrets set "ConnectionStrings:Scheduling" "<your-connection-string>" \
  --project src/BrightPathLearningCenter.Api
```

## API Reference

### `POST /lessons`

Create a lesson. The server assigns the id and rejects the request if it clashes with an existing lesson.

**Request body** (`application/json`)

```json
{
  "student": "Le Minh Chau",
  "tutorId": "T1",
  "roomId": "R1",
  "startsAt": "2026-03-11T09:00:00+07:00",
  "durationMinutes": 60,
  "note": "new booking"
}
```

| Field | Type | Rules |
|---|---|---|
| `student` | string | Required, non-empty. |
| `tutorId` | string | Required, non-empty (e.g. `T1`). |
| `roomId` | string | Required, non-empty (e.g. `R1`). |
| `startsAt` | date-time | Required; normalized to UTC. |
| `durationMinutes` | integer | `1..1440`. |
| `note` | string? | Optional. |

**`201 Created`** — instants are stored as UTC and re-expressed in the centre's `+07:00` offset; `endsAt` is derived from the duration.

```json
{
  "id": "L035",
  "student": "Le Minh Chau",
  "tutorId": "T1",
  "roomId": "R1",
  "startsAt": "2026-03-11T09:00:00+07:00",
  "endsAt": "2026-03-11T10:00:00+07:00",
  "durationMinutes": 60,
  "status": "booked",
  "note": "new booking"
}
```

**`400 Bad Request`** — one or more validation failures (`application/problem+json` with an `errors` map).

**`409 Conflict`** — the booking overlaps an existing lesson on the same student, tutor, or room:

```json
{
  "title": "Booking clashes with an existing lesson",
  "detail": "Lesson L035 overlaps 1 existing lesson(s) on the same student, tutor or room.",
  "status": 409,
  "conflicts": [
    { "lessonId": "L009", "types": ["tutor", "room"] }
  ]
}
```

`types` values are `student`, `tutor`, and `room`.

A ready-to-run request is available in `src/BrightPathLearningCenter.Api/BrightPathLearningCenter.http`.

## How Clash Detection Works

All conflict logic is centralized in `LessonExtensions.ConflictTypesWith`, which is the single source of truth:

1. **Both lessons must be active.** A `Cancelled` lesson frees its slot; `NoShow` does not.
2. **The slots must overlap.** Half-open interval: touching end-to-start is allowed.
3. **They must share a resource.** Each shared `Student`, `TutorId`, or `RoomId` contributes a `ConflictType`.

```csharp
if (!lesson.IsActive() || !other.IsActive() || !lesson.Slot().Overlaps(other.Slot()))
{
    return [];
}

// ... StudentDoubleBooked / TutorDoubleBooked / RoomDoubleBooked
```

- `ClashesWith(other)` is a thin wrapper: `ConflictTypesWith(other).Count > 0`.
- `LessonClashDetector.Detect` excludes the candidate itself and collects all clashing lessons through the same method — it neither re-implements nor duplicates the rules.

## Testing & Coverage

The solution uses xUnit.v3 across three test projects:

```bash
# Full suite
dotnet test

# Scoped run while iterating
dotnet test --filter "FullyQualifiedName~LessonClashDetector"
```

Test projects:

- `BrightPathLearningCenter.Api.Domain.Tests` — pure domain rules (clash predicate, overlap boundaries, factory normalization).
- `BrightPathLearningCenter.Api.Infrastructure.Tests` — repository and CSV parsing/seeding behavior.
- `BrightPathLearningCenter.Api.Tests` — HTTP integration tests using `WebApplicationFactory` against a throwaway SQLite file.

**Coverage** (Coverlet + ReportGenerator):

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
reportgenerator \
  -reports:"**/TestResults/**/coverage.cobertura.xml" \
  -targetdir:coveragereport \
  -reporttypes:Html
```

Generated migrations and `[ExcludeFromCodeCoverage]` types are excluded via `coverlet.runsettings`.

## Development Workflow

Before considering any change done, run the full **Build, Test, Lint** block:

```bash
dotnet build                        # must succeed with zero warnings-as-errors
dotnet format --verify-no-changes   # style/analyzer check; run `dotnet format` to fix
dotnet test                         # full suite
```

Conventions used across the repo:

- File-scoped namespaces, one top-level type per file, type name matches file name.
- Nullable reference types enabled; prefer `ArgumentNullException.ThrowIfNull` over `!`.
- Async all the way down; suffix async methods with `Async`.
- Records for immutable DTOs/value objects; classes for entities with identity.
- Repositories are pure data access — business rules live in Domain.
- No magic strings/numbers for business rules.

Create a migration after a model change:

```bash
dotnet ef migrations add <DescriptiveName> \
  --project src/BrightPathLearningCenter.Api.Infrastructure \
  --startup-project src/BrightPathLearningCenter.Api
```

Commit message format: `<type>: <what and why>` (e.g. `fix: prevent tutor double-booking on overlapping intervals`).

## Design Decisions

The full rationale lives in [`DECISIONS.md`](./DECISIONS.md). Highlights:

- **Reject, don't flag.** `409` with no override — "if the system allows it, the system is broken."
- **Student is a third uniqueness axis.** The owner's headline complaint is a student in two places at once, even though the written rules only constrained tutors and rooms.
- **Exam pairs count as two bookings.** An ambiguity in the rules is enforced conservatively rather than encoded as a permanent exception flag.
- **Overlap and counts live in code, not database constraints.** The student column and the cancelled/no-show asymmetry need conditional logic; splitting one rule across two enforcement points is how the halves drift apart.
- **Cancelled frees the slot; no-show does not.**
- **Times stored as UTC**, offset re-applied at the presentation layer.

## Known Limitations & Roadmap

Deliberately out of scope for this slice (see `DECISIONS.md` §5–6):

- **Daily tutor cap of six** is not enforced.
- **Closed-day bookings** (e.g. Mondays) are not blocked.
- **Change log / audit trail** is not implemented.
- **Day view / UI** — this is an API only; nothing renders yet.
- **Reschedule validation** is not implemented.
- The seed database intentionally contains pre-existing violations so the tool can open the real week's data.
