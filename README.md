# JarconiRestaurant – Web API (.NET + EF Core + JWT)

A clean, extensible Web API for a website with three modules: **Reservations**, **Menu**, and **News**.  
Authentication is **JWT-based** with two roles: **Admin** and **Client**.

- **Framework:** ASP.NET Core (.NET 8)  
- **ORM:** Entity Framework Core (PostgreSQL / Npgsql)  
- **Auth:** JWT (Bearer)  
- **Docs:** Swagger/OpenAPI

---

## Features

- Public endpoints for **Menu** and **News**
- Auth endpoints (**register**, **login**, **me**)
- **Reservations** with conflict checks, deadlines, and statuses
- **Admin** CRUD for Menu (categories/items) and News (posts, publish)
- Role-based access: `[Authorize(Roles="Admin")]`, `[Authorize(Roles="Client,Admin")]`
- EF Core Fluent configs, indexes, constraints, UTC timestamps

---

## Tech Stack

- **.NET 8** (ASP.NET Core Web API)  
- **EF Core** (Npgsql provider)  
- **JWT Bearer** auth  
- **Swagger** for interactive API docs

---

## Project Structure

JarconiRestaurant/
├─ Controllers/
│ ├─ AuthController.cs
│ ├─ ReservationsController.cs
│ ├─ MenuController.cs
│ └─ NewsController.cs
├─ Domain/
│ ├─ Common/BaseEntity.cs
│ ├─ Enums/{UserRole, ReservationStatus}.cs
│ ├─ Users/User.cs
│ ├─ Reservations/Reservation.cs
│ ├─ Menu/{MenuCategory, MenuItem}.cs
│ └─ News/NewsPost.cs
├─ Data/
│ ├─ AppDbContext.cs
│ └─ Configurations/
│ ├─ UserConfig.cs
│ ├─ ReservationConfig.cs
│ ├─ MenuCategoryConfig.cs
│ ├─ MenuItemConfig.cs
│ └─ NewsPostConfig.cs
├─ Auth/Jwt/{JwtOptions.cs, JwtTokenService.cs}
├─ DTOs/
│ ├─ Auth/{RegisterDto, LoginDto, AuthResponse}.cs
│ ├─ Reservations/{CreateReservationDto, UpdateReservationDto, ReservationVm}.cs
│ ├─ Menu/{CreateMenuCategoryDto, CreateMenuItemDto, UpdateMenuItemDto, MenuVm}.cs
│ └─ News/{CreateNewsDto, UpdateNewsDto, NewsVm}.cs
└─ Migrations/ (created after first migration)

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL (local or hosted)
- (Optional) EF CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Install dependencies
If you’re starting fresh, the key packages are:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
```

### Configuration

`appsettings.json`

```json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=jarconi;Username=postgres;Password=postgres;SSL Mode=Disable;"
  },
  "Jwt": {
    "Issuer": "JarconiRestaurant",
    "Audience": "JarconiRestaurant.Clients",
    "Key": "REPLACE_WITH_LONG_RANDOM_SECRET_64_CHARS",
    "ExpiresMinutes": 60
  }
}
```

⚠️ Security note: never commit real secrets. Use user-secrets, env vars, or CI/CD secrets.

### Database
Create DB schema with EF Core migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Run
```bash
dotnet run
```

Swagger UI:

- HTTPS (default launchSettings): https://localhost:4003/swagger  
- HTTP: http://localhost:4000/swagger  

If your client tool complains about the dev certificate, temporarily allow self-signed certs in your client (e.g., Postman setting).

---

## Authentication & Roles

- **Register** creates a Client by default.  
- **Promote to Admin** (manual, for development):

```sql
-- Example: set an existing user to Admin (UserRole.Admin = 1)
UPDATE users SET "Role" = 1 WHERE email = 'you@example.com';
```

⚠️ Important: if you change a user’s role in DB, re-login to get a new JWT (the role is embedded in the token).

JWT Claims: `sub` (user id), `email`, `role` (Admin|Client).  
Add header to protected calls:

```makefile
Authorization: Bearer <YOUR_JWT_TOKEN>
```

---

## API Overview

### Auth
- `POST /api/auth/register` — register as Client  
- `POST /api/auth/login` — get JWT  
- `GET /api/auth/me` — current user (requires JWT)  

### Menu
- `GET /api/menu` — public list (published & available)  
- `POST /api/menu/categories` — Admin  
- `POST /api/menu/items` — Admin  
- `GET /api/menu/items/{id}`  
- `PUT /api/menu/items/{id}` — Admin  
- `DELETE /api/menu/items/{id}` — Admin  

### News
- `GET /api/news` — public list (published)  
- `GET /api/news/{slug}` — public post  
- `POST /api/news` — Admin  
- `PUT /api/news/{id}` — Admin  
- `PATCH /api/news/{id}/publish` — Admin  
- `DELETE /api/news/{id}` — Admin  

### Reservations
- `POST /api/reservations` — Client/Admin (Admin can book for another user via ForUserId)  
- `GET /api/reservations/my` — Client/Admin (current user’s reservations)  
- `GET /api/reservations/{id}` — Client/Admin (owner or Admin)  
- `PUT /api/reservations/{id}` — Client/Admin (owner or Admin; client before deadline)  
- `DELETE /api/reservations/{id}` — Client/Admin (cancel; owner or Admin; client before deadline)  
- `PATCH /api/reservations/{id}/confirm` — Admin  

Business rules (v1 highlights):
- Times are UTC  
- Slot step: 30 minutes; default duration 90m (allowed: 60/90/120)  
- Conflict check by (TableNumber, DateTimeStartUtc, DurationMin) for Pending/Confirmed  
- Client can edit/cancel until 2h before start  

---

## Sample Requests (cURL)

### Login → get token
```bash
curl -X POST https://localhost:4003/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"you@example.com","password":"YourPass"}'
```

### Create a category (Admin)
```bash
curl -X POST https://localhost:4003/api/menu/categories \
  -H "Authorization: Bearer <TOKEN>" -H "Content-Type: application/json" \
  -d '{"name":"Pizzas","sortOrder":1}'
```

### Create a reservation (Client/Admin)
```bash
curl -X POST https://localhost:4003/api/reservations \
  -H "Authorization: Bearer <TOKEN>" -H "Content-Type: application/json" \
  -d '{"tableNumber":1,"dateTimeStartUtc":"2099-01-01T12:00:00Z","durationMin":90,"partySize":2,"comment":"Window seat"}'
```

---

## Postman Collection

Optionally include a collection file under `docs/JarconiRestaurant.postman_collection.json` with variables:

- `baseUrl` (e.g., https://localhost:4003)  
- `token` (set after login)  

Ready requests for Auth, Menu, News, Reservations, and negative checks (401/403).

---

## Development Notes

- Swagger is enabled in Development profile.  
- Indices & constraints are handled via Fluent configs.  
- All timestamps are UTC (CreatedAtUtc).  
- Price precision is configured for decimal(10,2) (Postgres compatible).  

---

## Roadmap

- Refresh tokens & token revocation  
- Rate limiting for public POSTs  
- Audit logs (who changed what, when)  
- Soft-delete & restore (for News/Menu)  
- I18n for Menu items  
- Caching for public endpoints  

---

## License

MIT (or your choice). Add a LICENSE file accordingly.

---

## Troubleshooting

- **403 on admin endpoints**: ensure your token is Admin (re-login after updating DB role).  
- **Dev HTTPS issues**: Postman → Settings → Disable SSL certificate verification (for local dev only).  
- **Migrations issues**: verify connection string, database exists, and your user has permissions.  
