# Chat Communicator - API

A real-time chat backend built on .NET 9 and C# 13 using Clean Architecture, SignalR, ASP.NET Core Identity, and EF Core (PostgreSQL).

---

## Overview

Chat Communicator enables users to register, authenticate, manage friendships, and exchange messages in real-time. The solution is split into layers:

- API: HTTP endpoints, SignalR `ChatHub`, OpenAPI/Swagger, exception handling.
- Application: CQRS with MediatR, authorization pipeline, mapping.
- Infrastructure: EF Core + PostgreSQL, ASP.NET Core Identity, email/avatars/files, Serilog, background jobs.
- Domain: Core entities and models.
- Shared: Result/Errors and common primitives.

## Tech Stack

- .NET 9, C# 13
- ASP.NET Core, MediatR
- SignalR (real-time)
- EF Core + Npgsql (PostgreSQL)
- ASP.NET Core Identity (JWT auth)
- Serilog (console + PostgreSQL sink)
- AutoMapper
- Swagger/OpenAPI (dev), optional ReDoc bundle
- xUnit  
- FluentAssertions
- FakeItEasy

## Features

- Authentication
  - Email/password login
  - JWT access tokens, DB-backed refresh tokens (rotation + cleanup job)
  - Email confirmation
  - Password reset (SMTP)
- Users
  - Registration
  - Change username/password
  - Avatar upload/change/delete (stored under `wwwroot`)
- Friendships
  - Invitations (send/accept/decline)
  - Friends list, searchable users (with "invitable" filter)
- Chat
  - Conversation create/get
  - Send messages, pagination from message id
  - Read receipts (per-user last read)
  - Last message tracking
  - Presence and multi-device connections

## Solution Structure

- `API`: Controllers, `ChatHub`, DI, OpenAPI/Swagger, ProblemDetails
- `Application`: Commands/Queries (MediatR), pipeline behaviors (authorization), settings, mappings
- `Infrastructure`: `ApplicationDbContext`, Identity, repositories/UoW, services (email, files, messages, conversations, friendships), Serilog, background services
- `Domain`: Entities
- `Shared`: `Result<T>`, errors, helpers

## Testing

This solution uses xUnit with `FluentAssertions` and `FakeItEasy`. Tests are split by layer:

- `Application.UnitTests` — unit tests for CQRS handlers, pipeline behaviors and application logic (example: `AuthorizationBehaviorTests`).
- `Infrastructure.Tests` — infrastructure-focused tests (example: `TokenServiceTests`); a mix of focused unit tests and small integration tests touching persistence/cleanup.

## Configuration

Configure your application settings in appsettings.json:

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

#### Running the Application

1.  Clone the repository
2.  Apply migrations to your database:
    `dotnet ef database update`
3.  Run the application:
    `dotnet run --project ChatCommunicator.API`

## API Documentation

You can access the full API documentation here:  
🔗 **[https://radoslawsmoronski.github.io/communicator/](https://radoslawsmoronski.github.io/communicator/)**

The documentation provides details on endpoints, models, responses, and authentication.

## SignalR Integration

ChatCommunicator leverages SignalR for real-time communication between clients. This enables instant message delivery, typing indicators, and read receipts.

##### Chat Hub

The application exposes a ChatHub that handles real-time messaging:
`/ChatHub`

##### Authentication

All SignalR connections require authentication. Clients must provide a valid JWT token as a query parameter:

`/ChatHub?access_token=your_jwt_token_here`

#### Available Methods

| Method      | Parameters                           | Description                                                     |
| ----------- | ------------------------------------ | --------------------------------------------------------------- |
| SendMessage | recipientId, conversationId, content | Sends a message to a specific recipient in a conversation       |
| ReadMessage | recipientId, conversationId          | Marks messages as read up to the last message in a conversation |

#### Client Callbacks

The server invokes these methods on connected clients:

| Method           | Parameters                                              | Description                                                    |
| ---------------- | ------------------------------------------------------- | -------------------------------------------------------------- |
| ReceiveMessage   | messageId, conversationId, senderId, content, timestamp | Notifies clients when a new message is received                |
| MessageRead      | messageId, conversationId                               | Notifies clients when a message has been read by the recipient |
| FriendConnect    | friendId                                                | Notifies clients when friend has been connected                |
| FriendDisconnect | friendId                                                | Notifies clients when friend has been disconnected             |

#### Connection Management

The application tracks user connections using the UsersConnectionService:

- Users can be connected from multiple devices simultaneously
- When a user disconnects, their connection is automatically removed
- Messages are delivered to all active connections of the recipient

## Logging

- Serilog writes to console and PostgreSQL table `Logs` (auto-created).
- Adjust sink settings in `API/Extensions/DependencyInjection.cs`.
- A background service periodically removes expired refresh tokens (not logs).

This project uses xUnit with FluentAssertions and FakeItEasy. Tests are split by layer:

- `Application.UnitTests` — unit tests for CQRS handlers, behaviors, and application logic.
- `Infrastructure.Tests` — tests for infrastructure services (example: `TokenService`), can be integration or focused unit tests that touch DB/IO.

## Notes

- Admin role bypass: requests marked with authorization interfaces are permitted when the caller has the `Admin` role.
- Static files: avatars are stored under `wwwroot/<UserAvatarSettings:Folder>`.
- HTTPS redirection is enabled by default.

## Project Status

Active development. Next steps:
- Containerization (Docker)
- Performance and UX improvements
- Further API hardening and tooling

Contributions and feedback are welcome.
