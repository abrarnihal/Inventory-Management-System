## Get Started on compiling, running and using our program:

- Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Getting the Source

```bash
git clone <repository-url>
cd Inventory-Management-System-main
```

## Configuration

### Database

The application uses two connection strings defined in configuration:

| Key | Location | Purpose |
|---|---|---|
| `DefaultConnection` | `coderush/appsettings.json` | Primary (Azure SQL) database |
| `LocalConnection` | `coderush/appsettings.Development.json` | LocalDB fallback for offline development |

When running in **Development** mode the app automatically tries `DefaultConnection` first. If that server is unreachable it falls back to `LocalConnection`, so you can work offline with LocalDB without changing any settings.

To use **only** LocalDB, no changes are required — just run in Development mode and the fallback will activate if the Azure SQL server is unavailable.

### OpenAI Chat Bot (optional)

The app includes an AI-powered chat bot. To enable it, supply an OpenAI API key via a `.env` file in the solution root:

1. Copy the example file:
   ```bash
   copy .env.example .env
   ```
2. Edit `.env` and set your key:
   ```
   OpenAI__ApiKey=sk-proj-YOUR_KEY_HERE
   OpenAI__Model=gpt-5.1
   ```

If no key is configured the rest of the application still works; only the chat feature will be unavailable.

### Email (optional)

Email sending is supported through **SMTP**. Configure the relevant section in `coderush/appsettings.json`:

- **SMTP** — fill in `SmtpOptions` with your SMTP host credentials (Gmail example is pre-configured).

Set `"IsDefault": true` on whichever provider you want to use.

## Building

### Visual Studio

1. Open `coderush.sln` in Visual Studio.
2. Build the solution: **Build > Build Solution** (`Ctrl+Shift+B`).

### Command Line

```bash
dotnet build coderush.sln
```

## Running the Application

### Visual Studio

1. Set **coderush** as the startup project (right-click the project > **Set as Startup Project**).
2. Press **F5** (Debug) or **Ctrl+F5** (Run without debugging).
3. The browser will open automatically. By default the app runs at:
   - `https://localhost:44308` (IIS Express)
   - `http://localhost:55013` (Kestrel)

### Command Line

```bash
cd coderush
dotnet run
```

Then open `http://localhost:55013` in your browser.

## Database Initialization

On first run the application will:

1. Create the database if it does not exist (`EnsureCreatedAsync`).
2. Run idempotent schema patches for any columns/tables added after the initial migration.
3. Seed a **Super Admin** account and default reference data.

No manual migration commands are needed for a fresh start.

### Applying Migrations Manually

If you need to update an existing database after pulling new migration files:

```bash
cd coderush
dotnet ef database update
```

## Default Login

After the database is seeded the following super admin account is available:

| Field | Value |
|---|---|
| Email | `super@admin.com` |
| Password | `123456` |

> **Warning: Change these credentials immediately in a production environment.** Update the values in the `SuperAdminDefaultOptions` section of `appsettings.json`.

## Using the Application

### Logging In

Navigate to the application URL. You will be redirected to the **Login** page. Sign in with the super admin credentials above (or any account created through the User management pages).

### Main Modules

After logging in you land on your **User Profile** page. From the navigation menu you can access:

| Module | Description |
|---|---|
| **Dashboard** | Overview of key metrics |
| **Customers** | Manage customers and customer types |
| **Vendors** | Manage vendors and vendor types |
| **Products** | Manage products, product types, and units of measure |
| **Purchase Orders** | Create and track purchase orders and purchase order lines |
| **Goods Received Notes** | Record goods received against purchase orders |
| **Bills** | Manage vendor bills and bill types |
| **Payment Vouchers** | Record payments made to vendors |
| **Sales Orders** | Create and track sales orders and sales order lines |
| **Shipments** | Manage shipments and shipment types |
| **Invoices** | Generate customer invoices and manage invoice types |
| **Payment Receive** | Record payments received from customers |

### Settings / Reference Data

The following reference data pages are available under the settings area:

- **Branch** — company branches/locations
- **Cash Bank** — cash and bank accounts
- **Currency** — supported currencies
- **Warehouse** — storage locations
- **Payment Type** — payment method definitions

### User and Role Management

- **User** — create and manage user accounts
- **Role** — define roles
- **Change Role** — assign roles to users (controls which menu items each user can see)
- **Change Password** — update a user password

## Running Tests

The solution includes multiple test projects:

| Project | Type |
|---|---|
| `coderush.UnitTests` | Unit tests |
| `coderush.IntegrationTests` | Integration tests |
| `coderush.E2ETests` | End-to-end tests |
| `coderush.AutomatedTests` | Automated tests |

### Visual Studio

Open **Test Explorer** (`Ctrl+E, T`) and click **Run All**.

### Command Line

```bash
dotnet test coderush.sln
```

## Project Structure

```
coderush.sln
+-- coderush/                    # Main web application
|   +-- Controllers/             # MVC controllers
|   +-- Controllers/Api/         # API controllers
|   +-- Data/                    # DbContext and database initializer
|   +-- Models/                  # Entity and view models
|   +-- Pages/                   # Menu and page definitions
|   +-- Services/                # Business logic and external integrations
|   +-- Views/                   # Razor views
|   +-- Migrations/              # EF Core migrations
|   +-- Program.cs               # Application entry point
|   +-- appsettings.json         # Configuration
+-- coderush.UnitTests/          # Unit tests
+-- coderush.IntegrationTests/   # Integration tests
+-- coderush.E2ETests/           # End-to-end tests
+-- coderush.AutomatedTests/     # Automated tests
+-- .env.example                 # Environment variable template
```

## License

See the repository root for license information.