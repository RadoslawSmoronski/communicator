🌐 [English](README.md) | 🇵🇱 **[Polski](README-pl.md)**

# Chat Communicator

> **Uwaga:** projekt jest rozwijany na **wspólnym, tymczasowym branchu development**. Mogą występować błędy, niedociągnięcia i zmiany łamiące kompatybilność — prace nad stabilizacją trwają.

https://communicator.rsmoronski.pl

Backend: .NET 10, PostgreSQL, JWT, SignalR. (api docs: https://radoslawsmoronski.github.io/communicator/)

Frontend: React + nginx (proxy do API).

Szczegółowy opis architektury i API: [`api/README-pl.md`](api/README-pl.md).

---

## Szybki start (Docker — cały stack)

**Wymagania:** Docker z **Compose v2** (`docker compose`) i **uruchomiony daemon** — np. Docker Desktop (Mac/Windows), albo **Docker Engine** na Linuxie (Ubuntu itd.); chodzi o to samo narzędzie w CLI, niekoniecznie aplikację „Desktop”.

Z **katalogu głównego** repozytorium (obok `docker-compose.yml`):

```bash
docker compose up --build
```

Po starcie:

| Co            | Adres                          |
|---------------|--------------------------------|
| Aplikacja web | http://localhost:3010          |
| API           | http://localhost:5205          |
| Swagger (Dev) | http://localhost:5205/swagger  |
| PostgreSQL    | `localhost:5432` (user/hasło jak w compose) |

Domyślne dane bazy w compose są zgodne z konfiguracją API w obrazie (host `db`, baza `portfolio` itd.).

---

## E-mail (ważne)

Rejestracja i logowanie zakładają **potwierdzenie adresu e-mail** (`RequireConfirmedEmail` w Identity). Bez działającego **SMTP** backend nie wyśle linku aktywacyjnego / resetu hasła — użytkownik nie dokończy rejestracji w typowym flow.

W `api/Api/API/appsettings.json` są sekcje **`SmtpEmailSettings`** oraz adresy w **`ConfirmEmailMessageSettings`** / **`RecoveryPasswordMessageSettings`** (linki kierują na frontend, np. `http://localhost:3010/...`). Uzupełnij **prawdziwy** serwer SMTP (np. konto testowe [Mailtrap](https://mailtrap.io/) lub inny) **przed** budową obrazu API, albo nadpisz te wartości zmiennymi środowiskowymi w `docker-compose.yml` dla serwisu `api`.

---

## Backend bez Dockera (skrót)

1. **.NET 10 SDK** + **PostgreSQL** (np. lokalnie lub sam kontener `db`).
2. W `api/Api/API/appsettings.json` (lub `appsettings.Development.json`) ustaw **`ConnectionStrings:DefaultConnection`**, **JWT**, **SMTP**, **CORS** (`AllowedOrigins` — np. `http://localhost:3010`).
3. Migracje z katalogu `api/Api`:

   ```bash
   cd api/Api
   dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
   ```

4. Uruchomienie API:

   ```bash
   dotnet run --project API/API.csproj
   ```

Potrzebne jest narzędzie **EF Core** (`dotnet tool install --global dotnet-ef`), jeśli jeszcze go nie masz.

---

## Status

Repozytorium w fazie rozwoju; powyższa instrukcja opisuje **aktualny** sposób uruchomienia pełnego stacku z głównego `docker-compose.yml`.
