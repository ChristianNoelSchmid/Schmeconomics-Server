# Schmeconomics

Schmeconomics is a self-hosted personal finance and budget tracking web app. Users create **accounts**, define **categories** (budget envelopes), and log **transactions** that debit a category's balance. A **refill** operation replenishes all categories in an account by their configured refill values.

## Tech stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core (.NET 9), Entity Framework Core |
| Database | SQLite |
| Frontend | Nuxt 4, Vue 3, Nuxt UI, Tailwind CSS |
| Auth | JWT (access) + HTTP-only refresh token cookies |
| Deployment | Docker + Docker Compose |
| PWA | `@vite-pwa/nuxt` with auto-update |

## Project structure

```
Schmeconomics_CSharp/
├── Schmeconomics.Api/          # ASP.NET Core Web API
│   ├── Accounts/               # Account management
│   ├── Auth/                   # JWT middleware, auth service
│   ├── Categories/             # Category CRUD + refill logic
│   ├── Controllers/            # HTTP endpoints
│   ├── Secrets/                # Rotating JWT signing keys (DB-backed)
│   ├── Tokens/                 # JWT + refresh token providers
│   ├── Transactions/           # Transaction CRUD
│   └── Users/                  # User management
├── Schmeconomics.Entities/     # EF Core models, DbContext, migrations
├── Schmeconomics.Api.UnitTests/ # MSTest unit tests
├── schmeconomics.client/       # Nuxt.js frontend
└── compose/                    # Docker Compose + Dockerfiles
```

## Core concepts

**Accounts** are shared budget workspaces. An admin creates accounts and assigns users to them. Multiple users can belong to the same account.

**Categories** are budget envelopes within an account. Each category has:
- `Balance` — current available funds
- `RefillValue` — how much to add when a refill is triggered
- `Order` — display order (drag-and-drop sortable)

**Transactions** debit a category's balance. They record the amount, category, creator, timestamp, and optional notes. Refill events are also stored as transactions (`IsRefill = true`).

**Refill** adds each category's `RefillValue` to its current balance and records one refill transaction per category.

## Running locally (Docker)

All Docker files live in `compose/`. Create a `compose/.env` file:

```env
SERVER_PORT=5153
CLIENT_PORT=3000
DATABASE_DIRECTORY_PATH=/path/to/your/db/folder
NUXT_PUBLIC_API_BASE=[public-facing URL, if applicable. Used for CORS validation]
```

Then start the stack:

```bash
cd compose
docker compose up
```

- **API**: http://localhost:5153
- **Frontend**: http://localhost:3000

Database migrations run automatically on API startup. A default admin account (`Admin` / `admin`) is created if no admin exists.

## Running locally (without Docker)

### API

```bash
cd Schmeconomics.Api
dotnet run
```

The API reads its connection string from `appsettings.json` (`Data Source=./db/app.db`) and runs on port 5153 by default.

### Frontend

```bash
cd schmeconomics.client
npm install
npm run dev
```

See [`schmeconomics.client/README.md`](schmeconomics.client/README.md) for more frontend details.

## Authentication

The API uses a two-token scheme:

- **Access token** — short-lived JWT (15 minutes), signed with a rotating DB-backed secret, returned in the response body.
- **Refresh token** — long-lived (30 days), stored in the database, sent and received as an HTTP-only `Secure` cookie. On each refresh, the old token is rotated (new token issued, old one invalidated). Token families detect reuse attacks.

The signing secret itself is rotated every 15 minutes and stored in the database, so both old and new secrets are valid during the overlap window.

## Roles

| Role | Capabilities |
|---|---|
| `Admin` | Full access: create/delete accounts, categories, users; assign users to accounts |
| `User` | Read accounts, view/create/delete transactions, trigger refills, update their own profile |

## API endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/Auth/SignIn` | None | Sign in, get JWT + refresh cookie |
| POST | `/Auth/Refresh` | Refresh cookie | Rotate refresh token, get new JWT |
| POST | `/Auth/SignOut` | Refresh cookie | Revoke refresh token |
| GET | `/Account/All` | User | List accounts for current user |
| POST | `/Account/Create` | Admin | Create an account |
| POST | `/Account/ToggleUser` | Admin | Add/remove user from account |
| PUT | `/Account/Update/{id}` | Admin | Rename account |
| DELETE | `/Account/Delete/{id}` | Admin | Delete account |
| GET | `/Category/ForAccount/{accountId}` | User | List categories for account |
| POST | `/Category/Create` | Admin | Create category |
| PUT | `/Category/Update/{id}` | Admin | Update category name/balance/refill |
| PUT | `/Category/UpdateOrder` | Admin | Reorder categories |
| PUT | `/Category/UpdateRefillValues` | User | Update refill values for categories |
| POST | `/Category/Refill/{accountId}` | User | Trigger a refill for all categories |
| DELETE | `/Category/Delete/{id}` | Admin | Delete category |
| GET | `/Transaction/{accountId}` | User | Paginated transaction list (filter by `?categoryId=`) |
| POST | `/Transaction/{accountId}` | User | Log one or more transactions |
| PUT | `/Transaction/{transactionId}` | User | Update a transaction |
| DELETE | `/Transaction/{accountId}/{transactionId}` | User | Delete a transaction |
| GET | `/User/All` | Admin | List all users |
| GET | `/User/Current` | User | Get current user |
| POST | `/User/Create` | Admin | Create user |
| PUT | `/User/Update` | User | Update own profile (admin can target any user) |
| DELETE | `/User/Delete/{userId}` | Admin | Delete user |

## Database migrations

Migrations are in `Schmeconomics.Entities/Migrations/`. To add or apply them manually from the solution root:

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project Schmeconomics.Entities \
  --startup-project Schmeconomics.Api

# Apply pending migrations
dotnet ef database update \
  --project Schmeconomics.Entities \
  --startup-project Schmeconomics.Api
```

Migrations are also applied automatically on startup (both locally and in Docker).

## Running tests

```bash
dotnet test
```

Unit tests cover the auth service, token providers, account/category services, and secret rotation logic.
