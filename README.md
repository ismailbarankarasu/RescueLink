# RescueLink API

**RescueLink is not just a lost-pet listing API. It is a location-aware matching platform designed to connect lost pet reports with found animal reports through geospatial search and intelligent matching.**

RescueLink helps pet owners reunite with lost animals and enables people who find potentially lost pets to reach the right owners. Instead of scattering information across social media, WhatsApp groups, and local communities, RescueLink brings **Lost** and **Found** reports together in one system — with real coordinates, structured animal data, event-driven matching, and two-sided match confirmation.

> **Current status:** The backend API foundation and core workflows are implemented: JWT authentication with refresh-token rotation and logout, authentication rate limiting, restricted frontend CORS, liveness/readiness health checks, global user profile management, English/Turkish/German request localization, localized validation and API error responses, localized JWT/rate-limit failures, culture-aware notification content, report creation and owner-specific listing, public filtering and pagination, report updates with automatic match recalculation, owner-only soft archive/restore lifecycle, archived-report listing, geospatial nearby search, secure photo management, event-driven smart matching, two-sided confirmation/rejection, secure counterpart contact disclosure after mutual confirmation, automatic resolution, in-app notifications, Docker support, and SQL Server-backed integration tests. Explainable scoring, realtime delivery, centralized monitoring, and production deployment remain on the roadmap.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Problem Statement](#2-problem-statement)
3. [Solution](#3-solution)
4. [Key Features](#4-key-features)
5. [How RescueLink Works](#5-how-rescuelink-works)
6. [Pet Matching Engine](#6-pet-matching-engine)
7. [Geospatial Search](#7-geospatial-search)
8. [Architecture](#8-architecture)
9. [Technologies](#9-technologies)
10. [Project Structure](#10-project-structure)
11. [Main Domain Models](#11-main-domain-models)
12. [API Features](#12-api-features)
13. [Example API Requests](#13-example-api-requests)
14. [Example Match Response](#14-example-match-response)
15. [Getting Started](#15-getting-started)
16. [Configuration](#16-configuration)
17. [Database Setup](#17-database-setup)
18. [File Storage](#18-file-storage)
19. [Testing](#19-testing)
20. [Future Roadmap](#20-future-roadmap)
21. [Security Considerations](#21-security-considerations)
22. [Contributing](#22-contributing)
23. [License](#23-license)

---

## 1. Project Overview

RescueLink is a **location-based lost-and-found pet reporting platform** built with **ASP.NET Core Web API** and **Clean Architecture**.

The long-term goal is not simply to let users create listings. RescueLink evaluates:

- Lost and Found report types
- Animal characteristics (species, breed, color, gender)
- Geographic location
- Event/report dates

…together to surface **potential matches** between reports created in the same area.

---

## 2. Problem Statement

When a pet goes missing, owners often post across many disconnected channels:

- Social media and stories
- Facebook / WhatsApp groups
- Veterinary clinics and local animal communities
- Physical flyers

At the same time, someone else may have found the same animal a few kilometers away and posted on a completely different platform.

The core problem: **the person who lost the pet and the person who found it often never see each other's posts.**

RescueLink aims to connect these two sides with technology:

```text
Lost Report + Found Report + Location + Animal Characteristics → Potential Match
```

---

## 3. Solution

RescueLink uses a unified **PetReport** model for both Lost and Found reports. Each report stores real geographic coordinates, animal attributes, photos, and lifecycle status.

Today, the API supports:

- User registration and JWT-based login
- Global user profiles with E.164 phone numbers, ISO country codes, city, preferred language, and IANA time zones
- Request localization through `Accept-Language` with English, Turkish, and German responses
- Localized RFC-compatible validation Problem Details (`title`, `detail`, and field messages)
- Centralized, localized application errors across authentication, reports, matches, notifications, and user profiles
- Localized JWT challenge/forbidden and rate-limit responses
- Notification titles and messages localized at query time while persisted content remains a safe fallback
- Hashed refresh-token persistence, single-use rotation, replay protection, logout/revocation, and optimistic concurrency control
- Per-IP rate limiting for authentication/token endpoints and configuration-based frontend CORS
- Creating Lost/Found reports with coordinates
- Resolving or cancelling reports with ownership checks
- Soft-archiving and idempotently restoring owned reports without deleting historical data
- Searching nearby active reports by distance
- Managing report photos (upload, delete, set primary)
- Automatically discovering and scoring potential Lost/Found matches
- Listing match suggestions by score and distance
- Two-sided match confirmation and rejection
- Secure counterpart contact disclosure only to match participants after mutual confirmation
- Automatically resolving both reports after mutual match confirmation
- Public report discovery with filtering and pagination
- Authenticated `mine` listing across Active, Resolved, and Cancelled reports, including an `archivedOnly` filter
- Owner-only report updates with automatic Suggested-match recalculation
- In-app match suggestion notifications with unread filtering and read tracking
- Structured Serilog request logging with trace/user correlation and safe global exception handling
- Dockerized API + SQL Server startup with automatic migrations, health checks, and persistent volumes

Matching is triggered through Domain Events after a report is persisted or updated. Spatial candidate discovery uses Dapper and SQL Server `STDistance`; business scoring remains isolated in the Domain layer.

---

## 4. Key Features

### Implemented

| Feature | Description |
|--------|-------------|
| **Unified PetReport model** | Single domain entity for `Lost` and `Found` reports |
| **JWT authentication** | Register/login plus protected report, photo, match, and notification operations |
| **Refresh-token sessions** | SHA-256 token hashes, rotation, replay prevention, logout/revocation, and SQL Server `rowversion` concurrency control |
| **API abuse protection** | Separate per-IP rate limits for authentication and token operations |
| **Frontend-ready CORS** | Configuration-based allowlist for trusted Angular origins |
| **Health checks** | Independent liveness plus SQL Server-backed readiness endpoints |
| **Global user profiles** | Authenticated profile retrieval/update with E.164 phone, ISO country, language, city, and IANA time-zone fields |
| **API localization** | `Accept-Language` negotiation with English, Turkish, and German validation, application errors, JWT failures, rate-limit responses, and Problem Details |
| **Localized notification content** | Notification titles/messages are selected from `.resx` resources by request culture, with stored database text as fallback |
| **Structured observability** | Serilog request logs, trace/user correlation, status-aware levels, and safe global exception handling |
| **Dockerized runtime** | Multi-stage .NET 10 image, SQL Server Compose service, automatic migrations, and persistent SQL/upload volumes |
| **Geospatial nearby search** | SQL Server `geography`, spatial index, NetTopologySuite, and Dapper |
| **Smart Matching Engine** | Event-driven Lost/Found candidate discovery with weighted scoring |
| **Two-sided match decisions** | Both owners must confirm; either owner can reject a suggestion |
| **Report lifecycle API** | Owner-only update, resolve, cancel, soft archive, and idempotent restore workflows |
| **Automatic match resolution** | Both reports become `Resolved` after both owners confirm |
| **Report discovery** | Public filtering/pagination plus authenticated owner-only `mine` listing with active/archive separation |
| **Match recalculation** | Updating an active report removes stale suggestions and recalculates candidates |
| **In-app notifications** | Suggested/confirmed match alerts, pagination, unread filtering/count, single read, and bulk read |
| **Photo management** | Up to 5 photos; primary selection; delete; signature-validated local storage |
| **CQRS + MediatR** | Commands, queries, pipeline behaviors, and Domain Event notifications |
| **FluentValidation** | Request validation via pipeline behavior |
| **Domain rules** | Rich `PetReport` aggregate (resolve/cancel rules, photo limits) |
| **Automated tests** | Domain, Application, API, and SQL Server Testcontainers integration coverage |
| **CI pipeline** | GitHub Actions restore, Release build, tests, and Docker image validation on .NET 10 |

### Planned

| Feature | Description |
|--------|-------------|
| **Explainable matches** | Human-readable reasons in addition to the implemented score |
| **Advanced filtering** | Add breed and date-range filters to implemented pagination |
| **Realtime delivery** | SignalR, email, or push delivery on top of persisted in-app notifications |
| **Cloud storage** | Azure Blob (or similar) via `IFileStorageService` |
| **Deployment automation** | Container registry publishing, centralized monitoring, and production deployment |

---

## 5. How RescueLink Works

There are two report types:

### Lost Report

Created when a pet owner reports a missing animal.

> Example: *A male Golden Retriever, yellow, lost in Nilüfer, Bursa.*

### Found Report

Created when someone finds a stray or potentially lost animal.

> Example: *A yellow Golden Retriever, male, found in Nilüfer, Bursa.*

Both are stored as **PetReport** records with a `ReportType` of `Lost` or `Found`. They remain independent records, while the Matching Engine evaluates compatible reports using shared animal attributes and geographic proximity.

### End-to-end vision

```text
User registers
      ↓
User logs in and receives access + refresh tokens
      ↓
Lost report is created with location + photos
      ↓
Another user creates a Found report
      ↓
Domain Event triggers Matching Engine
      ↓
Dapper finds nearby opposite-type candidates
      ↓
Domain scoring creates match suggestions
      ↓
Both report owners review the suggestion
      ↓
Both confirm (or either rejects)
      ↓
Both reports are automatically marked Resolved
      ↓
Confirmed participants can securely retrieve counterpart contact details
      ↓
Owners receive lifecycle-aware in-app notifications in the requested language
      ↓
Owner reunites with pet
```

---

## 6. Pet Matching Engine

> **Status: Implemented**

The Matching Engine distinguishes RescueLink from a basic CRUD listing API. Creating a `PetReport` raises a `PetReportCreatedDomainEvent`; updating an active report raises a `PetReportUpdatedDomainEvent`. After persistence, MediatR-backed observers invoke a shared recalculation command without coupling report writes to matching logic. Updating a report removes only stale `Suggested` matches, preserves `Confirmed`/`Rejected` history, and recalculates candidates using the new attributes and location.

Candidate discovery first applies hard rules:

- Only `Active` reports
- Opposite report types (`Lost` ↔ `Found`)
- Same animal species
- Different report owners
- Maximum distance of 10 km

Dapper executes the spatial candidate query using SQL Server `geography` and `STDistance`. The Domain scoring service then applies:

| Criterion | Weight |
|-----------|--------|
| Same species | +30 |
| Same breed | +20 |
| Same primary color | +20 |
| Any color overlap | +10 |
| Same known gender | +10 |
| Distance | +5 to +20 |

Suggestions require at least **50 points**. Results are stored as `PetReportMatch` records and ordered by score descending, then distance ascending.

Match decisions are two-sided:

- One owner confirms: the match remains `Suggested`
- Both owners confirm: the match becomes `Confirmed` and both reports become `Resolved`
- Either owner rejects: the match becomes `Rejected`
- A new suggestion raises a Domain Event that persists one in-app notification per owner
- Contact information is available only after mutual confirmation and only to the two participating owners

Rejected matches are excluded from match-list responses. Suggested-match notification delivery is persisted and owner-scoped.

---

## 7. Geospatial Search

RescueLink stores **real coordinates**, not just city/district text.

Locations are modeled as a `GeoLocation` value object and persisted using **NetTopologySuite** with SQL Server's `geography` type. Nearby queries use spatial distance (`STDistance`) and return results sorted by proximity.

**Example endpoint:**

```http
GET /api/pet-reports/nearby?latitude=40.21&longitude=28.98&radiusMeters=10000&reportType=Lost&species=Dog&limit=20
```

| Parameter | Default | Notes |
|-----------|---------|-------|
| `latitude` | required | -90 to 90 |
| `longitude` | required | -180 to 180 |
| `radiusMeters` | `5000` | Search radius in **meters** |
| `reportType` | optional | `Lost` or `Found` |
| `species` | optional | e.g. `Dog`, `Cat` |
| `limit` | `20` | Max results |

Only **Active** reports are returned. Each result includes `distanceMeters` from the query origin.

---

## 8. Architecture

The solution follows **Clean Architecture** with clear layer boundaries:

```text
┌─────────────────────────────────────┐
│              API                    │  HTTP endpoints, auth, OpenAPI
├─────────────────────────────────────┤
│         Application                 │  CQRS, handlers, validation, abstractions
├─────────────────────────────────────┤
│            Domain                   │  Entities, enums, value objects, rules
├─────────────────────────────────────┤
│       Infrastructure                │  JWT, local file storage
├─────────────────────────────────────┤
│         Persistence                 │  EF Core, Identity, Dapper reads, migrations
└─────────────────────────────────────┘
```

### Request flow

```text
HTTP Request
     ↓
Controller
     ↓
Command / Query (MediatR)
     ↓
ValidationBehavior (FluentValidation)
     ↓
Handler
     ↓
Domain / Repository / Read Service
     ↓
Response
```

Controllers stay thin: they translate HTTP to MediatR messages and map results to HTTP status codes.

### Event-driven matching flow

```text
PetReport.Create
      ↓
PetReportCreatedDomainEvent
      ↓
DbContext saves successfully
      ↓
DomainEventDispatcher / MediatR notification
      ↓
Dapper spatial candidate query
      ↓
Domain score calculator
      ↓
EF Core persists PetReportMatch suggestions
      ↓
PetReportMatchSuggestedDomainEvent
      ↓
Owner-scoped UserNotifications are persisted
```

---

## 9. Technologies

| Area | Stack |
|------|-------|
| Runtime | .NET 10 |
| Web framework | ASP.NET Core Web API |
| Patterns | Clean Architecture, CQRS, MediatR, Observer via Domain Events |
| Validation | FluentValidation with localized resource messages |
| Localization | ASP.NET Core Request Localization (`en`, `tr`, `de`), centralized error localization, and culture-aware notification `.resx` resources |
| ORM | Entity Framework Core 10 |
| Geospatial | NetTopologySuite, SQL Server `geography` |
| Read queries | Dapper (nearby search, matching candidates, match listing, report discovery, owner reports, notifications) |
| Identity | ASP.NET Core Identity |
| Authentication | JWT Bearer, hashed refresh tokens, rotation, revocation, optimistic concurrency |
| API protection | ASP.NET Core rate limiting, restricted CORS |
| Operations | Liveness/readiness health checks, Serilog structured request logging, and safe global exception handling |
| Containers | Multi-stage Dockerfile, Docker Compose, SQL Server 2022, persistent volumes |
| Storage | Local filesystem (`IFileStorageService`) |
| Testing | xUnit, Moq, FluentAssertions, WebApplicationFactory, Testcontainers for SQL Server |
| CI | GitHub Actions (.NET build/test + Docker image build) |
| API exploration | OpenAPI (Development), Postman collection |

---

## 10. Project Structure

```text
RescueLink/
├── src/
│   ├── RescueLink.API/              # Web API, controllers, Program.cs
│   ├── RescueLink.Application/      # Features, CQRS, validation, localization, abstractions
│   ├── RescueLink.Domain/           # PetReport, enums, GeoLocation
│   ├── RescueLink.Infrastructure/   # JWT, LocalFileStorageService
│   └── RescueLink.Persistence/      # DbContext, migrations, repositories, Dapper
├── tests/
│   ├── RescueLink.Domain.Tests/
│   ├── RescueLink.Application.Tests/
│   ├── RescueLink.API.Tests/        # HTTP exception handling tests
│   └── RescueLink.API.IntegrationTests/ # Real HTTP + SQL Server container flows
├── postman/                         # API request collection
├── .github/workflows/dotnet.yml     # .NET + Docker CI pipeline
├── Dockerfile                       # Multi-stage .NET 10 API image
├── compose.yml                      # API + SQL Server orchestration
├── .dockerignore
├── .env.example                     # Required environment variable template
└── RescueLink.slnx
```

---

## 11. Main Domain Models

### PetReport

Central aggregate for both Lost and Found reports.

| Field | Description |
|-------|-------------|
| `Id`, `UserId` | Identity and ownership |
| `ReportType` | `Lost` or `Found` |
| `Status` | `Active`, `Resolved`, `Cancelled` |
| `Title`, `Description` | Report summary and details |
| `Species`, `Gender`, `Breed` | Animal attributes |
| `PetName` | Optional; more common on Lost reports |
| `PrimaryColor`, `SecondaryColor` | Color descriptors |
| `EventDate` | When the pet was lost/found |
| `Location` | `GeoLocation` (latitude/longitude) |
| `Photos` | Up to 5 photos per report |
| `IsArchived`, `ArchivedAt` | Soft-archive state and audit timestamp |

Domain methods include `UpdateDetails()`, `Resolve()`, `Cancel()`, `Archive()`, `Restore()`, `AddPhoto()`, `SetPrimaryPhoto()`, and `RemovePhoto()`. Archived reports are protected from modification, excluded from normal EF Core and Dapper reads, and can be restored idempotently by their owner.

### PetReportPhoto

Stores a `storageKey` reference (not binary data in the database). One photo can be marked as primary.

### PetReportMatch

Stores a normalized Lost/Found pair, score, distance, lifecycle status, and each owner's confirmation state. A unique database index prevents duplicate Lost/Found pairs.

| Field | Description |
|-------|-------------|
| `LostReportId`, `FoundReportId` | Normalized report pair |
| `Score` | Domain-calculated score from 0 to 100 |
| `DistanceMeters` | SQL Server spatial distance |
| `Status` | `Suggested`, `Confirmed`, or `Rejected` |
| `LostOwnerConfirmed` | Lost report owner's decision |
| `FoundOwnerConfirmed` | Found report owner's decision |

### UserNotification

Persists owner-scoped in-app notifications with type, title, message, optional related entity ID, read state, and read timestamp. `MarkAsRead()` is idempotent.

### GeoLocation

Value object with validated latitude (-90…90) and longitude (-180…180).

---

## 12. API Features

### Authentication (`/api/auth`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Anonymous | Create account |
| POST | `/api/auth/login` | Anonymous | Login and receive access + refresh tokens |
| POST | `/api/auth/refresh` | Anonymous | Rotate a valid refresh token and issue a new token pair |
| POST | `/api/auth/logout` | Anonymous | Idempotently revoke a refresh token |

### Current User (`/api/users`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/users/me` | Required | Return the authenticated user's global profile |
| PUT | `/api/users/me` | Required | Update names, phone, country, city, language, and time zone |

Profile validation follows global standards: E.164 phone numbers, two-letter ISO country codes, valid culture codes, and IANA time-zone identifiers. Validation responses honor the request's `Accept-Language` header for `en`, `tr`, and `de`.

### Pet Reports (`/api/pet-reports`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/pet-reports` | Required | Create Lost/Found report |
| GET | `/api/pet-reports` | Anonymous | Public active report discovery with filters and pagination |
| GET | `/api/pet-reports/mine` | Required | List the caller's reports with type/status filters; use `archivedOnly=true` for the archive |
| GET | `/api/pet-reports/{id}` | Anonymous | Get report details |
| PUT | `/api/pet-reports/{id}` | Required | Update an owned active report and recalculate suggestions |
| GET | `/api/pet-reports/nearby` | Anonymous | Nearby active reports |
| POST | `/api/pet-reports/{id}/photos` | Required | Upload photo (multipart) |
| PATCH | `/api/pet-reports/{reportId}/photos/{photoId}/primary` | Required | Set primary photo |
| DELETE | `/api/pet-reports/{reportId}/photos/{photoId}` | Required | Delete photo |
| PATCH | `/api/pet-reports/{id}/resolve` | Required | Mark active report as resolved |
| PATCH | `/api/pet-reports/{id}/cancel` | Required | Cancel active report |
| DELETE | `/api/pet-reports/{id}` | Required | Soft-archive an owned report (idempotent) |
| PATCH | `/api/pet-reports/{id}/restore` | Required | Restore an owned archived report (idempotent) |
| GET | `/api/pet-reports/{id}/matches` | Required | List owner-visible match suggestions |

### Pet Report Matches (`/api/pet-report-matches`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| PATCH | `/api/pet-report-matches/{matchId}/confirm` | Required | Confirm match for the caller's report |
| PATCH | `/api/pet-report-matches/{matchId}/reject` | Required | Reject a suggested match |
| GET | `/api/pet-report-matches/{matchId}/contact` | Required | Return counterpart contact details only after mutual confirmation |

### Notifications (`/api/notifications`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | Required | Paginated owner notifications; supports `unreadOnly` and localized title/message content through `Accept-Language` |
| GET | `/api/notifications/unread-count` | Required | Return the caller's unread notification count |
| PATCH | `/api/notifications/{id}/read` | Required | Idempotently mark an owned notification as read |
| PATCH | `/api/notifications/read-all` | Required | Mark all caller notifications as read in one bulk update |

### Health (`/health`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/health/live` | Anonymous | Process liveness without external dependencies |
| GET | `/health/ready` | Anonymous | SQL Server-backed readiness check |

**Authorization rules:** Only report owners can modify reports/photos or view their report's matches. A match can be managed only by the owner of its Lost or Found report. Notification queries are always scoped to the authenticated user, and only the owner can mark a notification as read. API errors, authentication/authorization failures, rate-limit responses, and notification content honor supported `Accept-Language` values (`en`, `tr`, `de`).

---

## 13. Example API Requests

### Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "Ayşe",
  "lastName": "Yılmaz",
  "email": "ayse@example.com",
  "password": "SecurePass1",
  "confirmPassword": "SecurePass1"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "ayse@example.com",
  "password": "SecurePass1"
}
```

Response includes `accessToken`, `expiresAt`, `refreshToken`, and `refreshTokenExpiresAt`. Use the access token as `Authorization: Bearer {token}`. Refresh tokens are single-use: successful refresh rotates the token and revokes the previous value.

### Refresh an authentication session

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "{current-refresh-token}"
}
```

### Logout

```http
POST /api/auth/logout
Content-Type: application/json

{
  "refreshToken": "{current-refresh-token}"
}
```

### Create Lost Report

```http
POST /api/pet-reports
Authorization: Bearer {token}
Content-Type: application/json

{
  "reportType": "Lost",
  "title": "Missing cat in Nilüfer",
  "description": "Gray and white cat last seen near the park.",
  "species": "Cat",
  "gender": "Female",
  "petName": "Luna",
  "breed": "Domestic Shorthair",
  "primaryColor": "Gray",
  "secondaryColor": "White",
  "eventDate": "2026-08-13T18:00:00+03:00",
  "latitude": 40.195,
  "longitude": 29.060
}
```

### Nearby Search

```http
GET /api/pet-reports/nearby?latitude=40.195&longitude=29.060&radiusMeters=5000&reportType=Lost&species=Cat
```

### Archive and restore an owned report

```http
DELETE /api/pet-reports/{reportId}
Authorization: Bearer {token}
```

```http
PATCH /api/pet-reports/{reportId}/restore
Authorization: Bearer {token}
```

List only archived reports:

```http
GET /api/pet-reports/mine?archivedOnly=true
Authorization: Bearer {token}
```

Archive and restore are soft-delete operations: the database row is retained, `IsArchived`/`ArchivedAt` are updated, and ordinary discovery queries exclude archived reports.

### Upload Photo

```http
POST /api/pet-reports/{reportId}/photos
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [JPEG, PNG, or WebP — max 5 MB]
```

A Postman collection is available under `postman/collections/`.

---

## 14. Example Match Response

`GET /api/pet-reports/{id}/matches` returns the counterpart report together with its score, distance, status, and primary photo key:

```json
[
  {
    "matchId": "f51e3b32-cd50-4177-aaac-f744665dfc46",
    "counterpartReportId": "b4264822-10f9-45bc-9003-d02b74be58fa",
    "reportType": "Found",
    "title": "Found tabby cat in Nilüfer",
    "species": "Cat",
    "gender": "Male",
    "breed": "Tabby",
    "primaryColor": "Gray",
    "secondaryColor": "White",
    "eventDate": "2026-08-15T15:00:00+03:00",
    "latitude": 40.217,
    "longitude": 28.9852,
    "score": 100,
    "distanceMeters": 61.11,
    "status": "Suggested",
    "primaryPhotoStorageKey": null
  }
]
```

Human-readable scoring reasons are planned as a future enhancement; the numeric scoring and match lifecycle are implemented.

---

## 15. Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full instance)
- (Optional) Postman for API testing
- (Optional) Docker Desktop for containerized startup

### Clone and run

```bash
git clone https://github.com/ismailbarankarasu/RescueLink.git
cd RescueLink

dotnet restore
dotnet build

# Apply migrations (see Database Setup)
dotnet ef database update --project src/RescueLink.Persistence --startup-project src/RescueLink.API

dotnet run --project src/RescueLink.API
```

Default URLs (Development):

- HTTPS: `https://localhost:7051`
- HTTP: `http://localhost:5218`
- OpenAPI: available in Development environment

### Run with Docker Compose

Docker starts the API and SQL Server together, applies EF Core migrations when enabled by Compose, and stores database/uploads in named volumes.

```bash
git clone https://github.com/ismailbarankarasu/RescueLink.git
cd RescueLink

# Create the local secrets file from the committed template
cp .env.example .env

# Replace the placeholder values in .env, then start the stack
docker compose up -d --build
```

Windows PowerShell users can create the file with:

```powershell
Copy-Item .env.example .env
```

Docker endpoints and ports:

| Resource | Address | Purpose |
|----------|---------|---------|
| API | `http://localhost:8080` | RescueLink HTTP API |
| Liveness | `http://localhost:8080/health/live` | Process health |
| Readiness | `http://localhost:8080/health/ready` | API + SQL Server readiness |
| SQL Server | `localhost,14330` | Optional host access (container uses internal port 1433) |

Useful commands:

```bash
docker compose ps
docker compose logs api --tail 100
docker compose down
```

`docker compose down` keeps named volumes. Do not add `-v` unless you intentionally want to delete Docker database and upload data. The real `.env` file is ignored by Git; only `.env.example` is committed.

---

## 16. Configuration

### Connection string

Update `src/RescueLink.API/appsettings.json` (or User Secrets / environment variables):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=RescueLinkDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "RescueLink.API",
    "Audience": "RescueLink.Web",
    "ExpirationMinutes": 60
  },
  "RefreshToken": {
    "ExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://localhost:4200"
    ]
  }
}
```

### JWT secret (required)

The JWT signing key **must not** be committed. Configure it via User Secrets:

```bash
dotnet user-secrets set "Jwt:SecretKey" "your-super-secret-key-at-least-32-bytes-long" --project src/RescueLink.API
```

The secret must be at least **32 bytes**.

### Docker environment variables

Compose reads a local `.env` file in the repository root and injects configuration through ASP.NET Core environment-variable mapping:

```env
SQL_SA_PASSWORD=replace-with-a-strong-local-password
JWT_SECRET_KEY=replace-with-a-random-secret-of-at-least-64-characters
```

`Jwt__SecretKey`, `ConnectionStrings__DefaultConnection`, and `Database__ApplyMigrations` in `compose.yml` map to nested ASP.NET Core configuration. Never commit the real `.env` file or paste the resolved output of `docker compose config` into issues/logs because it may contain secrets.

---

## 17. Database Setup

RescueLink uses **EF Core migrations** with SQL Server and **NetTopologySuite** for spatial columns.

```bash
# From repository root
dotnet ef database update --project src/RescueLink.Persistence --startup-project src/RescueLink.API
```

Migrations include:

- Initial schema (PetReports, photos)
- ASP.NET Core Identity tables
- Spatial index on report locations
- `PetReportMatches` with score/distance constraints and unique Lost/Found pairs
- Two-sided owner confirmation fields
- `UserNotifications` with Identity ownership, unread/read state, and supporting index
- `RefreshTokens` with hashed values, expiry/revocation metadata, replacement chains, unique hash index, and SQL Server `rowversion`
- Global Identity profile fields for phone, country, city, preferred language, and time zone
- Pet report soft-archive fields (`IsArchived`, `ArchivedAt`) with EF Core global query filtering

Nearby search, matching candidate discovery, match listing, public/owner report discovery, and notification listing use **Dapper** against the same SQL Server database for efficient read queries. Dapper report queries explicitly apply archive predicates because EF Core global query filters do not affect raw SQL.

---

## 18. File Storage

Photos are **not** stored as BLOBs in the database. The `IFileStorageService` abstraction stores files externally; only the `storageKey` is persisted.

**Current implementation:** `LocalFileStorageService`

- Path: `wwwroot/uploads/pet-reports/`
- Allowed formats: JPEG, PNG, WebP (validated by file header)
- Max size: 5 MB per file
- Max photos per report: 5

**Planned:** Azure Blob Storage (or another provider) implementing the same interface for production.

---

## 19. Testing

```bash
dotnet test
```

Test projects:

| Project | Focus |
|---------|-------|
| `RescueLink.Domain.Tests` | PetReport rules, GeoLocation, entity behavior |
| `RescueLink.Application.Tests` | Handlers, validators, MediatR pipeline |
| `RescueLink.API.Tests` | Safe global exception responses and exception logging |
| `RescueLink.API.IntegrationTests` | Real HTTP pipeline and SQL Server flows through WebApplicationFactory + Testcontainers |

Examples of covered behavior:

- PetReport creation and photo limits
- Resolve/cancel domain rules
- Soft archive/restore domain rules, modification guards, ownership, and idempotency
- Nearby query validation
- Authentication validators and login/refresh/logout handlers
- Photo upload/delete/set-primary handlers
- Image signature validation and rollback behavior
- Resolve/cancel handlers and ownership rules
- Matching score combinations and distance bands
- Domain Event matching handler
- Two-sided confirm/reject handlers
- Automatic report resolution after mutual confirmation
- Report update authorization and active-state rules
- Match recalculation and stale-suggestion cleanup flow
- Notification entity rules, suggested/confirmed event observers, owner isolation, listing, unread count, single read, bulk-read handlers, and EN/TR/DE content localization
- Confirmed-match contact authorization and disclosure rules
- Health liveness/readiness behavior against a real SQL Server container
- Register/login, refresh-token rotation, logout, and authorization over HTTP
- Report creation, retrieval, ownership protection, spatial nearby ordering, archive filtering, and the complete archive/restore HTTP lifecycle
- Global user profile update/retrieval, normalization, invalid-update protection, and localized validation responses
- End-to-end notification localization through the real HTTP pipeline for English, Turkish, and German

CI runs restore, Release build, all tests, and a Linux Docker image build on every push/PR to `master` via GitHub Actions. The CI image is validated but is not yet published to a registry.

Integration tests run against an isolated SQL Server 2022 container and apply the real EF Core migrations before exercising the HTTP API.

---

## 20. Future Roadmap

- [x] **Pet Matching Engine** with weighted scoring
- [x] **Event-driven matching** with Domain Events / Observer Pattern
- [x] **Resolve / Cancel report API**
- [x] **Two-sided match confirmation and rejection**
- [ ] **Explainable match reasons**
- [x] **List & filter reports** with pagination
- [x] **Owner report listing** with status/type filters
- [x] **Update active reports** with automatic match recalculation
- [x] **In-app match suggestion notifications** with read tracking
- [x] **Confirmed-match notifications, unread-count endpoint, and bulk read**
- [x] **Secure counterpart contact disclosure after mutual confirmation**
- [x] **Refresh-token rotation, replay prevention, logout, and concurrency protection**
- [x] **Per-IP authentication/token rate limiting**
- [x] **Restricted frontend CORS policy**
- [x] **Liveness and SQL Server readiness health checks**
- [x] **Global user profile management** with international phone/country/language/time-zone fields
- [x] **English, Turkish, and German API localization** via `Accept-Language`
- [x] **Centralized localized errors** for controllers, JWT middleware, validation, and rate limiting
- [x] **Localized notification content** with persisted fallback text and HTTP integration coverage
- [x] **Soft archive and restore reports** with owner-only access, archived listing, global query filtering, and integration coverage
- [ ] **Realtime/email/push delivery** (SignalR → email/push)
- [ ] **Admin role** for moderation
- [ ] **Azure Blob Storage** provider
- [ ] **Web / mobile clients**
- [x] **Dockerized API + SQL Server** with persistent volumes and automatic migrations
- [x] **CI Docker image validation** after successful build/tests
- [x] **Structured Serilog request logging** with trace/user correlation
- [ ] **Centralized production monitoring and deployment**
- [x] **SQL Server Testcontainers integration tests** for health, authentication, authorization, reports, spatial queries, and user profiles

Success will be measured not only by report volume, but by **how many lost pets are reunited through the platform**.

---

## 21. Security Considerations

- Passwords hashed via ASP.NET Core Identity (complexity rules enforced)
- JWT Bearer authentication for protected endpoints
- Refresh tokens are stored only as SHA-256 hashes, rotated after use, revocable on logout, and protected from concurrent reuse with SQL Server `rowversion`
- Separate per-IP rate limits protect login/register and refresh/logout operations
- Frontend CORS uses an explicit configuration allowlist rather than `AllowAnyOrigin`
- Users can only manage their own reports, photos, match decisions, and notifications
- Soft archive and restore operations require report ownership; archived reports are hidden from ordinary public and matching reads
- Match details are visible only to the owner of the requested report
- Counterpart contact information is disclosed only to match participants after mutual confirmation
- Notification list/read operations are scoped to the authenticated user's ID
- Two-sided confirmation prevents one party from unilaterally confirming a reunion
- File uploads validated by content type (magic bytes), size, and path traversal protection on delete
- JWT secret and connection strings must be kept out of source control (User Secrets / environment variables)
- Sensitive data (passwords, tokens) must not appear in logs
- Unhandled exceptions return safe Problem Details with a trace ID while full details remain in server logs
- Authentication, authorization, validation, application, and rate-limit failures use centralized status mapping and localized safe responses
- Docker secrets are injected from an ignored `.env` file; `.env.example` contains placeholders only
- Admin/moderation capabilities are planned for inappropriate or fraudulent reports

---

## 22. Contributing

Contributions are welcome. Please:

1. Fork the repository
2. Create a feature branch from `master`
3. Follow existing Clean Architecture and CQRS conventions
4. Add or update unit tests for business rules
5. Ensure `dotnet build` and `dotnet test` pass
6. Open a pull request with a clear description

Keep controllers thin, put business logic in Domain/Application layers, and avoid embedding matching logic directly in HTTP handlers.

---

## 23. License

This project is licensed under the **MIT License**. See [LICENSE.txt](LICENSE.txt) for details.

---

<p align="center">
  <strong>RescueLink — connecting lost pets with the people looking for them.</strong>
</p>
