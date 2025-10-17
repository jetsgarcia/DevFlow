# DevFlow - Coding Session Tracker API

DevFlow is a minimal ASP.NET Core Web API for tracking coding sessions across multiple projects. It helps developers monitor their time spent on different projects, providing insights into their productivity and work patterns.

## Features

- 📊 **Project Management**: Create, update, and delete projects
- ⏱️ **Session Tracking**: Start and stop coding sessions for projects
- 📈 **Statistics & Analytics**: Track session durations, averages, and trends
- 🔒 **Single Active Session**: Ensures focus by allowing only one active session at a time
- 💾 **SQLite Database**: Lightweight, file-based database for easy setup
- 📝 **Swagger/OpenAPI**: Interactive API documentation

## Technology Stack

- **Framework**: .NET 8.0
- **Database**: SQLite with Entity Framework Core 9.0
- **API Documentation**: Swagger/Swashbuckle
- **Architecture**: Minimal APIs with service layer pattern

## Prerequisites

Before setting up DevFlow, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A code editor (recommended: [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/))
- Git (optional, for cloning the repository)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/jetsgarcia/DevFlow.git
cd DevFlow
```

### 2. Restore Dependencies

Navigate to the project directory and restore NuGet packages:

```bash
cd DevFlow
dotnet restore
```

### 3. Database Setup

The project uses SQLite with Entity Framework Core migrations. The database will be created automatically on first run.

#### Apply Migrations

```bash
dotnet ef database update
```

This creates a `devflow.db` file in the project directory with the required schema.

#### (Optional) Create New Migration

If you modify the data models, create a new migration:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### 4. Run the Application

Start the development server:

```bash
dotnet run
```

Or with hot reload:

```bash
dotnet watch run
```

The API will be available at:

- **HTTP**: http://localhost:5289

## Project Structure

```
DevFlow/
├── Data/
│   └── ApplicationDbContext.cs      # EF Core database context
├── Endpoints/
│   ├── ProjectEndpoints.cs          # Project API endpoints
│   └── SessionEndpoints.cs          # Session API endpoints
├── Migrations/                      # EF Core migrations
├── Models/
│   ├── Project.cs                   # Project entity
│   ├── Session.cs                   # Session entity
│   ├── ApiResponse.cs               # API response wrapper
│   └── DTOs/                        # Data Transfer Objects
│       ├── ProjectDto.cs
│       ├── SessionDto.cs
│       └── AnalyticsDto.cs
├── Services/
│   ├── IProjectService.cs           # Project service interface
│   ├── ProjectService.cs            # Project business logic
│   ├── ISessionService.cs           # Session service interface
│   └── SessionService.cs            # Session business logic
├── Utilities/
│   └── DateTimeHelper.cs            # Timezone utilities
├── appsettings.json                 # Application configuration
├── appsettings.Development.json     # Development configuration
├── Program.cs                       # Application entry point
└── DevFlow.csproj                   # Project file
```

## Configuration

The application configuration is stored in `appsettings.json` and `appsettings.Development.json`.

### Database Connection String

The default SQLite connection string is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=devflow.db"
  }
}
```

To use a different database location, modify the connection string in your configuration file.

### Timezone Configuration

The application uses Philippine Time (UTC+8) by default. This is configured in `DateTimeHelper.cs`. To change the timezone:

```csharp
// DevFlow/Utilities/DateTimeHelper.cs
private static readonly TimeZoneInfo PhilippineTimeZone =
    TimeZoneInfo.FindSystemTimeZoneById("Your-TimeZone-Id");
```

## API Endpoints

### Projects

- `GET /api/projects` - Get all projects
- `POST /api/projects` - Create a new project
- `PUT /api/projects/{id}` - Update a project
- `DELETE /api/projects/{id}` - Delete a project

### Sessions

- `POST /api/sessions/start` - Start a new session
- `POST /api/sessions/end` - End an active session
- `GET /api/sessions/project/{projectId}` - Get all sessions for a project
- `GET /api/sessions/active/{projectId}` - Check for active session on a project
- `GET /api/sessions/active` - Get any active session
- `GET /api/sessions/average-duration` - Get average session duration
- `GET /api/sessions/statistics` - Get comprehensive statistics
- `GET /api/sessions/statistics/project/{projectId}` - Get project-specific statistics

For detailed API documentation, run the application and visit the Swagger UI at http://localhost:5289/swagger/index.html

## Development

### Adding a New Migration

When you modify entity models, create a new migration:

```bash
dotnet ef migrations add <MigrationName> --project DevFlow
dotnet ef database update --project DevFlow
```

### Building for Production

Create a production build:

```bash
dotnet build --configuration Release
```

### Running Tests

(Note: Add this section when tests are implemented)

```bash
dotnet test
```

## Troubleshooting

### Database Issues

If you encounter database errors:

1. Delete the `devflow.db`, `devflow.db-shm`, and `devflow.db-wal` files
2. Run `dotnet ef database update` again

### Port Conflicts

If the default ports are in use, modify `launchSettings.json`:

```json
{
  "applicationUrl": "https://localhost:YOUR_HTTPS_PORT;http://localhost:YOUR_HTTP_PORT"
}
```

### Missing Dependencies

If packages are missing:

```bash
dotnet restore --force
```

## Support

For issues, questions, or contributions, please open an issue on GitHub.
