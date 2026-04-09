# Sprint 3 – Additional Features & Advancements

## Notification System

### Implementation

- `INotificationService` / `NotificationService` – Creates, retrieves, and marks notifications as read.
- `Notification` model – Stores notification content, target user, and creation timestamp.
- `NotificationReadStatus` model – Tracks per-user read/unread state.
- `NotificationActionFilter` – MVC action filter that injects unread notification count into every page.
- `Api/NotificationController` – API endpoints for fetching and managing notifications.

### User Experience

- Notification badge in the navigation bar shows unread count.
- Users can view, dismiss, and mark notifications as read.

---

## Health Check Endpoint

- `HealthController` – Provides a `/health` endpoint for monitoring and load balancer probes.
- Returns HTTP 200 when the application is healthy, enabling Azure App Service health monitoring.

---

## Slider CAPTCHA

- `ISliderCaptchaService` / `SliderCaptchaService` – Implements a slider-based CAPTCHA mechanism for bot protection on public-facing forms.
- `Api/CaptchaController` – Serves CAPTCHA challenges and validates responses.
- `TestCaptchaService` – Stub implementation used in all test projects to bypass CAPTCHA during automated testing.

---

## File Parsing (Excel Import)

- `IFileParserService` / `FileParserService` – Parses uploaded Excel files using ClosedXML and DocumentFormat.OpenXml.
- Enables bulk data import for products, customers, vendors, and other master data entities.

---

## Azure Deployment Preparation

### Azure SQL Migration

**Migration:** `20260402130117_AzureSqlMigration`

- Schema adjustments for Azure SQL Database compatibility.
- Ensures all column types and constraints are compatible with the cloud database.

### Azure Identity Integration

- `Azure.Identity` package (v1.20.0) integrated for managed identity authentication.
- Supports connecting to Azure SQL Database using Entra ID (Azure AD) tokens instead of connection string passwords.

### Connection String Strategy

```
Development:  LocalDB (automatic fallback)
Production:   Azure SQL Database (via DefaultConnection)
```

The application probes the primary Azure SQL connection at startup and gracefully falls back to LocalDB for offline development.

---

## Database Migrations Summary (Sprint 3)

| Migration | Date | Purpose |
|---|---|---|
| `AddChatLog` | 2026-03-29 | Chat message persistence |
| `AddConversationTitle` | 2026-03-29 | Conversation naming |
| `AddConversationIsPinned` | 2026-03-29 | Conversation bookmarking |
| `AzureSqlMigration` | 2026-04-02 | Azure SQL compatibility |
