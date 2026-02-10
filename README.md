# BrandVault API

**The backend for BrandVault — a creative agency platform for managing client assets and collecting feedback.**

![.NET](https://img.shields.io/badge/.NET-10-purple?logo=dotnet)
![EF Core](https://img.shields.io/badge/EF_Core-10-purple?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Supabase-blue?logo=postgresql)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-green)

**Frontend:** [BrandVault Client](https://github.com/hadiAlrahmani/BrandVault-Client-frontend-.git) (React + TypeScript)

---

## What Does This API Do?

This is the backend that powers the BrandVault platform. It handles everything the frontend needs:

- **User auth** — register, login, JWT tokens with auto-refresh, role-based access
- **Client & workspace management** — CRUD for organizing agency work
- **Asset versioning** — file uploads, version history, downloads
- **Comments & approvals** — feedback from both agency team and external clients
- **Public review links** — token-based access so clients can review assets without creating an account
- **Real-time updates** — SignalR hub that pushes notifications when clients submit feedback

There are **39 REST endpoints** and **1 SignalR hub** in total.

---

## Tech Stack

| | Tech | What For |
|---|---|---|
| **Framework** | ASP.NET Core 10 (.NET 10) | Web API |
| **ORM** | Entity Framework Core 10 | Database queries and migrations |
| **Database** | PostgreSQL (Supabase) | Data storage |
| **Auth** | JWT Bearer + BCrypt | Token auth + password hashing |
| **Real-time** | SignalR | WebSocket-based live notifications |
| **File Storage** | Local disk | Organized by date, with version tracking |
| **Images** | SixLabors.ImageSharp | Image processing and validation |
| **API Docs** | OpenAPI / Swagger | Auto-generated endpoint docs |

---

## The Architecture

The project is organized by **feature** — each feature has its own controller, service, interface, and DTOs all in one folder. No hunting across layers to find related code.

```
src/BrandVault.Api/
├── Program.cs                     # Startup, DI, middleware
├── Common/                        # Base entity, custom exceptions
├── Data/                          # EF Core DbContext + configurations
│   ├── AppDbContext.cs            # 9 DbSets, auto Id/CreatedAt
│   └── Configurations/            # Entity configs (PK, FK, indexes)
├── Models/                        # Database entities
│   ├── User.cs, Client.cs, Workspace.cs, Asset.cs...
│   └── Enums/                     # UserRole, AssetStatus, etc.
├── Features/                      # One folder per feature
│   ├── Auth/                      # Register, login, refresh, me, logout
│   ├── Clients/                   # Client CRUD
│   ├── Workspaces/                # Workspace CRUD + team assignments
│   ├── Assets/                    # Assets + versions + comments + approvals
│   ├── Reviews/                   # Review links + public review endpoints
│   └── Dashboard/                 # Stats aggregation
├── Hubs/
│   └── ReviewHub.cs               # SignalR for live feedback notifications
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs  # Global error handler
├── Services/FileStorage/          # File I/O (save, delete, validate)
└── Uploads/                       # Actual uploaded files (by date)
```

The idea is that if you're working on assets, everything you need is in `Features/Assets/`. No jumping between a `Controllers/`, `Services/`, `DTOs/` folder structure.

---

## Database Schema

9 tables, all using Guid primary keys and UTC timestamps.

### Entities

**User** — Agency team members
- Email (unique), name, password hash (BCrypt), role, refresh token
- Roles: `Admin`, `Manager`, `Designer`

**Client** — The brands/companies the agency works with
- Name, company, email, phone, industry
- Created by a user

**Workspace** — A project or campaign for a client
- Name, description, deadline, status
- Statuses: `Active`, `InReview`, `Completed`, `Archived`
- Belongs to a client, has team assignments

**WorkspaceAssignment** — Links users to workspaces (many-to-many)
- workspace_id + user_id

**Asset** — A file being worked on (logo, banner, etc.)
- Name, file type, current version number, status
- Statuses: `Draft`, `InReview`, `Approved`, `RevisionRequested`
- Belongs to a workspace

**AssetVersion** — Each uploaded version of an asset
- Version number (auto-incremented), file path, file size
- Path format: `Uploads/2026/02/guid_filename.ext`

**Comment** — Feedback on an asset
- Author name, author type (Agency or Client), content
- If from a public review, links to the review link

**ApprovalAction** — Approve or request revision
- Action type (Approved / RevisionRequested), optional comment
- Done by name + type (Agency or Client)

**ReviewLink** — Tokenized links for external client review
- Cryptographically random token, expiry date, active flag
- Tied to a workspace

### Naming Convention
- **Tables**: snake_case (`workspace_assignments`)
- **Columns**: PascalCase, quoted in SQL (`"CreatedAt"`)

---

## All 39 Endpoints + SignalR Hub

### Auth (5 endpoints)
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | — | Create account (first user = Admin, rest = Designer) |
| POST | `/api/auth/login` | — | Get JWT + refresh token |
| POST | `/api/auth/refresh-token` | — | Exchange refresh token for new JWT |
| GET | `/api/auth/me` | Yes | Current user info |
| POST | `/api/auth/logout` | Yes | Clear refresh token |

### Clients (5 endpoints)
| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/clients` | Yes | Any | List all |
| GET | `/api/clients/:id` | Yes | Any | Get one |
| POST | `/api/clients` | Yes | Admin, Manager | Create |
| PUT | `/api/clients/:id` | Yes | Admin, Manager | Update |
| DELETE | `/api/clients/:id` | Yes | Admin | Delete |

### Workspaces (8 endpoints)
| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/workspaces?clientId=` | Yes | Any | List (optional filter) |
| GET | `/api/workspaces/:id` | Yes | Any | Get details |
| POST | `/api/workspaces` | Yes | Admin, Manager | Create |
| PUT | `/api/workspaces/:id` | Yes | Admin, Manager | Update |
| DELETE | `/api/workspaces/:id` | Yes | Admin | Delete |
| GET | `/api/workspaces/:id/assignments` | Yes | Any | List team |
| POST | `/api/workspaces/:id/assignments` | Yes | Admin, Manager | Add member |
| DELETE | `/api/workspaces/:id/assignments/:userId` | Yes | Admin, Manager | Remove member |

### Assets (12 endpoints)
| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/assets?workspaceId=` | Yes | Any | List in workspace |
| GET | `/api/assets/:id` | Yes | Any | Detail + versions |
| POST | `/api/assets` | Yes | Admin, Manager, Designer | Upload (multipart, 50MB max) |
| PUT | `/api/assets/:id` | Yes | Admin, Manager | Update name/status |
| DELETE | `/api/assets/:id` | Yes | Admin | Delete asset + files |
| GET | `/api/assets/:id/versions` | Yes | Any | Version history |
| POST | `/api/assets/:id/versions` | Yes | Admin, Manager, Designer | Upload new version (multipart) |
| GET | `/api/assets/:id/versions/:v/download` | Yes | Any | Download file |
| GET | `/api/assets/:id/comments` | Yes | Any | List comments |
| POST | `/api/assets/:id/comments` | Yes | Any | Add comment |
| GET | `/api/assets/:id/approvals` | Yes | Any | List approvals |
| POST | `/api/assets/:id/approvals` | Yes | Admin, Manager | Approve / request revision |

### Review Links (4 endpoints)
| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/review-links?workspaceId=` | Yes | Any | List links |
| POST | `/api/review-links` | Yes | Admin, Manager | Create link |
| PUT | `/api/review-links/:id` | Yes | Admin, Manager | Toggle / update |
| DELETE | `/api/review-links/:id` | Yes | Admin | Delete |

### Public Review (5 endpoints — no auth needed)
| Method | Route | Description |
|---|---|---|
| GET | `/api/reviews/:token` | Get workspace + assets via review link |
| GET | `/api/reviews/:token/assets/:id` | Asset detail |
| POST | `/api/reviews/:token/assets/:id/comments` | Submit comment as client |
| POST | `/api/reviews/:token/assets/:id/approvals` | Submit approval as client |
| GET | `/api/reviews/:token/assets/:id/versions/:v/download` | Download/view file |

### Dashboard (1 endpoint)
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/dashboard` | Yes | Stats (clients, workspaces, assets, comments, approvals) |

### SignalR Hub
**URL:** `ws://localhost:5248/hubs/reviews?access_token=JWT`

| Type | Name | Description |
|---|---|---|
| Method | `JoinWorkspace(id)` | Subscribe to a workspace's notifications |
| Method | `LeaveWorkspace(id)` | Unsubscribe |
| Event | `NewComment` | Fired when a client posts a comment via review link |
| Event | `NewApproval` | Fired when a client approves / requests revision |

---

## Authentication

JWT-based with refresh tokens. Here's the flow:

```
1. User logs in → gets accessToken (60 min) + refreshToken (7 days)
2. Every request sends: Authorization: Bearer <accessToken>
3. When access token expires, frontend sends refresh token → gets new pair
4. Logout clears the refresh token server-side
```

**JWT claims:** user ID, email, name, role

**Password storage:** BCrypt hash — raw passwords are never stored

**First user to register becomes Admin.** Everyone after that is a Designer by default.

**SignalR note:** WebSockets can't send custom headers, so the JWT goes in the query string: `?access_token=xxx`. The `OnMessageReceived` event in Program.cs handles extracting it.

---

## Role Permissions

| | Admin | Manager | Designer |
|---|:---:|:---:|:---:|
| View everything | Yes | Yes | Yes |
| Create/edit clients & workspaces | — | Yes | — |
| Delete anything | Yes | — | — |
| Upload assets & versions | — | Yes | Yes |
| Edit/delete assets | Yes | Yes | — |
| Manage team & review links | — | Yes | — |
| Comment | Yes | Yes | Yes |
| Approve / request revision | Yes | Yes | — |

---

## File Storage

Files are saved to disk (not in the database). Here's how it works:

- **Location:** `Uploads/` directory at the project root
- **Organization:** `{year}/{month}/{guid}_{originalfilename}` — for example `Uploads/2026/02/a1b2c3d4_logo.png`
- **Max size:** 50 MB per file
- **Allowed types:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.svg`, `.pdf`, `.ai`, `.psd`, `.fig`, `.sketch`, `.ttf`, `.otf`, `.woff`, `.woff2`
- **Security:** File names are sanitized with `Path.GetFileName()` to prevent path traversal. A GUID prefix prevents filename collisions.

Versions are never deleted — uploading a new version of an asset just adds a new file. Old versions stay downloadable.

---

## Error Handling

Every error goes through one global middleware that returns consistent JSON:

```json
{ "error": "Human-readable message here" }
```

Services throw `ApiException(message, statusCode)` and the middleware catches it:
- **400** — bad input (validation errors, invalid file type, etc.)
- **401** — missing or invalid JWT
- **403** — user doesn't have the right role
- **404** — resource not found
- **500** — unexpected error (details logged server-side, generic message to client)

---

## Running It

### What You Need
- **.NET 10 SDK**
- **PostgreSQL** database (I used Supabase, but any Postgres works)

### Configuration

Update `appsettings.Development.json` with your own values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host;Database=postgres;Username=postgres;Password=your-password"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "BrandVault",
    "Audience": "BrandVault",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "FileStorage": {
    "BasePath": "Uploads",
    "MaxFileSizeMB": 50
  }
}
```

### Run

```bash
cd src/BrandVault.Api

# Restore packages
dotnet restore

# Run (includes building + applying migrations)
dotnet run
```

The API starts at **http://localhost:5248**.

Swagger docs available at `http://localhost:5248/openapi/v1.json` in development mode.

### CORS

The API allows requests from `http://localhost:3000` and `http://localhost:5173` (frontend dev servers). Update the CORS policy in `Program.cs` if your frontend runs elsewhere.

---

## Middleware Pipeline

The request flows through these in order (and responses flow back up):

1. **Exception handler** — catches all errors, returns JSON
2. **HTTPS redirect** — forces HTTPS in production
3. **CORS** — allows frontend origins
4. **Authentication** — validates JWT from header (or query string for SignalR)
5. **Authorization** — checks `[Authorize]` and role requirements
6. **Routing** — maps to the right controller action
7. **SignalR hub** — handles WebSocket connections at `/hubs/reviews`

---

## The Public Review System

This is the most interesting part of the backend. Here's what happens:

1. A Manager creates a review link for a workspace → the service generates a cryptographically random token (32 bytes, base64url-encoded)
2. The token is stored in the `review_links` table with an expiry date
3. The frontend builds a URL like `/review/abc123xyz...` and sends it to the client
4. When the client opens it, the `PublicReviewController` handles the request — **no `[Authorize]` attribute**, so no JWT needed
5. The service validates the token (exists? active? not expired?) before doing anything
6. Client can view assets, post comments, and submit approvals
7. After the client submits, the `ReviewService` broadcasts via SignalR to the `workspace_{id}` group
8. Agency users connected to that workspace get the notification instantly

All client feedback includes the `ReviewLinkId` so you can trace which review link it came from.

---

## Frontend

This repo is just the API. The frontend is a separate React app.

**Repo:** [BrandVault Client](https://github.com/hadiAlrahmani/BrandVault-Client-frontend-.git)

**Built with:** React 19, TypeScript, Vite, Tailwind CSS 4, shadcn/ui
