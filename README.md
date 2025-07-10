# 💬 Chat Communicator - API

**Chat Communicator** is a simple chat application built with .NET 8. It provides user registration, login, friend management, and real-time messaging using SignalR.

---

## 🧰 Technologies

- ASP.NET Core Web API (.NET 8)
- SignalR
- Entity Framework Core
- Identity
- JWT (authentication)
- PostgreSQL
- AutoMapper
- Serilog
- xUnit, FluentAssertions, FakeItEasy (unit testing)
- Redoc (API documentation)

---

## 📄 REST API Documentation

You can access the full API documentation here:  
🔗 **[https://radoslawsmoronski.github.io/communicator/](https://radoslawsmoronski.github.io/communicator/)**

The documentation provides details on endpoints, models, responses, and authentication.

---

## 📡 SignalR – `ChatHub`

Real-time messaging is handled through a SignalR hub called `ChatHub`.

### Method available for clients

#### `SendMessage(Guid recipientId, Guid conversationId, string content)`

Sends a message to another user in the context of a specific conversation.

- `recipientId`: The target user's ID
- `conversationId`: The ID of the conversation
- `content`: The message content (text)

### 🖥️ Frontend nad branches

The frontend is developed using **React** and maintained in a separate `client` branch.

- `api/development` — backend (API) development branch, contains the latest backend changes.
- `client` — frontend development branch, containing the React app.
- `development` — integration branch where the backend (`api/development`) and frontend (`client`) branches are merged.

**Note:** The `development` branch may contain an older version of the backend API compared to `api/development`, since it integrates both backend and frontend changes.  
Similarly, the frontend in `client` may be ahead of what is currently merged into `development`.

## ⚙️ Configuration and Running Locally

### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- PostgreSQL

### Example `appsettings.json`

<<<<<<< HEAD
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
=======
    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=string;Database=string;Port=string;Username=string;Password=string"
      },
      "JWT": {
        "Issuer": "string",
        "Audience": "string",
        "SigningKey": "your-signing-key-here"
      },
      "AllowedHosts": "*"
>>>>>>> api/feature/development
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

## 💬 Logging with Serilog

The project uses Serilog for structured logging. Logs are saved to the console and to a PostgreSQL database (table Logs).

Additionally, an automatic cleanup service runs periodically to delete log entries older than 30 days.

The table is created automatically if it does not exist.

## 🧪 Testing

Unit tests are located in the `ChatCommunicator.Tests` project.

They cover key components such as:

- **UserController** – user registration, login, profile management
- **ChatManager** – messaging, conversations, chat logic
- **FriendsManager** – friend requests and management
- **TokenManager** – JWT token generation and validation

### Testing tools used:

- **xUnit** – testing framework
- **FakeItEasy** – mocking dependencies
- **FluentAssertions** – expressive assertions

### Running tests

Run all tests using the command:

`dotnet test`

## 🚧 Project Status & Future Work

This project is currently **under active development** and not yet complete. There are still several important features and improvements planned, including:

- Removing friends functionality
- Avatar editing for user profiles
- Editing user account details
- Potential Docker integration for easier deployment and environment management
- Additional enhancements to improve performance and user experience

We welcome feedback, ideas, and contributions to help make **Chat Communicator** even better!

Stay tuned for updates as the project evolves. 🚀
