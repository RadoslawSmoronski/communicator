🌐 [English](README.md) | 🇵🇱 [Polski](README-pl.md)
# Chat Communicator – Backend (.NET 9)

Real‑time backend do komunikatora (chat 1‑na‑1) z rejestracją użytkowników, autoryzacją JWT, znajomymi, zaproszeniami i wiadomościami w czasie rzeczywistym, zbudowany w oparciu o .NET 9, C# 13 i Clean Architecture.

---

## Spis treści

1. [Opis projektu](#opis-projektu)  
2. [Technologie](#technologie)  
3. [Architektura rozwiązania](#architektura-rozwiązania)  
4. [Funkcjonalności](#funkcjonalności)  
5. [Struktura solution](#struktura-solution)  
6. [Konfiguracja](#konfiguracja)  
7. [Uruchomienie lokalne](#uruchomienie-lokalne)  
8. [Testy](#testy)  
9. [API / Dokumentacja](#api--dokumentacja)  
10. [SignalR i komunikacja w czasie rzeczywistym](#signalr-i-komunikacja-w-czasie-rzeczywistym)  
11. [Logowanie](#logowanie)  
12. [Status projektu](#status-projektu)

---

## Opis projektu

**Chat Communicator** to backend do aplikacji czatowej, który pozwala użytkownikom:

- założyć konto, potwierdzić e‑mail, zalogować się i odświeżać tokeny,
- wysyłać i akceptować zaproszenia do znajomych,
- rozmawiać w czasie rzeczywistym za pomocą SignalR,
- zarządzać profilem (m.in. avatar użytkownika).

Projekt jest napisany z myślą o nauce i prezentacji dobrych praktyk:

- podział na warstwy (API, Application, Domain, Infrastructure, Shared),
- CQRS + MediatR,
- ASP.NET Core Identity,
- EF Core z migracjami,
- testy jednostkowe osobno dla Application i Infrastructure.

---

## Technologie

- .NET 9, C# 13  
- ASP.NET Core Web API  
- MediatR (CQRS: Commands/Queries)  
- SignalR (real‑time)  
- EF Core + Npgsql (PostgreSQL)  
- ASP.NET Core Identity (JWT auth)  
- Serilog (console + PostgreSQL sink)  
- AutoMapper  
- Swagger / OpenAPI (w środowisku Development)  
- xUnit  
- FluentAssertions  
- FakeItEasy

---

## Architektura rozwiązania

Projekt jest oparty o podejście zbliżone do **Clean Architecture**:

- **API**  
  - Kontrolery, `ChatHub` dla SignalR.  
  - Obsługa HTTP, routing, walidacja wejścia, mapowanie na komendy/zapytania.  
  - Konfiguracja Swagger/OpenAPI, obsługa wyjątków (ProblemDetails), CORS, auth.

- **Application**  
  - Logika aplikacyjna w postaci **Command/Query handlers** (MediatR).  
  - Pipeline behaviors (np. authorization).  
  - Interfejsy serwisów (porty), mapowania, konfiguracja (typed settings).

- **Domain**  
  - Encje domenowe (User, Friendship, FriendshipInvitation, Conversation, Message, RefreshToken, itp.).  
  - Logika domeny i relacje między encjami.

- **Infrastructure**  
  - `ApplicationDbContext` (EF Core + Identity).  
  - Implementacje repozytoriów i serwisów (e‑mail, pliki, wiadomości, znajomi, tokeny).  
  - Migracje bazy danych.  
  - Integracja z Serilog (logi do konsoli i PostgreSQL).

- **Shared**  
  - `Result<T>` i typy błędów.  
  - Wspólne prymitywy używane w różnych warstwach.

---

## Funkcjonalności

### Autoryzacja i użytkownicy

- Rejestracja użytkownika (email + hasło).
- Logowanie:
  - JWT access token.
  - Refresh token zapisany w bazie (rotacja, czas życia, cleanup job).
- Potwierdzanie adresu e‑mail (link z tokenem).
- Reset hasła (link wysyłany mailem, ustawianie nowego hasła).
- Zarządzanie użytkownikiem:
  - Zmiana nazwy użytkownika/hasła.
  - Zarządzanie avatarem (upload/zmiana/usunięcie, pliki w `wwwroot`).

### Znajomi

- Wysyłanie zaproszeń do znajomych.
- Akceptowanie / odrzucanie zaproszeń.
- Lista znajomych.
- Wyszukiwanie użytkowników z filtrem typu "invitable" (kogo jeszcze można zaprosić).

### Czat / konwersacje

- Tworzenie i pobieranie konwersacji 1‑na‑1.
- Wysyłanie wiadomości.
- Paginacja wiadomości (od wskazanego `messageId` w dół).
- Śledzenie ostatniej wiadomości w konwersacji.
- Read receipts:
  - Per‑użytkownik informacja o ostatnio przeczytanej wiadomości.

### Real‑time (SignalR)

- Wysyłanie/odbieranie wiadomości w czasie rzeczywistym.
- Read receipts (powiadomienia o przeczytaniu).
- Zarządzanie wieloma połączeniami użytkownika (multi‑device).
- Powiadomienia o podłączeniu/rozłączeniu znajomych.

---

## Struktura solution

W katalogu głównym:

- `ChatCommunicator.sln` – główne solution.

Główne projekty:

- `API/`  
  - Kontrolery (Auth, Users, Chats, Friendships, Invitations).  
  - `ChatHub` i interfejsy SignalR.  
  - Konfiguracja DI (extension methods), Swagger, ProblemDetails.  
  - Statyczne pliki (`wwwroot` – m.in. avatary).

- `Application/`  
  - `Auth`, `Chats`, `FriendInvitations`, `Friendships`, `Users` – Commands/Queries.  
  - `Common` – behaviors, interfejsy, security, settings, mapping profile.  
  - `Repositories` – interfejsy repozytoriów używanych przez Application.

- `Domain/`  
  - `Entities` – encje domenowe (`User`, `Friendship`, `FriendshipInvitation`, `Conversation`, `Message`, `RefreshToken`, itd.).

- `Infrastructure/`  
  - `Database` – `ApplicationDbContext`, `UnitOfWork`.  
  - `Entities` – odwzorowania encji na bazę (jeśli oddzielone).  
  - `Migrations` – migracje EF Core.  
  - `Repositories` – implementacje repozytoriów.  
  - `Services` – serwisy infrastrukturalne (np. e‑mail, pliki, tokeny, połączenia użytkowników).  
  - `Authorization` – integracja z ASP.NET Core Authorization.  
  - `InfrastructureProfile` – mapowania infrastruktury.

- `Shared/`  
  - `Result/` – typy wyników (`Result<T>`, `Error` itp.).

Projekty testowe:

- `Application.UnitTests/`  
  - Testy handlerów, behaviors i logiki Application.

- `Infrastructure.UnitTests/`  
  - Testy serwisów i elementów warstwy Infrastructure (w tym z użyciem bazy / InMemory).

---

## Konfiguracja

Główna konfiguracja aplikacji znajduje się w `API/appsettings.json`. Przykład:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=<INSERT_HOST_HERE>;Database=<INSERT_DATABASE_HERE>;Port=<INSERT_PORT_HERE>;Username=<INSERT_USERNAME_HERE>;Password=<INSERT_PASSWORD_HERE>"
  },
  "JWTTokenSettings": {
    "Issuer": "<INSERT_ISSUER_HERE>",
    "Audience": "<INSERT_AUDIENCE_HERE>",
    "SigningKey": "<INSERT_SIGNING_KEY_HERE>",
    "AccessTokenLifeInSeconds": 900
  },
  "RefreshTokenSettings": {
    "RefreshTokenLifeInSeconds": 604800,
    "RefreshTokenCleanUpIntercalInSeconds": 604800
  },
  "CORS": {
    "AllowedOrigins": [
      "<INSERT_ALLOW_ORIGIN_HERE>"
    ]
  },
  "SmtpEmailSettings": {
    "SmtpHost": "<INSERT_SMTP_HOST_HERE>",
    "SmtpPort": "<INSERT_SMTP_PORT_HERE>",
    "Username": "<INSERT_SMTP_USERNAME_HERE>",
    "Password": "<INSERT_SMTP_PASSWORD_HERE>",
    "FromAddress": "<INSERT_FROM_ADDRESS_HERE>"
  },
  "ConfirmEmailMessageSettings": {
    "Content": "Click to confirm an email: [address]",
    "Address": "<INSERT_WEBSITE_ADDRESS_HERE>"
  },
  "RecoveryPasswordMessageSettings": {
    "Content": "PasswordRecovery Link: [address]",
    "Address": "<INSERT_WEBSITE_ADDRESS_HERE>"
  },
  "UserAvatarSettings": {
    "MaxFileSizeBytes": 5242880,
    "MaxImageWidth": 500,
    "MaxImageHeight": 500,
    "MinImageWidth": 100,
    "MinImageHeight": 100,
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".bmp", ".gif" ]
  },
  "Identity": {
    "Password": {
      "RequireDigit": false,
      "RequiredLength": 6,
      "RequireLowercase": false,
      "RequireUppercase": false,
      "RequireNonAlphanumeric": false
    },
    "Lockout": {
      "AllowedForNewUsers": true,
      "MaxFailedAccessAttempts": 5,
      "DefaultLockoutTimeSpan": 5
    },
    "User": {
      "RequireUniqueEmail": false
    }
  },
  "AllowedHosts": "*"
}
```

Najważniejsze wartości do ustawienia przed uruchomieniem:

- `ConnectionStrings.DefaultConnection` – parametry połączenia do PostgreSQL.  
- `JWTTokenSettings` – `Issuer`, `Audience`, `SigningKey`.  
- `CORS.AllowedOrigins` – adres(y) frontendu.  
- `SmtpEmailSettings` – jeśli chcesz testować e‑maile.

---

## Uruchomienie lokalne

Zakładam, że masz zainstalowane:

- .NET 9 SDK  
- PostgreSQL (lokalnie lub w Dockerze)

1. **Sklonuj repozytorium**

```bash
git clone https://github.com/<TWOJE_KONTO>/communicator.git
cd communicator
```

2. **Skonfiguruj `appsettings.Development.json` / `appsettings.json`**

- Uzupełnij połączenie do bazy i ustawienia JWT.
- Upewnij się, że `DefaultConnection` wskazuje na działającą bazę.

3. **Zastosuj migracje bazy**

```bash
cd Api
dotnet ef database update
```

4. **Uruchom API**

```bash
dotnet run --project API/API.csproj
```

Domyślnie API będzie dostępne pod adresem zdefiniowanym w `launchSettings.json` (np. `https://localhost:5001`).

---

## Testy

Projekt posiada testy jednostkowe dla warstw **Application** i **Infrastructure**.

Aby uruchomić wszystkie testy:

```bash
cd Api
dotnet test ChatCommunicator.sln
```

Aktualnie:

- 72 testy przechodzą (xUnit + FluentAssertions + FakeItEasy).

---

## API / Dokumentacja

Pełna dokumentacja API (snapshot) jest dostępna pod adresem:

**https://radoslawsmoronski.github.io/communicator/**

Zawiera:

- listę endpointów,  
- modele request/response,  
- wymagania autoryzacyjne (Bearer JWT),  
- kody odpowiedzi.

W środowisku Development dostępny jest również Swagger/OpenAPI hostowany przez API.

---

## SignalR i komunikacja w czasie rzeczywistym

Backend wystawia hub SignalR:

- Endpoint huba: `/ChatHub` (zwykle mapowany jako `/chathub`).

### Autoryzacja połączeń

Połączenia SignalR wymagają ważnego JWT. Token przekazywany jest jako query parameter:

```text
/ChatHub?access_token=your_jwt_token_here
```

### Metody na hubie (wywoływane przez klienta)

| Metoda      | Parametry                           | Opis                                                        |
| ----------- | ----------------------------------- | ----------------------------------------------------------- |
| `SendMessage` | `recipientId`, `conversationId`, `content` | Wysyła wiadomość do konkretnego odbiorcy w danej konwersacji |
| `ReadMessage` | `recipientId`, `conversationId`            | Oznacza wiadomości jako przeczytane do ostatniej w konwersacji |

### Callbacki dla klienta (wywoływane przez serwer)

| Metoda             | Parametry                                              | Opis                                                         |
| ------------------ | -------------------------------------------------------| ------------------------------------------------------------ |
| `ReceiveMessage`   | `messageId`, `conversationId`, `senderId`, `content`, `timestamp` | Informacja o nowej wiadomości                               |
| `MessageRead`      | `messageId`, `conversationId`                          | Informacja, że odbiorca przeczytał wiadomość                |
| `FriendConnect`    | `friendId`                                            | Powiadomienie, że znajomy się podłączył                     |
| `FriendDisconnect` | `friendId`                                            | Powiadomienie, że znajomy się rozłączył                     |

### Zarządzanie połączeniami

- Jeden użytkownik może mieć wiele aktywnych połączeń (multi‑device).
- Aplikacja śledzi aktywne connectionId użytkowników.
- Wiadomości i powiadomienia są wysyłane do wszystkich aktywnych połączeń danego użytkownika.

---

## Logowanie

- Serilog zapisuje logi do konsoli oraz do tabeli `Logs` w PostgreSQL (tworzona automatycznie).
- Konfiguracja sinków znajduje się w rozszerzeniach DI w projekcie `API`.
- Dodatkowy background service czyści wygasłe refresh tokeny.

---

## Status projektu

Projekt jest **ukończony w wersji Beta**:

- architektura warstwowa,  
- obsługa real‑time, auth, friends, reset hasła, avatary,  
- testy jednostkowe po stronie Application i Infrastructure,  
- pełne uruchomienie z migracjami i dokumentacją API.

Kolejne możliwe kroki (opcjonalne):

- konteneryzacja (Docker + docker‑compose z PostgreSQL),  
- dodatkowe testy integracyjne API,
- kolejne funkcjonalności
