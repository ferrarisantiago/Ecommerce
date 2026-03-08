# Ecommerce (MVC + API, .NET 8)

Portfolio-grade ASP.NET Core ecommerce solution refactored into separate MVC, API, Application, and Infrastructure projects while preserving existing MVC login/register/cart behavior.

## Architecture

```text
Clients
  |
  +-- Ecommerce (MVC UI, Razor Views/Controllers)
  |
  +-- Ecommerce.Api (REST API + Swagger + JWT)
           |
           v
     Ecommerce.Application (use-cases/services/contracts)
           |
           v
     Ecommerce.Infrastructure (EF Core, Identity, auth, seeding)
           |
           v
     Ecommerce_Datos + SQL Server
```

## Projects

- `Ecommerce` - Existing MVC storefront/admin UI.
- `Ecommerce.Api` - Web API for auth, products, and cart calculations.
- `Ecommerce.Application` - Core service contracts + business logic.
- `Ecommerce.Infrastructure` - Persistence/auth implementations and data seeding.
- `Ecommerce_Datos`, `Ecommerce_Modelos`, `Ecommerce_Utilidades` - Existing shared/data projects.
- `tests/Ecommerce.Application.Tests` - Unit tests.
- `tests/Ecommerce.Api.Tests` - Integration tests (`WebApplicationFactory`).

## Key Features

- .NET upgrade to `net8.0` across all projects.
- Swagger/OpenAPI enabled for API endpoints.
- JWT auth endpoints (`/api/auth/login`, `/api/auth/register`) with role support (`Admin`, `User`).
- Global API error handling with `ProblemDetails`.
- Serilog logging in MVC and API.
- Seed data:
  - Roles: `Admin`, `User`
  - Demo admin user
  - Sample categories/products
- Docker support with one command (`docker compose up --build`).
- 7 tests currently passing (3 unit + 4 integration).
- Admin area (`/Admin`) protected by role-based authorization:
  - Dashboard
  - Product management (CRUD + soft delete + active/inactive visibility)
  - Low-stock alerts (`Stock <= MinimumStockLevel`)
  - Sales audit report (Top Selling Products)
- Storefront uses only active products (`IsActive = true` and not soft deleted).
- Inventory-aware cart and checkout validation:
  - Product details validation
  - Cart quantity update validation
  - Checkout re-validation before order commit
  - Stock is reduced atomically when order is created.
- Sales report filters:
  - Today
  - Last 7 days
  - Last 30 days
  - All time
  - Includes totals for orders, units sold, and revenue.

## API Endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/products`
- `POST /api/cart/summary` (JWT required)

Swagger UI:

- `http://localhost:8080/swagger` (when running API in Development)

## Demo Credentials (Development)

- Email: `admin@ecommerce.local`
- Password: `Admin123!`

The account is seeded and assigned `Admin`.

## Admin Inventory Rules

- `Price` must be greater than `0`.
- `StockQuantity` (`Stock`) must be `>= 0`.
- `MinimumStockLevel` must be `>= 0`.
- Products appear in storefront only when `IsActive = true` and not soft deleted.
- `Add to cart` is disabled when stock is `0`.

## Migrations

- Added migration: `20260308070000_AddProductInventoryControls`
  - Adds `IsActive`, `MinimumStockLevel`, `UpdatedAt` to `Productos`
  - Replaces index with `IX_Productos_IsDeleted_IsActive_CreatedAt`

## Local Run

1. Restore:
   - `dotnet restore Ecommerce.sln`
2. Build:
   - `dotnet build Ecommerce.sln`
3. Run MVC:
   - `dotnet run --project Ecommerce`
   - `dotnet run --project Ecommerce --urls "http://localhost:5005"`
4. Run API:
   - `dotnet run --project Ecommerce.Api`
5. Optional HTTPS cert trust (for local HTTPS profile):
   - `dotnet dev-certs https --trust`

If your SQL Server is not local, override `ConnectionStrings:DefaulConnection` in `appsettings` or environment variables.

## Docker Run

Run all services (SQL Server + API + MVC):

- `docker compose up --build`

Endpoints:

- MVC: `http://localhost:8081`
- API: `http://localhost:8080`
- SQL Server host port: `14333`

## Tests

- `dotnet test Ecommerce.sln`

Includes:

- Unit tests for cart/price calculation rules.
- Integration tests for auth and cart API endpoints.

## Framework Upgrade Notes

- Selected target: `net8.0` (LTS).
- `net10.0` was not used because this environment currently has .NET SDKs `8.0.x` and `9.0.x` installed, and the solution/package set is already stable on .NET 8.
- Package versions were aligned to .NET 8 compatible versions for ASP.NET Core, Identity, EF Core, and JWT-related components.
