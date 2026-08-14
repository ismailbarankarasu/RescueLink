# RescueLink API

**RescueLink is not just a lost-pet listing API. It is a location-aware matching platform designed to connect lost pet reports with found animal reports through geospatial search and intelligent matching.**

RescueLink helps pet owners reunite with lost animals and enables people who find stray or potentially lost pets to reach the right owners. Instead of scattering information across social media, WhatsApp groups, and local communities, RescueLink brings **Lost** and **Found** reports together in one system — with real coordinates, structured animal data, and (planned) intelligent matching.

> **Current status:** The backend API foundation is in active development. Core report creation, authentication, geospatial nearby search, and photo management are implemented. The **Pet Matching Engine**, notifications, advanced filtering, and production deployment features are on the roadmap.

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
- Searching nearby active reports by distance
- Managing report photos (upload, delete, set primary)

The next major capability is the **Pet Matching Engine**, which will compare new reports against existing ones and return scored, explainable match suggestions.

---

## 4. Key Features

### Implemented

| Feature | Description |
|--------|-------------|
| **Unified PetReport model** | Single domain entity for `Lost` and `Found` reports |
| **JWT authentication** | Register, login, protected report/photo operations |
| **Geospatial nearby search** | SQL Server `geography` + NetTopologySuite; distance in meters |
| **Photo management** | Up to 5 photos per report; primary photo; local file storage |
| **CQRS + MediatR** | Commands/queries with dedicated handlers |
| **FluentValidation** | Request validation via pipeline behavior |
| **Domain rules** | Rich `PetReport` aggregate (resolve/cancel rules, photo limits) |
| **Unit tests** | Domain and Application layer test coverage |
| **CI pipeline** | GitHub Actions build and test on .NET 10 |

### Planned

| Feature | Description |
|--------|-------------|
| **Pet Matching Engine** | Score-based matching between Lost and Found reports |
| **Explainable matches** | Match score + human-readable reasons |
| **Report lifecycle API** | Resolve/cancel endpoints (domain logic exists) |
| **Advanced search & pagination** | Filter by species, breed, status, date range, etc. |
| **Notifications** | Alert users on high-confidence matches |
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

Both are stored as **PetReport** records with a `ReportType` of `Lost` or `Found`. They remain independent records, but the planned Matching Engine will compare them using shared attributes and proximity.

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
Matching Engine compares reports          ← planned
      ↓
High-confidence match is suggested       ← planned
      ↓
User is notified                         ← planned
      ↓
Owner reunites with pet
      ↓
Report is marked Resolved                ← domain ready, API planned
```

---

## 6. Pet Matching Engine

> **Status: Planned — not yet implemented**

This is the feature that distinguishes RescueLink from a basic CRUD listing API.

When a Found report is created, the system will evaluate existing Lost reports (and vice versa) using weighted criteria such as:

| Criterion | Example weight |
|-----------|----------------|
| Species match | +30 |
| Breed match | +25 |
| Color match | +15 |
| Gender match | +10 |
| Distance < 2 km | +20 |

The algorithm will live in a dedicated application/domain service — **not** inside controllers or handlers as large `if/else` blocks — so it can evolve independently.

See [Example Match Response](#14-example-match-response) for the intended API shape.

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

---

## 9. Technologies

| Area | Stack |
|------|-------|
| Runtime | .NET 10 |
| Web framework | ASP.NET Core Web API |
| Patterns | Clean Architecture, CQRS, MediatR |
| Validation | FluentValidation |
| ORM | Entity Framework Core 10 |
| Geospatial | NetTopologySuite, SQL Server `geography` |
| Read queries | Dapper (nearby search) |
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

Domain methods include `Resolve()`, `Cancel()`, `AddPhoto()`, `SetPrimaryPhoto()`, and `RemovePhoto()`.

### PetReportPhoto

Stores a `storageKey` reference (not binary data in the database). One photo can be marked as primary.

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
| GET | `/api/pet-reports/{id}` | Anonymous | Get report details |
| GET | `/api/pet-reports/nearby` | Anonymous | Nearby active reports |
| POST | `/api/pet-reports/{id}/photos` | Required | Upload photo (multipart) |
| PATCH | `/api/pet-reports/{reportId}/photos/{photoId}/primary` | Required | Set primary photo |
| DELETE | `/api/pet-reports/{reportId}/photos/{photoId}` | Required | Delete photo |

**Authorization rules:** Only the report owner can upload, delete, or change photos. Other users' reports cannot be modified.

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

> **Status: Planned — illustrative response shape**

When the Matching Engine is implemented, an endpoint such as `GET /api/pet-reports/{id}/matches` may return:

```json
{
  "sourceReportId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "matches": [
    {
      "reportId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "reportType": "Found",
      "matchScore": 87,
      "distanceMeters": 1800,
      "reasons": [
        "Same species",
        "Same breed",
        "Same gender",
        "Similar color",
        "Reported within 2 km"
      ]
    }
  ]
}
```

This keeps matching **explainable**: users see not only a score, but why the system suggested a match.

---

## 15. Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full instance)
- (Optional) Postman for API testing

### Clone and run

```bash
git clone https://github.com/your-org/RescueLink.git
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

Nearby search reads use **Dapper** against the same SQL Server database for efficient geospatial queries.

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

CI runs build and tests on every push/PR to `master` via GitHub Actions.

Integration tests for full API flows are planned.

---

## 20. Future Roadmap

- [ ] **Pet Matching Engine** with configurable scoring
- [ ] **Explainable match results** (score + reasons)
- [ ] **Resolve / Cancel report API** (`PATCH /api/pet-reports/{id}/resolve`)
- [ ] **List & filter reports** with pagination
- [ ] **Update / delete reports**
- [ ] **Notification system** (in-app → email/push/SignalR)
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
- Users can only manage their own reports and photos
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
