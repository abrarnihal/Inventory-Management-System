# Sprints Brief – Inventory Management System

## Project Summary

The **Inventory Management System** (codenamed *coderush*) is a full-stack web application built on **ASP.NET Core (.NET 10)** with Razor views, Entity Framework Core, and SQL Server. It was delivered by a **6-person team** over **3 sprints (6 weeks)**, completing **62 user stories** totalling **146 story points**.

---

## Sprint 1 – Project Structure & Functional Backend

**Duration:** 2 weeks | **Stories:** 18 | **Points:** 42

### What Was Done

- Established the solution structure: one main web project (`coderush`) plus four test projects (Unit, Integration, E2E, Automated).
- Designed and implemented **25+ domain models** spanning master data (Product, Customer, Vendor, Warehouse, Branch, Currency), transactions (PurchaseOrder, SalesOrder, GoodsReceivedNote, Shipment), and financials (Invoice, Bill, PaymentReceive, PaymentVoucher).
- Built **30+ RESTful API controllers** under `Controllers/Api/` with full CRUD support and Syncfusion `CrudViewModel` binding.
- Created the **service layer** with interfaces and implementations for number sequencing (`INumberSequence`), role management (`IRoles`), email dispatch (`IEmailSender` via SendGrid), and database initialization (`IFunctional`).
- Configured **ASP.NET Core Identity** with customisable password, lockout, and sign-in policies bound from `appsettings.json`.
- Generated the initial database migration (`20260325031729_initialdb`) and seed logic in `DbInitializer`.

### Processes & Resources

- **Agile/Scrum** ceremonies: daily standups, sprint planning, review, and retrospective.
- Git feature-branch workflow with mandatory pull-request reviews.
- Team: 1 Project Manager, 1 Solution Architect, 2 Backend Developers, 1 DevOps Engineer, 1 QA Lead.

---

## Sprint 2 – Frontend Development & Backend Adjustments

**Duration:** 2 weeks | **Stories:** 24 | **Points:** 56

### What Was Done

- Implemented **35+ Razor views** organised by feature: Dashboard, Products, Customers, Vendors, Purchase Orders (with Detail), Sales Orders (with Detail), Invoices, Bills, Payments, Shipments, GRN, Warehouses, Branches, and all reference-data types.
- Created corresponding **MVC page controllers** for every view (e.g., `DashboardController`, `ProductController`, `SalesOrderController`).
- Integrated **Syncfusion data grids** for inline CRUD editing, sorting, filtering, and paging across all list views.
- Built the **shared layout** with a responsive sidebar navigation (`MainMenu`), top bar with notification badge, and Bootstrap-based styling.
- Delivered full **account management** pages: Login, Register, 2FA, Lockout, Forgot Password, and profile management (Change Password, Enable Authenticator, External Logins).
- Added **profile picture upload** support with a new migration (`20260326002948_profilepicture`) and `UploadProfilePictureController`.
- Implemented `NotificationActionFilter` to inject unread notification counts into every page automatically.
- Enhanced `Program.cs` with a **dual connection-string fallback** (Azure SQL → LocalDB) and `.env` file loading for secret management.

### Processes & Resources

- Mid-sprint UI review with stakeholders; wireframes for Dashboard and Order Detail pages.
- Cross-functional pairing between frontend and backend developers for API integration.
- Browser testing across Chrome, Edge, and Firefox.
- Team: 1 Project Manager, 1 Frontend Lead, 2 Frontend Developers, 1 Backend Developer, 1 DevOps Engineer.

---

## Sprint 3 – Testing, Additional Features & Deployment Readiness

**Duration:** 2 weeks | **Stories:** 20 | **Points:** 48

### What Was Done

**Testing (4 tiers):**

| Tier | Project | Scope |
|---|---|---|
| Unit | `coderush.UnitTests` | **80+ test classes** – every controller, API controller, service, helper, filter, migration, and DbContext |
| Integration | `coderush.IntegrationTests` | **10 test classes** – full HTTP pipeline tests via `WebApplicationFactory`, plus Postman collection runner |
| E2E | `coderush.E2ETests` | **6 Selenium test classes** – Dashboard, Auth, Master Data, Sales Cycle, Purchase Cycle, Negative Flows |
| Automated UI | `coderush.AutomatedTests` | **11 test classes** – Navigation, Sidebar, CRUD for Products/Customers/Vendors, Orders, Grids, User Profile |

**Additional Features:**

- **AI ChatBot** – OpenAI-powered assistant with `ChatBotService`, `ChatResponseOrchestrator`, `ChatBotController`, `ChatLog` model, and 3 supporting migrations (AddChatLog, AddConversationTitle, AddConversationIsPinned).
- **Notification System** – `NotificationService`, `Notification`/`NotificationReadStatus` models, `NotificationController` API.
- **Slider CAPTCHA** – `SliderCaptchaService` with `TestCaptchaService` stubs for automated testing.
- **File Import** – `FileParserService` for bulk Excel imports via ClosedXML.
- **Health Endpoint** – `HealthController` returning HTTP 200 for Azure App Service probes.

**Azure Deployment:**

- `20260402130117_AzureSqlMigration` for cloud SQL compatibility.
- `Azure.Identity` integration for Entra ID managed-identity authentication.

### Processes & Resources

- Test-driven development (TDD) for the ChatBot feature.
- Bug triage meetings 3× per week; quality gates enforced (zero critical defects, full build pass).
- CI pipeline running all four test projects on every pull request.
- Team: 1 Project Manager, 1 QA Lead, 2 QA Engineers, 1 Full-Stack Developer, 1 DevOps Engineer.

---

## At a Glance

| Metric | Sprint 1 | Sprint 2 | Sprint 3 | **Total** |
|---|---|---|---|---|
| Duration | 2 weeks | 2 weeks | 2 weeks | **6 weeks** |
| Stories | 18 | 24 | 20 | **62** |
| Story Points | 42 | 56 | 48 | **146** |
| Team Size | 6 | 6 | 6 | **6** |
| Focus | Backend & Architecture | Frontend & UI | Testing & Features | — |
