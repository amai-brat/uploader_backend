# Uploader Backend — Documentation

The **Uploader Backend** is a file-upload microservice built on **ASP.NET Core 10.0**
with **Dapper** and **SQLite**. It stores files on local disk, generates thumbnails
for images and videos with **ffmpeg**, and supports anonymous uploads as well as
uploads attributed to a **Twitch** account via an API key.

- Frontend: [`amai-brat/uploader_frontend`](https://github.com/amai-brat/uploader_frontend)
- Docker image: [`amaicock/uploader-api`](https://hub.docker.com/r/amaicock/uploader-api)
- License: [AGPL-3.0](../LICENSE)

> This repository is the **backend only**. All API endpoints live under the
> `/api` route group.

## Contents

| Doc | Description |
| --- | --- |
| [Overview](#overview) | Project purpose, goals and non-goals |
| [Solution structure](#solution-structure) | Layout of the codebase and layers |
| [Database scheme](#database-scheme) | SQLite tables, migrations and model |
| [Features](#features) | Functional capabilities of the application |
| [HTTP API](#http-api) | Endpoint reference with request/response shapes |
| [Configuration](#configuration) | App settings, env vars and connection strings |
| [Build & run](#build--run) | Local and Docker deployment |

## Overview

### Goals

- Provide a simple, dependency-free file upload endpoint (max 50 MB by default).
- Auto-generate and serve thumbnails (`/t/`) for images and videos.
- Allow anonymous uploads (files identified by a random key).
- Optionally associate uploads with a Twitch user and let the user list their
  own uploads.

### Non-goals

- No user authentication other than Twitch OAuth2 (used only to mint an API key).
- No multi-tenant / cloud storage abstraction beyond local disk.
- No server-side transcoding of media — only static thumbnails via ffmpeg.

---

## Solution structure

The solution uses a **clean-architecture–style modular layout** with a shared
`Core` project and thin presentation layer. Projects are connected via
composition roots (`Entry` classes) and referenced by the Web host project.

```
uploader_backend/
├── .dockerignore
├── .editorconfig
├── .gitignore
├── Uploader.slnx                 # .NET 10 solution (slnx)
├── compose.yaml                  # docker compose service definition
├── Dockerfile                    # multi-stage build (build → runtime, ffmpeg)
├── README.md
├── changelog.md
├── migrations/
│   ├── 0_init.sql                # creates Uploads table
│   └── 1_add_user.sql            # creates Users table + Uploads.UserId FK
└── src/
    ├── Directory.Build.props      # net10.0, nullable, implicit usings
    ├── Directory.Packages.props  # central package versions
    └── Uploader.Web/              # host / entry point (Program.cs)
        └── appsettings*.json
    ├── Uploader.Core/             # entities, interfaces, Result, options
    ├── Uploader.Feature/          # HTTP endpoints (markers + request/response DTOs)
    ├── Uploader.Infrastructure/   # Dapper repos, file storage, ffmpeg, background worker
    ├── Uploader.Integration/      # Twitch OAuth2 client, DI wiring
    └── Uploader.Web/
```

### Layers

| Project | Role | Public API | Key contents |
| --- | --- | --- | --- |
| `Uploader.Core` | Domain + contracts | Entities, `I*` abstractions, `Result`, `AppSettings` | `Upload`, `User`, repository & storage interfaces, thumbnail job queue, `Result<T>`, `AppSettings` |
| `Uploader.Integration` | External boundaries | `ITwitchClient`, `ApiResponse<T>` | Twitch OAuth2 validation client, `TwitchClientOptions`, DI registration |
| `Uploader.Feature` | Presentation | `IEndpoint` markers, request/response records | `POST /api/upload`, `GET /api/object`, `POST /api/delete`, `POST /api/key`, `GET /api/uploads` |
| `Uploader.Infrastructure` | Implementation | Dapper repositories, `LocalFileStorage`, ffmpeg service, background worker | `DapperContext`, `UploadRepositoryDapper`, `UserRepositoryDapper`, `ThumbnailBackgroundWorker`, `MediaThumbnailService`, `LocalFileStorage` |
| `Uploader.Web` | Host | `Program.cs` | Composition root, middleware pipeline, endpoint mapping |

### Dependency direction

```
Uploader.Web
   │  uses
   ├──► Uploader.Feature   ─┐
   │       ─┐              │
   │       ▼              │ references
   ├──► Uploader.Infrastructure ─┘
   │        ▲
   │        │ references
   └────────┴──► Uploader.Core
   │
   └────────► Uploader.Integration
```

- **Core** has no dependencies on the rest of the solution (framework only).
- **Feature** depends on Core + Integration (uses DTOs and external clients).
- **Infrastructure** depends on Core (implements the abstractions).
- **Integration** is a leaf that talks to the Twitch API.
- **Web** wires everything together.

---

## Database scheme

The database is **SQLite** (`Data Source=uploader.db` by default). Schema is
created by applying the migration files in order — see the README quick start:

```bash
sqlite3 uploader.db < migrations/0_init.sql
sqlite3 uploader.db < migrations/1_add_user.sql
```

### Tables

#### `Uploads`

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | INTEGER PK | Autoincrement primary key |
| `UploadTime` | TEXT | Upload timestamp (UTC) |
| `FileId` | TEXT | Unique random 6-char file id |
| `OriginalFilename` | TEXT | Filename chosen by the uploader client |
| `Key` | TEXT | Random key used to delete the file |
| `ChecksumMd5` | TEXT | MD5 of the uploaded content |
| `ContentType` | TEXT | MIME type (nullable) |
| `Extension` | TEXT | Lowercased file extension (nullable) |
| `Size` | INTEGER | File size in bytes |
| `UserAgent` | TEXT | Uploader's User-Agent (nullable) |
| `RemoteIpAddress` | TEXT | Uploader's IP, from `X-Forwarded-For` or socket (nullable) |
| `IsDeleted` | INTEGER | Soft-delete flag (`0`/`1`) |
| `UserId` | INTEGER FK → `Users(Id)` | Owning user (nullable, migration 1) |

Unique index on `FileId`.

#### `Users`

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | INTEGER PK | Autoincrement primary key |
| `TwitchUserId` | TEXT | Twitch user id (unique) |
| `TwitchUsername` | TEXT | Twitch login/username |
| `ApiKey` | TEXT | Secret used to authenticate the user's uploads |

Unique index on `TwitchUserId`. `Uploads.UserId` references `Users.Id`.

### Entity model

```
User 1 ──< many Uploads      (relation via UserId, filtered by IsDeleted = 0)
```

- `Upload.User` back-reference is populated in `GetByApiKeyWithUploadsAsync`
  through a Dapper split-query join.

---

## Features

- **File upload** — POST a file via `multipart/form-data`, up to a configurable
  limit (default **50 MB**). Files are validated for presence/size and an MD5
  checksum is computed.
- **Delete by key** — remove an upload using its generated `key`
  (`/api/delete?key=...`). Deletes are soft-flagged in DB and physically removed
  from disk (source + thumbnail).
- **Thumbnails** — images and videos are automatically enqueued for thumbnail
  generation. ffmpeg resizes them to `min(320, width)` and stores the result
  under `<StoragePath>/t/`. A background worker processes the job queue
  asynchronously with a 30 s per-job timeout.
- **Anonymous uploads** — anyone can upload and receive a public `link` and a
  `delete` link; nothing is required.
- **Twitch integration** — validate a Twitch OAuth2 token to (create or reuse) a
  user and issue an `api_key`. Uploads can be sent with `X-Api-Key` and listed
  back via `/api/uploads`.

---

## HTTP API

All endpoints are mounted under `/api`. Responses are JSON; each endpoint serializes
its DTOs through a `System.Text.Json` `JsonSerializerContext`.

### `POST /api/upload`

Upload a file as `multipart/form-data` (field `File`). Optional `X-Api-Key`
header associates the file with a Twitch user.

Validates:
- file must be present and non-empty;
- size must not exceed `App:MaxFileSize`.

**Response `200 OK`** — `UploadResponse`

| Field | JSON name | Description |
| --- | --- | --- |
| File id | `id` | Random file identifier |
| Extension | `ext` | Lowercased extension |
| Content type | `type` | MIME type |
| Checksum | `checksum` | MD5 hex |
| Key | `key` | Delete key |
| Link | `link` | `<BaseUrl>/<id><ext>` — public URL |
| DeleteLink | `delete` | `<BaseUrl>/api/delete?key=<key>` |

`400 Bad Request` — `UploadErrorResponse { error }`

### `POST /api/key`

Request body: `{ "token": "<oauth2_token>" }`. The Twitch token is validated; a
`User` row is created if missing and an API key is generated.

**Response `200 OK`** — `GetApiKeyResponse { api_key }`
`<status> Failure` — `GetApiKeyErrorResponse { error }`

### `GET /api/object?id=<fileId>`

Returns metadata about a file (does not serve the file bytes).

**Response `200 OK`** — `GetObjectResponse`

| Field | JSON name | Description |
| --- | --- | --- |
| File id | `id` | |
| Content type | `type` | nullable |
| Upload date (ms) | `date` | Unix milliseconds |
| Checksums | `checksums.md5` | MD5 |
| Filename | `name` | `<FileId><Extension>` |

`404 Not Found` when the file id is unknown.

### `POST /api/delete?key=<key>`

Marks the file deleted and removes it from disk (source + thumbnail).

**Response `200 OK`** — `DeleteResponse { success }`
`404 Not Found` when no upload matches the key.

### `GET /api/uploads?X-Api-Key=<apiKey>`

Returns the authenticated user's non-deleted uploads.

**Response `200 OK`** — `AuthGetObjectsResponse { objects[] }`

| Field | JSON name | Description |
| --- | --- | --- |
| File id | `id` | |
| Content type | `type` | nullable |
| Upload date (ms) | `date` | Unix milliseconds |
| Checksums | `checksums.md5` | MD5 |
| Filename | `name` | Original filename |
| Extension | `ext` | nullable |
| Key | `key` | |

`401 Unauthorized` when the API key is missing or unknown.

### Error handling

Unhandled exceptions are converted to a `500 ProblemDetails` JSON body that
includes a `traceId` and `timestamp` (see
`Uploader.Web/Core/GlobalExceptionHandler.cs`).

---

## Configuration

Configuration is layered: `appsettings.json` → `appsettings.{env}.json` →
environment variables with the `ENV_` prefix.

### `App`

| Key | Default | Description |
| --- | --- | --- |
| `StoragePath` | *(required)* | Root directory for uploaded files |
| `BaseUrl` | *(required)* | Public base URL, used to build `link`/`delete` URLs |
| `MaxFileSize` | `52428800` (50 MiB) | Maximum upload size in bytes |

### `ConnectionStrings:Sqlite`

| Default | Description |
| --- | --- |
| `Data Source=uploader.db` | Path / connection string for SQLite |

### `Integration:Twitch`

| Key | Default | Description |
| --- | --- | --- |
| `BaseUrl` | `https://id.twitch.tv/` | Twitch Hub base URL for OAuth2 calls |

### Environment variables

| Variable | Maps to |
| --- | --- |
| `ENV_App__StoragePath` | `App:StoragePath` |
| `ENV_App__BaseUrl` | `App:BaseUrl` |
| `ENV_ConnectionStrings__Sqlite` | `ConnectionStrings:Sqlite` |
| `ENV_Integration__Twitch__BaseUrl` | `Integration:Twitch:BaseUrl` |

`AppSettingsValidateOptions` fails startup if `StoragePath` or `BaseUrl` are empty.

---

## Build & run

### Prerequisites

- .NET SDK 10.0
- `ffmpeg` installed (used for thumbnail generation)
- A `SQLitePCL`-compatible platform

### Local

```bash
mkdir -p /tmp/uploader
sqlite3 /tmp/uploader/uploader.db < ./migrations/0_init.sql
sqlite3 /tmp/uploader/uploader.db < ./migrations/1_add_user.sql   # and any further migrations

# from src/Uploader.Web
dotnet run --launch-profile http
```

The `http` launch profile uses `appsettings.Development.json`
(`StoragePath=/tmp/uploader`, `BaseUrl=http://localhost:5000`).

### Docker

```bash
docker compose up            # builds the image
# or run the published image directly:
docker compose up --build    # image: amaicock/uploader-api:latest
```

`compose.yaml` sets:

```yaml
environment:
  ENV_App__StoragePath: /tmp/uploader
  ENV_App__BaseUrl: http://localhost:5000/
  ENV_ConnectionStrings__Sqlite: Data Source=uploader.db
volumes:
  - /tmp/uploader:/tmp/uploader
  - /tmp/uploader/uploader.db:/app/uploader.db
```

The `Dockerfile` uses a multi-stage build and installs **ffmpeg** in the runtime
image (`mcr.microsoft.com/dotnet/aspnet:10.0-alpine`), exposing port **5000**.

### Contributing

- Create a branch from `dev`, e.g. `feature/{issue_number}/{name}`.
- Open issues and pull requests as needed.

### License

[AGPL-3.0](../LICENSE)
