# AGENTS.md

## Project Overview

BrightPathLearningCenter is a scheduling and conflict-detection tool for a tutoring centre, built on .NET 10 (C# 14) using Minimal APIs, Fluent Validation, Xunit.v3 @3.2.2, EF Core 10 (SQLite for local dev) following the Clean Architecture pattern.

```
src/
  BrightPathLearningCenter.Api/             # Entry point, endpoints, DI wiring, middleware, DTOs
  BrightPathLearningCenter.Api.Domain/          # Entities, value objects, domain services + interfaces — no framework refs
  BrightPathLearningCenter.Api.Infrastructure/  # EF Core DbContext, external service clients, repositories
tests/
  BrightPathLearningCenter.Api.Tests/
  BrightPathLearningCenter.Api.Domain.Tests/
  BrightPathLearningCenter.Api.Infrastructure.Tests/
```

Dependency direction: `Api → Infrastructure → Domain`. Domain never references Infrastructure or Api. If you're adding a `using` that violates this, stop and reconsider the layer.
`Api` needs to reference `Domain` and `Infrastructure`.
`Infrastructure` needs to reference `BrightPathLearningCenter.Api.Domain`.
---

## Setup Commands

```bash
# Restore tools and packages (reads global.json + .config/dotnet-tools.json)
dotnet tool restore
dotnet restore

# Apply EF Core migrations to local dev DB
dotnet ef database update --project src/BrightPathLearningCenter.Infrastructure --startup-project src/BrightPathLearningCenter.Api

# Run the API with hot reload
dotnet watch --project src/BrightPathLearningCenter.Api run
```

Requires the SDK version pinned in `global.json` (**10.0.x**) — do not bump this file without checking CI images support it.

---

## Build, Test, Lint

Run these before considering any task done. All must pass — don't report success on a red build.

```bash
dotnet build                        # must succeed with zero warnings-as-errors
dotnet format --verify-no-changes   # style/analyzer check; run `dotnet format` (no flag) to fix
dotnet test                         # full suite
dotnet test --filter "FullyQualifiedName~<Namespace>"   # scoped run while iterating
```

- New code needs new tests. A bug fix needs a regression test that fails before the fix and passes after.
- Don't reduce test coverage to make a build pass. Don't delete or skip (`[Fact(Skip=...)]`) a failing test to get green — fix the code or fix the test.
- If `dotnet format` changes files you didn't intend to touch, review the diff before committing; don't blanket-revert formatting on files you did mean to change.

---

## Code Style

- **Nullable reference types are enabled** (`<Nullable>enable</Nullable>`). Don't suppress with `!` unless you've actually verified non-null; prefer proper null checks or `ArgumentNullException.ThrowIfNull`.
- File-scoped namespaces, one top-level type per file, type name matches file name.
- Prefer primary constructors and collection expressions (`[]` / `[..]`) where they're clearer — don't force them where a body is genuinely clearer.
- Async all the way down: no `.Result` / `.Wait()` on a `Task`. Suffix async methods with `Async`.
- Use `ErrorOr<T>` (or `<your Result type>`) for expected failure paths in the Domain/Application layer; reserve exceptions for actually-exceptional cases.
- Records for immutable DTOs and value objects; classes for entities with identity.
- No magic strings/numbers for business rules — named constants or config, colocated with the rule they express.
- Match the `.editorconfig` at the repo root; it is the tiebreaker for anything not listed here.

---

## Database & Migrations

- Never hand-edit a generated migration under `Migrations/`. If a migration is wrong, add a new one — don't rewrite history that may already be applied elsewhere.
- Every migration must have a paired down-migration that actually reverses it. Test `dotnet ef database update <PreviousMigration>` before considering a migration done.
- Seed data changes belong in the seeding class, not inline in a migration.

```bash
dotnet ef migrations add <DescriptiveName> --project src/BrightPathLearningCenter.Infrastructure --startup-project src/BrightPathLearningCenter.Api
dotnet ef migrations remove --project src/BrightPathLearningCenter.Infrastructure --startup-project src/BrightPathLearningCenter.Api   # only if NOT yet applied/pushed
```

---

## Security & Secrets

- Never commit connection strings, API keys, or tokens. Local secrets go in `dotnet user-secrets`, not `appsettings.json`.
- If a task involves auth, don't loosen an authorization policy to make a test pass — fix the test's setup instead.
- Don't add packages from unfamiliar sources; stick to nuget.org and packages already in use elsewhere in the repo.

---

## PR / Commit Instructions

- One logical change per commit. Message format: `<type>: <what and why>` (e.g. `fix: prevent tutor double-booking on overlapping intervals`).
- Run the full **Build, Test, Lint** block above before opening a PR — not just the tests you think are related.
- PR description should state what changed, why, and what you deliberately did not do (scope left out), not just a diff summary.
- Don't squash or rewrite history that's already been pushed for review.

---

## Boundaries — do not do these without asking

- Don't change the target framework, SDK version, or CI pipeline config.
- Don't add a new project/package reference across the Domain → Infrastructure boundary described above.
- Don't rename or move files outside the scope of the current task — it breaks the diff for the reviewer.
- Don't touch files under `<generated/ or /obj, /bin>` — regenerate them, don't hand-edit.
- Always ask for approval before making any of these changes.

---

## When You're Stuck

If a command in this file fails, or a convention here conflicts with what you observe in the code, trust the code and flag the mismatch rather than silently picking one — this file goes stale faster than the codebase does.