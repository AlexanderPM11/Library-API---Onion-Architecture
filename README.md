# LibraryAPI Backend 🚀

A robust, enterprise-grade RESTful API built with **ASP.NET Core 8**. This secure backend serves as the foundation for the LibraryApp system, handling book cataloging, author management, dynamic categorization, and identity provision.

## 🏗️ Architecture Stack

- **Framework:** ASP.NET Core 8 Web API
- **Language:** C#
- **Architecture Pattern:** Clean Architecture (Domain-Driven Design inspired)
  - `LibraryAPI.Domain`: Core entities and exceptions.
  - `LibraryAPI.Application`: CQRS/Services, DTOs, Mapping profiles.
  - `LibraryAPI.Infrastructure`: Data access implementations, DbContext.
  - `LibraryAPI.Api`: Controllers, Middleware, API configurations.
- **ORM:** Entity Framework Core (MySQL Provider)
- **Authentication:** JWT (JSON Web Tokens) with ASP.NET Core Identity
- **Validation:** FluentValidation
- **Object Mapping:** AutoMapper
- **Documentation:** Swagger (OpenAPI)

---

## 🔒 Security & Roles

The API routes are inherently protected using dynamic Role-Based Access Control (RBAC).
- `[AllowAnonymous]` - Read-only endpoints available to all (e.g., retrieving lists of books).
- `[Authorize]` - Standard endpoints requiring a valid JWT token.
- `[Authorize(Roles = "Admin")]` - High-privilege routes for creating, updating, and deleting records.

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)

### 1. Database Configuration

Navigate to the API project directory (`src/Presentation/LibraryAPI.Api`) and ensure your connection string is properly set in `appsettings.json` or `appsettings.Development.json` targeting your local MySQL server.
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=libraryapp_db;User=root;Password=YOUR_PASSWORD;"
}
```

### 2. Entity Framework Migrations & Seeding

The application implements an automatic Database Initializer. Upon the first successful run, if the database doesn't exist or is not up to date, it will:
1. Apply any pending Entity Framework Entity mapping changes.
2. Seed the fundamental Identity Roles (`Admin`, `User`).
3. Seed the default Administrator user account.

**Default Administrator Account:**
- **Email:** `admin@library.com`
- **Password:** `Admin123!`

### 3. Running the API

You can start the project via the .NET CLI from the solution root:

```bash
dotnet restore
dotnet run --project src/Presentation/LibraryAPI.Api/LibraryAPI.Api.csproj
```

The application will typically map to ports `https://localhost:7103` or `https://localhost:44381` (validate via terminal output or `launchSettings.json`).

### 4. Interactive Documentation (Swagger)

A completely interactive API visual explorer is baked directly into the application. Once running, navigate to:
```url
https://localhost:<PORT>/swagger
```
*Note: Make sure to log in, retrieve your token, and click the "Authorize" button in Swagger inserting `Bearer <YOUR_TOKEN>` to successfully execute locked requests directly from the UI.*
