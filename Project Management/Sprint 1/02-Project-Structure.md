# Sprint 1 – Project Structure

## Solution Layout

The solution was organized into five projects from the outset to enforce separation of concerns and enable parallel development across team members.

```
Inventory-Management-System/
├── coderush/                        # Main web application
│   ├── Controllers/                 # MVC + API controllers
│   │   └── Api/                     # RESTful API endpoints
│   ├── Data/                        # EF Core DbContext & seeding
│   ├── Extensions/                  # Helper extension methods
│   ├── Filters/                     # Action filters (e.g., notifications)
│   ├── Helpers/                     # HTML helpers
│   ├── Migrations/                  # EF Core database migrations
│   ├── Models/                      # Domain & view models
│   │   ├── AccountViewModels/       # Login, Register, 2FA models
│   │   ├── ManageViewModels/        # Profile management models
│   │   └── SyncfusionViewModels/    # Grid CRUD view models
│   ├── Pages/                       # Razor Pages (MainMenu)
│   ├── Services/                    # Business logic services
│   ├── Views/                       # Razor views per feature
│   └── wwwroot/                     # Static assets (CSS, JS, images)
├── coderush.UnitTests/              # Unit test project
├── coderush.IntegrationTests/       # Integration test project
├── coderush.E2ETests/               # End-to-end Selenium tests
└── coderush.AutomatedTests/         # Automated UI test project
```

## Rationale

- **Single web project** keeps deployment simple while the `Controllers/Api/` sub-folder cleanly separates API endpoints from MVC page controllers.
- **Four dedicated test projects** allow each testing tier (unit, integration, E2E, automated) to run independently with its own dependencies and execution strategy.
- **`InternalsVisibleTo`** attributes on the main project expose internal members to `coderush.UnitTests` and `coderush.IntegrationTests` for thorough testing without making internals public.

## Technology Stack Established

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET | 10.0 |
| Web Framework | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core | 10.0.5 |
| Identity | ASP.NET Core Identity | 10.0.5 |
| Database | SQL Server / LocalDB | Latest |
| Email | SendGrid | 9.29.3 |
| Spreadsheets | ClosedXML / DocumentFormat.OpenXml | 0.105.0 / 3.5.1 |
| Cloud Identity | Azure.Identity | 1.20.0 |