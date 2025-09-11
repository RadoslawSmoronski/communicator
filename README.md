# Chat Communicator - API

A real-time chat application built with .NET 9 and C# 13.0, providing secure and efficient communication between users.

---

## Overview

ChatCommunicator is a full-featured chat platform that enables users to connect, manage friendships, and exchange messages in real-time. The application is built using a clean architecture approach with distinct layers for API, application logic, and infrastructure.

## Technologies

- .NET 9
- C# 13.0
- ASP.NET Core
- SignalR for real-time communication
- Entity Framework Core with PostgreSQL
- JWT Authentication
- Serilog for structured logging
- AutoMapper for object mapping
- Swagger/ReDoc for API
- xUnit, FluentAssertions, FakeItEasy (unit testing)

## Features

#### Authentication

- User registration and login
- JWT token-based authentication
- Refresh token mechanism for extended sessions

#### User Management

- Profile management
- Username customization
- Password changes
- Avatar upload, change, and deletion

#### Friendship

- Send, accept, and decline friend invitations
- View pending invitations
- Search for users to add as friends
- View friend list
- Messaging
- Real-time chat between friends
- Message history with pagination
- Read receipts for messages
- Last message tracking for conversations

## Project Structure

- ChatCommunicator.API: API endpoints and controllers
- ChatCommunicator.Application: Business logic and services
- ChatCommunicator.Infrastructure: Data access, persistence, and external services
- ChatCommunicator.Contracts: DTOs and shared models
- ChatCommunicator.Shared: Utilities and helper classes

## Configuration and Running Locally

#### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- PostgreSQL

#### Configuration

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
  "JWT": {
    "Issuer": "<INSERT_ISSUER_HERE>",
    "Audience": "<INSERT_AUDIENCE_HERE>",
    "SigningKey": "<INSERT_SIGNING_KEY_HERE>"
  },
  "CORS": {
    "AllowedOrigins": ["<INSERT_ALLOW_ORIGIN_HERE>"]
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

## Logging with Serilog

The project uses Serilog for structured logging. Logs are saved to the console and to a PostgreSQL database (table Logs).

Additionally, an automatic cleanup service runs periodically to delete log entries older than 30 days.

The table is created automatically if it does not exist.

## Testing

Unit tests are located in the `ChatCommunicator.Tests` project.

#### Testing tools used:

- **xUnit** – testing framework
- **FakeItEasy** – mocking dependencies
- **FluentAssertions** – expressive assertions

#### Running tests

Run all tests using the command:

`dotnet test`

## Project Status & Future Work

This project is currently **under active development** and not yet complete. There are still several important features and improvements planned, including:

- Potential Docker integration for easier deployment and environment management
- Additional enhancements to improve performance and user experience
- General refactor to clean architecture

We welcome feedback, ideas, and contributions to help make **Chat Communicator** even better!

Stay tuned for updates as the project evolves. 🚀
