🌐 **[English](README.md)** | 🇵🇱 [Polski](README-pl.md)

# Chat Communicator

> **Note:** the project is developed on a **shared temporary `development` branch**. Bugs, rough edges, and breaking changes are possible — stabilization work is ongoing.

https://communicator.rsmoronski.pl

Backend: .NET 10, PostgreSQL, JWT, SignalR. (api docs: https://radoslawsmoronski.github.io/communicator/)

Frontend: React + nginx (proxy to the API).

Detailed architecture and API description: [`api/README.md`](api/README.md).

---

## Quick start (Docker — full stack)

**Requirements:** Docker with **Compose v2** (`docker compose`) and a **running daemon** — e.g. Docker Desktop (Mac/Windows), or **Docker Engine** on Linux (Ubuntu, etc.); you need the same CLI tooling, not necessarily the “Desktop” app.

From the **repository root** (next to `docker-compose.yml`):

```bash
docker compose up --build
```

After startup:

| Service        | URL                            |
|----------------|--------------------------------|
| Web app        | http://localhost:3010          |
| API            | http://localhost:5205          |
| Swagger (Dev) | http://localhost:5205/swagger |
| PostgreSQL     | `localhost:5432` (user/password as in compose) |

Default database settings in compose match the API image configuration (host `db`, database `portfolio`, etc.).

---

## E-mail (important)

Registration and sign-in assume **confirmed e-mail** (`RequireConfirmedEmail` in Identity). Without working **SMTP**, the backend will not send activation / password-reset links — users will not complete the usual registration flow.

`api/Api/API/appsettings.json` contains **`SmtpEmailSettings`** and URLs in **`ConfirmEmailMessageSettings`** / **`RecoveryPasswordMessageSettings`** (links point at the frontend, e.g. `http://localhost:3010/...`). Configure a **real** SMTP server (e.g. a [Mailtrap](https://mailtrap.io/) test inbox or another provider) **before** building the API image, or override those values with environment variables in `docker-compose.yml` for the `api` service.

---

## Backend without Docker (short)

1. **.NET 10 SDK** + **PostgreSQL** (locally or e.g. only the `db` container).
2. In `api/Api/API/appsettings.json` (or `appsettings.Development.json`) set **`ConnectionStrings:DefaultConnection`**, **JWT**, **SMTP**, **CORS** (`AllowedOrigins` — e.g. `http://localhost:3010`).
3. Migrations from the `api/Api` directory:

   ```bash
   cd api/Api
   dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
   ```

4. Run the API:

   ```bash
   dotnet run --project API/API.csproj
   ```

You need the **EF Core** tool (`dotnet tool install --global dotnet-ef`) if you do not have it yet.

---

## Status

The repository is under active development; the instructions above describe the **current** way to run the full stack from the root `docker-compose.yml`.
