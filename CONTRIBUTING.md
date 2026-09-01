# Contributing to RescueLink

Thank you for helping improve RescueLink. Contributions of code, tests, documentation, bug reports, and design feedback are welcome.

This guide explains how to choose an issue, prepare the project, follow the architecture, verify a change, and submit a pull request.

## Code of Conduct

Be respectful, constructive, and patient. Focus reviews and discussions on the work rather than the person. Harassment, discrimination, spam, and disclosure of private or sensitive information are not acceptable.

## Before You Start

### Choose an issue

Start with the [open issues](https://github.com/ismailbarankarasu/RescueLink/issues).

- `good first issue` contains focused tasks suitable for a first contribution.
- `help wanted` identifies work where community contributions are especially welcome.
- Read the complete description, acceptance criteria, and out-of-scope section.
- Check that nobody is already assigned and that no linked pull request is in progress.

### Claim the issue

Before writing code:

1. Comment on the issue.
2. Briefly describe your intended implementation and tests.
3. Wait for confirmation and assignment.
4. Ask questions before expanding or changing the requested scope.

Do not open competing pull requests for an assigned issue without discussing it first.

## Prerequisites

For local development, install:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git
- SQL Server 2022, SQL Server Express, or LocalDB
- Docker Desktop for Docker Compose and SQL Server Testcontainers integration tests
- An editor such as Visual Studio 2022, Visual Studio Code, or JetBrains Rider
- Optional: Postman or another HTTP client

Verify the SDK:

```bash
dotnet --version
```

The result should be a compatible .NET 10 SDK.

## Fork and Clone

Fork the repository on GitHub, then clone your fork:

```bash
git clone https://github.com/YOUR-USERNAME/RescueLink.git
cd RescueLink
```

Add the original repository as `upstream`:

```bash
git remote add upstream https://github.com/ismailbarankarasu/RescueLink.git
git remote -v
```

Before starting new work, update `master`:

```bash
git fetch upstream
git switch master
git pull --ff-only upstream master
git push origin master
```

## Create a Branch

Create one branch per issue from the latest `master`.

Recommended examples:

```bash
git switch -c feature/21-sighting-history
git switch -c fix/15-public-count-filter
git switch -c docs/6-contributing-guide
git switch -c test/matching-boundaries
```

Use a short lowercase name with hyphens. Do not develop directly on `master`.

## Configuration

### Local SQL Server

Use configuration appropriate for your machine. Apply migrations from the repository root:

```bash
dotnet ef database update \
  --project src/RescueLink.Persistence \
  --startup-project src/RescueLink.API
```

If `dotnet ef` is unavailable, install a .NET 10-compatible EF Core CLI tool.

### JWT secret

Never commit a real signing key. Configure it with User Secrets:

```bash
dotnet user-secrets set "Jwt:SecretKey" "a-local-development-key-at-least-32-bytes-long" \
  --project src/RescueLink.API
```

Use development-only values locally.

### Docker Compose

Copy the committed template and provide local secrets:

```bash
cp .env.example .env
docker compose up -d --build
```

PowerShell:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

The real `.env` file is ignored and must never be committed.

## Build and Run

From the repository root:

```bash
dotnet restore RescueLink.slnx
dotnet build RescueLink.slnx
dotnet run --project src/RescueLink.API
```

Development endpoints are documented in the [README](README.md#getting-started).

## Architecture Rules

RescueLink follows Clean Architecture and CQRS.

### Domain

The Domain layer owns:

- entities, value objects, and enums;
- business invariants and state transitions;
- domain events;
- logic that must remain independent of frameworks and storage.

Do not add EF Core, HTTP, Identity, file-system, or infrastructure concerns to Domain.

### Application

The Application layer owns:

- commands and queries;
- MediatR handlers and pipeline behaviors;
- FluentValidation validators;
- response models and abstractions;
- orchestration of domain behavior.

Application should depend on abstractions rather than concrete infrastructure implementations.

### Persistence

The Persistence layer owns:

- EF Core configurations and migrations;
- repository implementations;
- Identity persistence;
- Dapper read services and parameterized SQL.

Never concatenate user input into SQL. Remember that EF Core global query filters do not apply to Dapper queries.

### Infrastructure

The Infrastructure layer owns external implementations such as token generation and file storage. Keep third-party and operating-system concerns behind Application abstractions.

### API

Controllers must remain thin. A controller should:

1. accept and bind the HTTP request;
2. create or send a command/query;
3. translate the application result into an HTTP response.

Do not place business rules, database queries, file-storage logic, or matching calculations directly in controllers.

## CQRS and Feature Conventions

- Keep commands, queries, handlers, validators, and response types in the appropriate feature folder.
- Use MediatR for application use cases.
- Use FluentValidation for request-shape validation.
- Keep business invariants in Domain rather than duplicating them in handlers.
- Follow existing naming, namespace, nullable-reference-type, and file-layout conventions.
- Reuse existing abstractions and error handling before introducing new patterns.
- Avoid unrelated refactoring in a focused pull request.

## Database Migrations

A migration is required when the persisted model or schema changes.

Before creating one:

1. Confirm the proposed data model in the issue.
2. Update the entity and EF Core configuration.
3. Choose a descriptive migration name.
4. Generate the migration from the repository root.

Example:

```bash
dotnet ef migrations add AddPetReportSightings \
  --project src/RescueLink.Persistence \
  --startup-project src/RescueLink.API
```

Then apply and inspect it:

```bash
dotnet ef database update \
  --project src/RescueLink.Persistence \
  --startup-project src/RescueLink.API
```

Review both generated migration methods. Do not edit an already-merged migration to introduce a new schema change. Add a new migration instead.

A migration should:

- preserve existing data where practical;
- define safe defaults or nullable transitions for existing rows;
- include appropriate indexes and constraints;
- avoid destructive changes unless they were explicitly approved.

Do not create migrations for documentation-only, validator-only, or non-persistent changes.

## Testing

Add or update tests for every behavioral change.

Use the existing test style:

- xUnit for tests;
- FluentAssertions for assertions;
- focused unit tests for Domain and Application behavior;
- integration tests for HTTP, authorization, persistence, migrations, and spatial behavior.

Run the solution checks:

```bash
dotnet restore RescueLink.slnx
dotnet build RescueLink.slnx --configuration Release --no-restore
dotnet test RescueLink.slnx --configuration Release --no-build
```

Integration tests use SQL Server Testcontainers. Docker must be running when those tests are included in the test target.

When relevant, test:

- the successful path;
- validation boundaries;
- authorization and ownership;
- missing or invalid resources;
- idempotency and duplicate prevention;
- localization;
- privacy and sensitive-data boundaries;
- regression behavior identified by the issue.

Do not weaken, remove, or skip existing tests to make a change pass.

## Formatting and Quality

Before committing:

```bash
dotnet format RescueLink.slnx --verify-no-changes
git diff --check
git status
```

If formatting verification reports issues introduced by your change, run:

```bash
dotnet format RescueLink.slnx
```

Review the final diff and remove generated files, debug output, unrelated formatting, and local configuration.

## Security and Secrets

Never commit or paste into an issue or pull request:

- passwords or database credentials;
- JWT signing keys;
- access or refresh tokens;
- real `.env` files;
- production connection strings;
- personal addresses, phone numbers, or other private data;
- resolved output from commands that may expand secrets.

Use User Secrets, environment variables, and safe placeholders. Redact sensitive values from logs and screenshots.

Treat uploaded files and user-provided paths as untrusted. Preserve existing authorization, ownership, rate-limit, path-validation, and error-handling behavior.

If you discover a security vulnerability, do not publish exploitation details in a public issue. Contact the repository owner privately.

## Commits

Keep commits focused and use an imperative message.

Examples:

```text
feat: add pet report sighting history
fix: exclude archived reports from total count
test: cover matching score boundaries
docs: add contributor guide
```

Do not include unrelated changes in the same commit.

## Pull Requests

Before opening a pull request:

- rebase or merge the latest `upstream/master` as appropriate;
- resolve conflicts locally;
- review `git diff upstream/master...HEAD`;
- ensure build, tests, and formatting checks pass;
- push the issue branch to your fork.

A pull request should:

- target `ismailbarankarasu/RescueLink:master`;
- have a clear title and summary;
- reference the issue with `Fixes #<issue-number>` when it fully resolves it;
- explain important implementation decisions;
- list the tests and commands run;
- mention migrations, API-contract changes, or security effects;
- include screenshots or example responses when useful;
- remain limited to the confirmed issue scope.

Maintainers may request changes. Push follow-up commits to the same branch and re-request review when ready. Do not close and recreate the pull request unless asked.

## Definition of Done

A contribution is complete when:

- [ ] The confirmed issue scope and acceptance criteria are satisfied.
- [ ] Layer boundaries and existing conventions are preserved.
- [ ] Relevant tests are added or updated.
- [ ] `dotnet build RescueLink.slnx --configuration Release` succeeds.
- [ ] `dotnet test RescueLink.slnx --configuration Release` succeeds.
- [ ] Formatting and `git diff --check` pass.
- [ ] Database migrations are included and reviewed when required.
- [ ] No secrets, credentials, tokens, or private data are committed.
- [ ] Documentation and API examples are updated when behavior changes.
- [ ] The pull request references its issue and explains verification.
- [ ] CI checks pass.

Thank you for contributing to RescueLink and helping reunite lost pets with their families.
