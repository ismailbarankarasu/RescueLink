# RescueLink API

**RescueLink is not just a lost-pet listing API. It is a location-aware matching platform designed to connect lost pet reports with found animal reports through geospatial search and intelligent matching.**

RescueLink helps pet owners reunite with lost animals and enables people who find potentially lost pets to reach the right owners. Instead of scattering information across social media, WhatsApp groups, and local communities, RescueLink brings **Lost** and **Found** reports together in one system — with real coordinates, structured animal data, event-driven matching, and two-sided match confirmation.

> **Current status:** The backend API foundation and core workflows are implemented: authentication, report creation and owner-specific listing, public filtering and pagination, report updates with automatic match recalculation, lifecycle management, geospatial nearby search, secure photo management, event-driven smart matching, two-sided confirmation/rejection, automatic resolution after mutual confirmation, and in-app match suggestion notifications. Confirmed-match notifications, explainable scoring, and production deployment remain on the roadmap.

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
- Creating Lost/Found reports with coordinates
- Resolving or cancelling reports with ownership checks
- Searching nearby active reports by distance
- Managing report photos (upload, delete, set primary)
- Automatically discovering and scoring potential Lost/Found matches
- Listing match suggestions by score and distance
- Two-sided match confirmation and rejection
- Automatically resolving both reports after mutual match confirmation
- Public report discovery with filtering and pagination
- Authenticated `mine` listing across Active, Resolved, and Cancelled reports
- Owner-only report updates with automatic Suggested-match recalculation
- In-app match suggestion notifications with unread filtering and read tracking

Matching is triggered through Domain Events after a report is persisted or updated. Spatial candidate discovery uses Dapper and SQL Server `STDistance`; business scoring remains isolated in the Domain layer.

---

## 4. Key Features

### Implemented

| Feature | Description |
|--------|-------------|
| **Unified PetReport model** | Single domain entity for `Lost` and `Found` reports |
| **JWT authentication** | Register, login, protected report/photo operations |
| **Geospatial nearby search** | SQL Server `geography`, spatial index, NetTopologySuite, and Dapper |
| **Smart Matching Engine** | Event-driven Lost/Found candidate discovery with weighted scoring |
| **Two-sided match decisions** | Both owners must confirm; either owner can reject a suggestion |
| **Report lifecycle API** | Owner-only update, resolve, and cancel workflows |
| **Automatic match resolution** | Both reports become `Resolved` after both owners confirm |
| **Report discovery** | Public filtering/pagination plus authenticated owner-only `mine` listing |
| **Match recalculation** | Updating an active report removes stale suggestions and recalculates candidates |
| **In-app notifications** | Match suggestion notifications, pagination, unread filtering, and read tracking |
| **Photo management** | Up to 5 photos; primary selection; delete; signature-validated local storage |
| **CQRS + MediatR** | Commands, queries, pipeline behaviors, and Domain Event notifications |
| **FluentValidation** | Request validation via pipeline behavior |
| **Domain rules** | Rich `PetReport` aggregate (resolve/cancel rules, photo limits) |
| **Unit tests** | Domain and Application layer test coverage |
| **CI pipeline** | GitHub Actions build and test on .NET 10 |

### Planned

| Feature | Description |
|--------|-------------|
| **Explainable matches** | Human-readable reasons in addition to the implemented score |
| **Explainable notifications** | Confirmed-match messages and richer notification content |
| **Advanced filtering** | Add breed and date-range filters to implemented pagination |
| **Realtime delivery** | SignalR, email, or push delivery on top of persisted in-app notifications |
| **Cloud storage** | Azure Blob (or similar) via `IFileStorageService` |
| **Production readiness** | Docker, health checks, monitoring, deployment |

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
User logs in
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
Owners receive lifecycle-aware in-app notifications
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
| Validation | FluentValidation |
| ORM | Entity Framework Core 10 |
| Geospatial | NetTopologySuite, SQL Server `geography` |
| Read queries | Dapper (nearby search, matching candidates, match listing, report discovery, owner reports, notifications) |
| Identity | ASP.NET Core Identity |
| Authentication | JWT Bearer |
| Storage | Local filesystem (`IFileStorageService`) |
| Testing | xUnit, FluentAssertions (Domain + Application tests) |
| CI | GitHub Actions |
| API exploration | OpenAPI (Development), Postman collection |

---

## 10. Project Structure

