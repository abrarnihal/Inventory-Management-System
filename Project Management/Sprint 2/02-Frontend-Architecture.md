# Sprint 2 – Frontend Architecture

## View Organization

The frontend follows the ASP.NET Core MVC conventions with Razor views organized by feature area:

```
Views/
├── Account/                # Authentication pages
│   ├── Login.cshtml
│   ├── Register.cshtml
│   ├── ForgotPassword.cshtml
│   ├── ConfirmEmail.cshtml
│   ├── ConfirmEmailNotification.cshtml
│   ├── ExternalLogin.cshtml
│   ├── Lockout.cshtml
│   ├── AccessDenied.cshtml
│   ├── LoginWith2fa.cshtml
│   └── LoginWithRecoveryCode.cshtml
├── Manage/                 # Profile management
│   ├── Index.cshtml
│   ├── ChangePassword.cshtml
│   ├── EnableAuthenticator.cshtml
│   ├── Disable2fa.cshtml
│   ├── ExternalLogins.cshtml
│   ├── GenerateRecoveryCodes.cshtml
│   └── ManageNavPages.cs
├── Dashboard/
│   └── Index.cshtml
├── Product/
│   └── Index.cshtml
├── ProductType/
│   └── Index.cshtml
├── Customer/
│   └── Index.cshtml
├── CustomerType/
│   └── Index.cshtml
├── Vendor/
│   └── Index.cshtml
├── VendorType/
│   └── Index.cshtml
├── PurchaseOrder/
│   ├── Index.cshtml
│   └── Detail.cshtml
├── SalesOrder/
│   ├── Index.cshtml
│   └── Detail.cshtml
├── Invoice/ ... Bill/ ... PaymentReceive/ ... PaymentVoucher/ ...
├── Shipment/ ... GoodsReceivedNote/ ...
├── Warehouse/ ... Branch/ ... Currency/ ... CashBank/ ... UnitOfMeasure/ ...
├── UserRole/
│   ├── Index.cshtml
│   ├── ChangePassword.cshtml
│   └── ChangeRole.cshtml
└── Shared/
    └── Error.cshtml
```

## Razor Pages

- `Pages/MainMenu.cs` – Defines the sidebar navigation structure used across all pages.

---

## MVC Page Controllers

Each business view is served by a corresponding MVC controller:

| Controller | Views Served |
|---|---|
| `HomeController` | Landing / redirect logic |
| `DashboardController` | Dashboard analytics |
| `ProductController` | Product management |
| `ProductTypeController` | Product type management |
| `CustomerController` | Customer management |
| `CustomerTypeController` | Customer type management |
| `VendorController` | Vendor management |
| `VendorTypeController` | Vendor type management |
| `PurchaseOrderController` | Purchase orders + detail |
| `SalesOrderController` | Sales orders + detail |
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
| `SalesTypeController` | Sales types |
| `PurchaseTypeController` | Purchase types |
| `AccountController` | Login, Register, 2FA |
| `ManageController` | Profile management |
| `UserRoleController` | User and role administration |

---

## UI Components & Libraries

| Component | Purpose |
|---|---|
| **Data Grids** | Inline CRUD editing, sorting, filtering, and paging for all list views |
| **CrudViewModel** | Standardized data binding model for grid operations |
| **Bootstrap** | Responsive layout and styling |
| **wwwroot/** | Static assets – stylesheets, scripts, images |
