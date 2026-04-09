# Sprint 3 – Testing Strategy

## Test Pyramid

The team implemented a comprehensive testing strategy following the test pyramid approach:

```
          ┌─────────┐
         │  E2E /   │    ← Selenium browser tests (critical paths)
        │ Automated │
       ├───────────┤
      │ Integration │    ← API tests with WebApplicationFactory
     ├─────────────┤
    │   Unit Tests   │   ← Isolated component tests (largest tier)
   └─────────────────┘
```

---

## Unit Tests (`coderush.UnitTests`)

**80+ test classes** covering every layer of the application.

### Controller Tests

| Test Class | Tests |
|---|---|
| `HomeControllerTests` | Landing page routing |
| `DashboardControllerTests` | Dashboard data loading |
| `ProductControllerTests` | Product CRUD views |
| `CustomerControllerTests` | Customer CRUD views |
| `VendorControllerTests` | Vendor CRUD views |
| `PurchaseOrderControllerTests` | Purchase order workflows |
| `SalesOrderControllerTests` | Sales order workflows |
| `InvoiceControllerTests` | Invoice management |
| `BillControllerTests` | Bill management |
| `PaymentReceiveControllerTests` | Payment receipt handling |
| `PaymentVoucherControllerTests` | Payment voucher handling |
| `ShipmentControllerTests` | Shipment operations |
| `GoodsReceivedNoteControllerTests` | GRN processing |
| `ManageControllerTests` | Profile management |
| `UserRoleControllerTests` | User/role administration |
| *(and all reference data controllers)* | Type, Currency, UoM, etc. |

### API Controller Tests

| Test Class | Tests |
|---|---|
| `Api/ProductControllerTests` | Product API CRUD |
| `Api/CustomerControllerTests` | Customer API CRUD |
| `Api/VendorControllerTests` | Vendor API CRUD |
| `Api/PurchaseOrderControllerTests` | Purchase order API |
| `Api/SalesOrderControllerTests` | Sales order API |
| `Api/SalesOrderLineControllerTests` | Order line API |
| `Api/PurchaseOrderLineControllerTests` | Order line API |
| `Api/InvoiceControllerTests` | Invoice API |
| `Api/BillControllerTests` | Bill API |
| `Api/ChatBotControllerTests` | ChatBot API |
| `Api/CaptchaControllerTests` | CAPTCHA verification |
| `Api/UploadProfilePictureControllerTests` | File upload API |
| *(and all remaining API controllers)* | Full coverage |

### Service Tests

| Test Class | Scope |
|---|---|
| `RolesTests` | Role creation and assignment logic |
| `EmailSenderTests` | Email dispatch via SendGrid |
| `NumberSequenceServiceTests` | Document number generation |
| `NotificationServiceTests` | Notification CRUD operations |
| `FileParserServiceTests` | File parsing (Excel imports) |
| `ChatBotServiceTests` | ChatBot response logic |
| `ChatResponseOrchestratorTests` | Chat orchestration flow |
| `SliderCaptchaServiceTests` | CAPTCHA validation |
| `FunctionalTests` | Database initialization & seeding |

### Other Tests

| Test Class | Scope |
|---|---|
| `ErrorViewModelTests` | Error model binding |
| `HtmlHelpersTests` | Custom HTML helper output |
| `UrlHelperExtensionsTests` | URL generation helpers |
| `EmailSenderExtensionsTests` | Email template extensions |
| `NotificationActionFilterTests` | Notification filter behavior |
| `ManageNavPagesTests` | Navigation page state logic |
| `DbInitializerTests` | Database seed data verification |
| `ApplicationDbContextTests` | Context configuration |
| `profilepictureTests` | Profile picture migration |
| `AddChatLogTests` | Chat log migration |

### Test Infrastructure

- `TestableDbSet<T>` – Custom in-memory `DbSet` mock for unit testing EF Core queries without a database.

---

## Integration Tests (`coderush.IntegrationTests`)

Tests the full HTTP pipeline using `WebApplicationFactory<Program>`.

| Test Class | Scope |
|---|---|
| `AuthenticationApiTests` | Login, registration, token flows |
| `ProductApiTests` | Product CRUD via HTTP |
| `CustomerApiTests` | Customer CRUD via HTTP |
| `VendorApiTests` | Vendor CRUD via HTTP |
| `SalesOrderApiTests` | Sales order creation and querying |
| `PurchaseOrderApiTests` | Purchase order creation and querying |
| `OrderLineApiTests` | Line item operations |
| `TransactionApiTests` | Financial transaction flows |
| `ReferenceDataApiTests` | Master data CRUD |
| `PostmanCollectionRunnerTests` | Validates Postman collection compatibility |

### Infrastructure

- `CustomWebApplicationFactory` – Configures an in-memory test server with test database.
- `AuthHelper` – Generates authentication tokens for protected endpoint tests.
- `TestCaptchaService` – Stubs CAPTCHA validation to allow automated testing.

---

## E2E Tests (`coderush.E2ETests`)

Browser-based Selenium tests that validate complete user journeys.

| Test Class | Workflow Tested |
|---|---|
| `DashboardE2ETests` | Dashboard loads with correct data |
| `AuthorizationE2ETests` | Login, logout, access denied scenarios |
| `MasterDataE2ETests` | CRUD operations on master data grids |
| `SalesCycleE2ETests` | Full sales cycle (order → shipment → invoice → payment) |
| `PurchaseCycleE2ETests` | Full purchase cycle (order → GRN → bill → payment) |
| `NegativeFlowE2ETests` | Validation errors, edge cases, invalid inputs |

### Infrastructure

- `E2ETestBase` / `SeleniumTestBase` – Base classes managing browser lifecycle.
- `TestWebServer` – Launches the application for E2E testing.
- `LoginPage` – Page object for authentication flows.
- `SyncfusionGridPage` – Page object for interacting with Syncfusion grids.

---

## Automated UI Tests (`coderush.AutomatedTests`)

Focused UI automation tests for specific features.

| Test Class | Feature Tested |
|---|---|
| `AuthenticationTests` | Login/logout flows |
| `NavigationTests` | Page-to-page navigation |
| `SidebarNavigationTests` | Sidebar menu interactions |
| `ProductCrudTests` | Product create, read, update, delete |
| `CustomerCrudTests` | Customer CRUD |
| `VendorCrudTests` | Vendor CRUD |
| `SalesOrderTests` | Sales order workflows |
| `PurchaseOrderTests` | Purchase order workflows |
| `TransactionGridTests` | Transaction grid interactions |
| `ReferenceDataGridTests` | Reference data grid CRUD |
| `UserProfileTests` | Profile viewing and editing |
