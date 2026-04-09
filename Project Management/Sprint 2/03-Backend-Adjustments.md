# Sprint 2 – Backend Adjustments

## Changes Made to Support the Frontend

While the core backend was completed in Sprint 1, several adjustments were required during Sprint 2 to support frontend requirements.

---

## Database Schema Changes

### Profile Picture Migration

**Migration:** `20260326002948_profilepicture`

- Added a profile picture field to the user model to support avatar uploads in the UI.
- The `UploadProfilePictureController` (API) was extended to handle multipart file uploads and persist the image.

---

## Controller Enhancements

### Notification Action Filter

- `NotificationActionFilter` was implemented to inject unread notification counts into every view automatically via `ViewData`.
- This filter runs on every MVC action, ensuring the notification badge in the sidebar is always up-to-date.

### HTML Helpers

- `HtmlHelpers.cs` provides custom Razor helper methods to simplify common rendering patterns across views (e.g., active menu highlighting).

### Extension Methods

- `UrlHelperExtensions.cs` – URL generation helpers for email confirmation and password reset links.
- `EmailSenderExtensions.cs` – Convenience methods for sending templated confirmation and reset emails.

---

## API Refinements

- Several API controllers received minor updates to their response shapes to match the grid's expected `CrudViewModel` format.
- Pagination and filtering parameters were standardized across all list endpoints.

---

## Environment Configuration

### Connection String Fallback

The `Program.cs` startup logic was enhanced with a dual-connection-string strategy:

1. **Primary:** Azure SQL Database connection (`DefaultConnection`).
2. **Fallback:** LocalDB connection (`LocalConnection`) for offline development.

The application automatically probes the primary connection and falls back to LocalDB if Azure SQL is unreachable, ensuring developers can work without cloud connectivity.

### Environment File Support

- `.env` file loading was added to keep secrets out of `appsettings.json`.
- Falls back to `.env.example` when no `.env` file exists.