```text
RescueLink/
├── src/
│   ├── RescueLink.API/              # Web API, controllers, Program.cs
│   ├── RescueLink.Application/      # Features, CQRS, validation, abstractions
│   ├── RescueLink.Domain/           # PetReport, enums, GeoLocation
│   ├── RescueLink.Infrastructure/   # JWT, LocalFileStorageService
│   └── RescueLink.Persistence/      # DbContext, migrations, repositories, Dapper
├── tests/
│   ├── RescueLink.Domain.Tests/
│   └── RescueLink.Application.Tests/
├── postman/                         # API request collection
├── .github/workflows/dotnet.yml     # CI pipeline
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

Domain methods include `UpdateDetails()`, `Resolve()`, `Cancel()`, `AddPhoto()`, `SetPrimaryPhoto()`, and `RemovePhoto()`.

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
| POST | `/api/auth/login` | Anonymous | Login and receive JWT |

### Pet Reports (`/api/pet-reports`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/pet-reports` | Required | Create Lost/Found report |
| GET | `/api/pet-reports` | Anonymous | Public active report discovery with filters and pagination |
| GET | `/api/pet-reports/mine` | Required | List the caller's reports with type/status filters |
| GET | `/api/pet-reports/{id}` | Anonymous | Get report details |
| PUT | `/api/pet-reports/{id}` | Required | Update an owned active report and recalculate suggestions |
| GET | `/api/pet-reports/nearby` | Anonymous | Nearby active reports |
| POST | `/api/pet-reports/{id}/photos` | Required | Upload photo (multipart) |
| PATCH | `/api/pet-reports/{reportId}/photos/{photoId}/primary` | Required | Set primary photo |
| DELETE | `/api/pet-reports/{reportId}/photos/{photoId}` | Required | Delete photo |
| PATCH | `/api/pet-reports/{id}/resolve` | Required | Mark active report as resolved |
| PATCH | `/api/pet-reports/{id}/cancel` | Required | Cancel active report |
| GET | `/api/pet-reports/{id}/matches` | Required | List owner-visible match suggestions |

### Pet Report Matches (`/api/pet-report-matches`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| PATCH | `/api/pet-report-matches/{matchId}/confirm` | Required | Confirm match for the caller's report |
| PATCH | `/api/pet-report-matches/{matchId}/reject` | Required | Reject a suggested match |

### Notifications (`/api/notifications`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | Required | Paginated owner notifications; supports `unreadOnly` |
| PATCH | `/api/notifications/{id}/read` | Required | Idempotently mark an owned notification as read |

**Authorization rules:** Only report owners can modify reports/photos or view their report's matches. A match can be managed only by the owner of its Lost or Found report. Notification queries are always scoped to the authenticated user, and only the owner can mark a notification as read.

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

Response includes `accessToken` — use as `Authorization: Bearer {token}`.

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
  }
}
```

### JWT secret (required)

The JWT signing key **must not** be committed. Configure it via User Secrets:

```bash
dotnet user-secrets set "Jwt:SecretKey" "your-super-secret-key-at-least-32-bytes-long" --project src/RescueLink.API
```

The secret must be at least **32 bytes**.

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

Nearby search, matching candidate discovery, match listing, public/owner report discovery, and notification listing use **Dapper** against the same SQL Server database for efficient read queries.

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

Examples of covered behavior:

- PetReport creation and photo limits
- Resolve/cancel domain rules
- Nearby query validation
- Authentication validators
- Photo upload/delete/set-primary handlers
- Image signature validation and rollback behavior
- Resolve/cancel handlers and ownership rules
- Matching score combinations and distance bands
- Domain Event matching handler
- Two-sided confirm/reject handlers
- Automatic report resolution after mutual confirmation
- Report update authorization and active-state rules
- Match recalculation and stale-suggestion cleanup flow
- Notification entity rules, event observers, owner isolation, listing, and mark-as-read handlers

CI runs build and tests on every push/PR to `master` via GitHub Actions.

Integration tests for full API flows are planned.

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
- [ ] **Confirmed-match notifications and unread-count endpoint**
- [ ] **Delete/archive reports**
- [ ] **Realtime/email/push delivery** (SignalR → email/push)
- [ ] **Admin role** for moderation
- [ ] **Azure Blob Storage** provider
- [ ] **Web / mobile clients**
- [ ] **Docker & CI/CD deployment**
- [ ] **Health checks & structured logging**
- [ ] **Integration tests**

Success will be measured not only by report volume, but by **how many lost pets are reunited through the platform**.

---

## 21. Security Considerations

- Passwords hashed via ASP.NET Core Identity (complexity rules enforced)
- JWT Bearer authentication for protected endpoints
- Users can only manage their own reports, photos, match decisions, and notifications
- Match details are visible only to the owner of the requested report
- Notification list/read operations are scoped to the authenticated user's ID
- Two-sided confirmation prevents one party from unilaterally confirming a reunion
- File uploads validated by content type (magic bytes), size, and path traversal protection on delete
- JWT secret and connection strings must be kept out of source control (User Secrets / environment variables)
- Sensitive data (passwords, tokens) must not appear in logs
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
