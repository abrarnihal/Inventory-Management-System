# Sprint 2 – UI/UX Design Process

## Design Principles

1. **Consistency** – Every list view follows the same grid-based layout with inline editing, ensuring users learn one interaction pattern.
2. **Efficiency** – Syncfusion grids provide in-place CRUD without requiring separate create/edit pages for reference data.
3. **Progressive disclosure** – Master-detail views (Purchase Order → Detail, Sales Order → Detail) let users drill down into line items.
4. **Responsive** – Bootstrap grid system ensures the application works across desktop and tablet screen sizes.

---

## Page Layout

### Shared Layout Structure

```
┌──────────────────────────────────────────────────┐
│  Top Navigation Bar (User menu, Notifications)   │
├────────────┬─────────────────────────────────────┤
│            │                                     │
│  Sidebar   │        Main Content Area            │
│  Navigation│                                     │
│  (MainMenu)│   ┌─────────────────────────────┐   │
│            │   │  Syncfusion Grid / Form      │   │
│  • Dashboard   │  or Dashboard Widgets        │   │
│  • Products│   │                               │   │
│  • Customers   └─────────────────────────────┘   │
│  • Vendors │                                     │
│  • Purchase│                                     │
│  • Sales   │                                     │
│  • Finance │                                     │
│  • Settings│                                     │
│            │                                     │
└────────────┴─────────────────────────────────────┘
```

### Dashboard

The dashboard provides at-a-glance analytics:
- Summary cards (total products, customers, vendors, orders)
- Recent transaction activity
- Quick-action links to common operations

### Authentication Pages

- Clean, centered form layout for Login, Register, and password recovery flows.
- Two-factor authentication support with authenticator app and recovery codes.

---

## Accessibility & Error Handling

- **Error view** (`Shared/Error.cshtml`) provides a user-friendly error page with the `ErrorViewModel`.
- **Access denied** page for unauthorized access attempts.
- **Lockout** page displayed when accounts are locked due to failed login attempts.
