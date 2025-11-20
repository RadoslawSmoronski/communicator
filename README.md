🌐 [English](README.md) | 🇵🇱 [Polski](README-pl.md)
# Chat Communicator – Backend (.NET 9)

Real-time backend for a chat application (1-to-1 conversations) with user registration, JWT authentication, friends, invitations and live messages. 
Built with .NET 9, C# 13 and a Clean Architecture style.

---

## Table of contents

1. [Project overview](#project-overview)  
2. [Technologies](#technologies)  
3. [Architecture](#architecture)  
4. [Features](#features)  
5. [Solution structure](#solution-structure)  
6. [Configuration](#configuration)  
7. [How to run locally](#how-to-run-locally)  
8. [Tests](#tests)  
9. [API / Documentation](#api--documentation)  
10. [SignalR and real-time communication](#signalr-and-real-time-communication)  
11. [Logging](#logging)  
12. [Project status](#project-status)

---

## Project overview

**Chat Communicator** is a backend for a chat application that allows users to:

- create an account, confirm their email, log in and refresh tokens,
- send and accept friend invitations,
- chat in real time using SignalR,
- manage their profile (for example user avatar).

The project was created to learn and show good backend practices:

- clear layers (API, Application, Domain, Infrastructure, Shared),
- CQRS with MediatR,
- ASP.NET Core Identity,
- EF Core with migrations,
- separate unit tests for Application and Infrastructure.

---

## Technologies

- .NET 9, C# 13  
- ASP.NET Core Web API  
- MediatR (CQRS: Commands/Queries)  
- SignalR (real-time communication)  
- EF Core + Npgsql (PostgreSQL)  
- ASP.NET Core Identity (JWT auth)  
- Serilog (console + PostgreSQL sink)  
- AutoMapper  
- Swagger / OpenAPI (in Development)  
- xUnit  
- FluentAssertions  
- FakeItEasy

---

## Architecture

The project follows a style close to **Clean Architecture**:

- **API**  
  - Controllers, `ChatHub` for SignalR.  
  - Handles HTTP, routing, input validation, mapping to commands/queries.  
  - Configures Swagger/OpenAPI, exception handling (ProblemDetails), CORS and authentication.

- **Application**  
  - Application logic as **command/query handlers** (MediatR).  
  - Pipeline behaviors (for example authorization).  
  - Service interfaces (ports), mapping profiles, configuration (typed settings).

- **Domain**  
  - Domain entities (`User`, `Friendship`, `FriendshipInvitation`, `Conversation`, `Message`, `RefreshToken`, etc.).  
  - Domain rules and relationships between entities.

- **Infrastructure**  
  - `ApplicationDbContext` (EF Core + Identity).  
  - Implementations of repositories and services (email, files, messages, friends, tokens).  
  - Database migrations.  
  - Serilog integration (logs to console and PostgreSQL).

- **Shared**  
  - `Result<T>` and error types.  
  - Common primitives shared between layers.

---

## Features

### Authentication and users

- User registration (email + password).
- Login:
  - JWT access token.
  - Refresh token stored in the database (rotation, lifetime, cleanup job).
- Email confirmation (confirmation link with token).
- Password reset (link sent by email, set new password).
- User management:
  - Change username / password.
  - Manage avatar (upload / change / delete, files in `wwwroot`).

### Friends

- Send friend invitations.
- Accept / reject invitations.
- Friends list.
- Search users with an "invitable" filter (who can still be invited).

### Chat / conversations

- Create and get 1-to-1 conversations.
- Send messages.
- Paginate messages (from a given `messageId` backwards).
- Track the last message in a conversation.
- Read receipts:
  - For each user, track the last read message.

### Real-time (SignalR)

- Send and receive messages in real time.
- Read receipt notifications.
- Support for multiple connections per user (multi-device).
- Notifications when a friend connects or disconnects.

---

## Solution structure

In the root folder:

- `ChatCommunicator.sln` – main solution file.

Main projects:

- `API/`  
  - Controllers (Auth, Users, Chats, Friendships, Invitations).  
  - `ChatHub` and SignalR interfaces.  
  - DI configuration (extension methods), Swagger, ProblemDetails.  
  - Static files (`wwwroot` – for example avatars).

- `Application/`  
  - `Auth`, `Chats`, `FriendInvitations`, `Friendships`, `Users` – commands/queries.  
  - `Common` – behaviors, interfaces, security, settings, mapping profile.  
  - `Repositories` – repository interfaces used by Application.

- `Domain/`  
  - `Entities` – domain entities (`User`, `Friendship`, `FriendshipInvitation`, `Conversation`, `Message`, `RefreshToken`, etc.).

- `Infrastructure/`  
  - `Database` – `ApplicationDbContext`, `UnitOfWork`.  
  - `Entities` – database mapping entities (if separated).  
  - `Migrations` – EF Core migrations.  
  - `Repositories` – repository implementations.  
  - `Services` – infrastructure services (for example email, files, tokens, user connections).  
  - `Authorization` – integration with ASP.NET Core Authorization.  
  - `InfrastructureProfile` – mapping profiles for infrastructure.

- `Shared/`  
  - `Result/` – result types (`Result<T>`, `Error`, etc.).

Test projects:

- `Application.UnitTests/`  
  - Tests for handlers, behaviors and Application logic.

- `Infrastructure.UnitTests/`  
  - Tests for services and parts of the Infrastructure layer (including database / InMemory).

---

## Configuration

Main application configuration is in `API/appsettings.json`. Example:

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

Before running the app, set:

- `ConnectionStrings.DefaultConnection` – PostgreSQL connection string.  
- `JWTTokenSettings` – `Issuer`, `Audience`, `SigningKey`.  
- `CORS.AllowedOrigins` – frontend origin(s).  
- `SmtpEmailSettings` – if you want to test emails.

---

## How to run locally

You need:

- .NET 9 SDK  
- PostgreSQL (locally or in Docker)

1. **Clone the repository**

```bash
git clone https://github.com/<YOUR_ACCOUNT>/communicator.git
cd communicator
```

2. **Configure `appsettings.Development.json` / `appsettings.json`**

- Set database connection and JWT settings.
- Make sure `DefaultConnection` points to a working database.

3. **Apply database migrations**

```bash
cd Api
dotnet ef database update
```

4. **Run the API**

```bash
dotnet run --project API/API.csproj
```

By default, the API will be available at the URL from `launchSettings.json` (for example `https://localhost:5001`).

---

## Tests

The project has unit tests for **Application** and **Infrastructure** layers.

To run all tests:

```bash
cd Api
dotnet test ChatCommunicator.sln
```

Current status:

- 72 tests passing (xUnit + FluentAssertions + FakeItEasy).

---

## API / Documentation

A snapshot of the API documentation is available here:

**https://radoslawsmoronski.github.io/communicator/**

It contains:

- list of endpoints,  
- request/response models,  
- auth requirements (Bearer JWT),  
- response codes.

In the Development environment, Swagger/OpenAPI is also exposed directly by the API.

---

## SignalR and real-time communication

The backend exposes a SignalR hub:

- Hub endpoint: `/ChatHub` (usually mapped as `/chathub`).

### Connection authorization

SignalR connections require a valid JWT. The token is passed as a query parameter:

```text
/ChatHub?access_token=your_jwt_token_here
```

### Hub methods (called by clients)

| Method       | Parameters                                 | Description                                                  |
|-------------|---------------------------------------------|--------------------------------------------------------------|
| `SendMessage` | `recipientId`, `conversationId`, `content`  | Sends a message to a specific recipient in a conversation    |
| `ReadMessage` | `recipientId`, `conversationId`             | Marks messages as read up to the last message in the conversation |

### Client callbacks (called by server)

| Method            | Parameters                                                          | Description                                      |
|------------------|---------------------------------------------------------------------|--------------------------------------------------|
| `ReceiveMessage` | `messageId`, `conversationId`, `senderId`, `content`, `timestamp`   | Notifies about a new message                     |
| `MessageRead`    | `messageId`, `conversationId`                                       | Notifies that the recipient read the message     |
| `FriendConnect`  | `friendId`                                                          | Notifies that a friend connected                 |
| `FriendDisconnect` | `friendId`                                                        | Notifies that a friend disconnected              |

### Connection management

- One user can have many active connections (multi-device).
- The app tracks active connectionIds for each user.
- Messages and notifications are sent to all active connections of the user.

---

## Logging

- Serilog writes logs to the console and to a `Logs` table in PostgreSQL (created automatically).
- Sink configuration is in DI extensions in the `API` project.
- There is also a background service that removes expired refresh tokens.

---

## Project status

The project is **in Beta**:

- layered architecture,  
- real-time chat, auth, friends, password reset, avatars,  
- unit tests for Application and Infrastructure,  
- full local run with migrations and API documentation.

Possible next steps (optional):

- containerization (Docker + docker-compose with PostgreSQL),  
- additional API integration tests,
- more features.
