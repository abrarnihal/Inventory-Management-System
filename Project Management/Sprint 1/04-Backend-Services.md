# Sprint 1 – Backend Services & API Layer

## Service Layer

All business logic is encapsulated behind interfaces to facilitate dependency injection and testability.

### Core Service Interfaces & Implementations

| Interface | Implementation | Responsibility |
|---|---|---|
| `IFunctional` | `Functional` | Database initialization, seed data, and utility operations |
| `INumberSequence` | `NumberSequence` | Generates unique sequential document numbers for orders, invoices, bills, etc. |
| `IRoles` | `Roles` | Role creation, assignment, and super-admin bootstrapping |
| `IEmailSender` | `EmailSender` | Email dispatch via SendGrid or SMTP |
| `INotificationService` | `NotificationService` | Creating and retrieving in-app notifications |
| `ISliderCaptchaService` | `SliderCaptchaService` | CAPTCHA verification for bot protection |
| `IFileParserService` | `FileParserService` | Parsing uploaded files (Excel via ClosedXML) |

### Configuration Options Classes

| Class | Purpose |
|---|---|
| `IdentityDefaultOptions` | Binds password, lockout, and sign-in policy settings from `appsettings.json` |
| `SuperAdminDefaultOptions` | Stores default super-admin credentials for initial seeding |
| `SmtpOptions` | SMTP server configuration |
| `SendGridOptions` | SendGrid API key configuration |

---

## API Controllers

A comprehensive set of RESTful API controllers was built under `Controllers/Api/` to serve the frontend with JSON data. Each controller follows the same pattern:

- Constructor injection of `ApplicationDbContext`
- Standard CRUD endpoints (`Get`, `GetById`, `Insert`, `Update`, `Delete`)
- `CrudViewModel` support for grid data binding

### API Controllers Implemented

| Controller | Resource |
|---|---|
| `ProductController` | Products |
| `ProductTypeController` | Product types |
| `CustomerController` | Customers |
| `CustomerTypeController` | Customer types |
| `VendorController` | Vendors |
| `VendorTypeController` | Vendor types |
| `PurchaseOrderController` | Purchase orders |
| `PurchaseOrderLineController` | Purchase order lines |
| `SalesOrderController` | Sales orders |
| `SalesOrderLineController` | Sales order lines |
| `InvoiceController` | Invoices |
| `InvoiceTypeController` | Invoice types |
| `BillController` | Bills |
| `BillTypeController` | Bill types |
| `PaymentReceiveController` | Payment receipts |
| `PaymentVoucherController` | Payment vouchers |
| `PaymentTypeController` | Payment types |
| `ShipmentController` | Shipments |
| `ShipmentTypeController` | Shipment types |
| `GoodsReceivedNoteController` | Goods received notes |
| `CurrencyController` | Currencies |
| `CashBankController` | Cash/bank accounts |
| `WarehouseController` | Warehouses |
| `BranchController` | Branches |
| `UnitOfMeasureController` | Units of measure |
| `NumberSequenceController` | Number sequences |
| `UserController` | User management |
| `RoleController` | Role management |
| `CaptchaController` | CAPTCHA verification |
| `ChangePasswordController` | Password changes |
| `UploadProfilePictureController` | Profile picture uploads |

---

## Data Layer

### ApplicationDbContext

- Extends `IdentityDbContext<ApplicationUser>` to integrate Identity tables alongside business entities.
- Configures all `DbSet<T>` properties for every domain model.
- Database initialization handled by `DbInitializer` which seeds reference data and the super-admin account.

### Initial Migration

- `20260325031729_initialdb` – Creates the complete database schema including all tables, relationships, and indexes.

---

## Team Resources Allocated

| Role | Team Members | Responsibilities |
|---|---|---|
| Backend Lead | 1 | Architecture decisions, DbContext design, service interfaces |
| Backend Developers | 2 | API controllers, model implementation, migrations |
| Team Lead | 1 | Project scaffolding, CI pipeline setup, SQL Server provisioning |
