# Kanban Pro

> **Проєктування системи управління задачами для розробників на основі Kanban-методології**

A full-stack task management system built with **ASP.NET Core 8 + Blazor WebAssembly + PostgreSQL**, following the Kanban methodology.

## Features

- **Kanban Boards** — Create personal boards with columns and cards
- **Task Management** — Priority levels (Low / Medium / High / Critical), due dates, descriptions
- **Teams** — Create teams, invite members via link, share boards
- **Real-time** — Live updates via SignalR
- **Authentication** — JWT-based login/register with ASP.NET Identity
- **Roles** — Admin and Developer roles
- **Admin Panel** — View and manage all boards in the system
- **Profile** — Update display name and change password
- **Responsive** — Works on mobile, tablet and desktop

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Blazor WebAssembly (.NET 8) |
| Backend | ASP.NET Core 8 Web API |
| Database | PostgreSQL 16 + EF Core |
| Auth | ASP.NET Identity + Bearer Token |
| Real-time | SignalR |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / Web) |

## Architecture

```
src/
├── KanbanApp.Domain/         # Entities, Repository Interfaces
├── KanbanApp.Application/    # Services, DTOs, Service Interfaces  
├── KanbanApp.Infrastructure/ # EF Core, Repositories, Migrations
└── KanbanApp.Web/
    ├── KanbanApp.Web/        # ASP.NET Core API, Controllers, Hubs
    └── KanbanApp.Web.Client/ # Blazor WASM Frontend
```

## Local Development

### Prerequisites
- .NET 8 SDK
- Docker (for PostgreSQL)

### Run locally

```bash
# Start PostgreSQL
docker compose up -d

# Run the application
dotnet run --project src/KanbanApp.Web/KanbanApp.Web
```

App will be available at `https://localhost:5001` / `http://localhost:5174`

### Default admin account

A default admin account is created on first launch.  
**Change the password immediately after first login via the Profile page.**

## Deploy to Railway

See [Railway deployment guide](https://railway.app) — the `Dockerfile` is included.

Required environment variables:
- `DATABASE_URL` — PostgreSQL connection string from Railway
- `ASPNETCORE_ENVIRONMENT` — `Production`

## API Documentation

Swagger UI available at `/swagger` in Development mode.

## License

MIT
