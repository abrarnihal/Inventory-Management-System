# Sprint 1 – Domain Models

## Overview

The domain model was designed to capture the full inventory lifecycle: purchasing, warehousing, sales, invoicing, shipping, and financial transactions.

---

## Entity Catalog

### Master Data Entities

| Entity | Description |
|---|---|
| `Product` | Inventory items tracked by the system |
| `ProductType` | Classification categories for products |
| `UnitOfMeasure` | Measurement units (kg, pcs, liters, etc.) |
| `Customer` | Buyer/client records |
| `CustomerType` | Customer classification |
| `Vendor` | Supplier records |
| `VendorType` | Vendor classification |
| `Warehouse` | Storage location definitions |
| `Branch` | Business branch/location entities |
| `Currency` | Supported currency definitions |
| `CashBank` | Cash and bank account references |

### Transaction Entities

| Entity | Description |
|---|---|
| `PurchaseOrder` | Purchase order headers |
| `PurchaseOrderLine` | Line items within purchase orders |
| `PurchaseType` | Purchase classification |
| `SalesOrder` | Sales order headers |
| `SalesOrderLine` | Line items within sales orders |
| `SalesType` | Sales classification |
| `GoodsReceivedNote` | Receiving records for purchased goods |
| `Shipment` | Outbound shipment records |
| `ShipmentType` | Shipment classification |

### Financial Entities

| Entity | Description |
|---|---|
| `Invoice` | Customer invoices |
| `InvoiceType` | Invoice classification |
| `Bill` | Vendor bills |
| `BillType` | Bill classification |
| `PaymentReceive` | Incoming payment records |
| `PaymentVoucher` | Outgoing payment records |
| `PaymentType` | Payment method classification |

### System Entities

| Entity | Description |
|---|---|
| `ApplicationUser` | Extended Identity user with profile fields |
| `UserProfile` | Additional user profile information |
| `NumberSequence` | Auto-incrementing document number generator |
| `Notification` | In-app notification records |
| `NotificationReadStatus` | Tracks read/unread state per user |

---

## Design Principles

1. **Consistent naming** – All entities use PascalCase and singular nouns.
2. **Soft references** – Foreign keys link related entities (e.g., `PurchaseOrder` → `Vendor`, `SalesOrder` → `Customer`).
3. **View models separated** – `AccountViewModels/`, `ManageViewModels/`, and `SyncfusionViewModels/` folders keep presentation concerns out of the domain layer.
