# Sprint 1 – Overview

## Determination of Project Structure & Development of Functional Backend

**Sprint Duration:** 2 weeks  
**Sprint Goal:** Establish the foundational architecture of the Inventory Management System and deliver a fully functional backend capable of supporting all core business operations.

---

## Objectives

1. Define the overall solution structure and project organization.
2. Design and implement the domain model covering all inventory entities.
3. Build RESTful API controllers for every domain resource.
4. Configure Entity Framework Core with SQL Server for data persistence.
5. Set up ASP.NET Core Identity for authentication and role-based authorization.
6. Create seed data and database initialization logic.

---

## Deliverables

| Deliverable | Status |
|---|---|
| Solution structure with `coderush` main project | ✅ Complete |
| Domain models for 25+ entities | ✅ Complete |
| API controllers for all CRUD operations | ✅ Complete |
| ApplicationDbContext with full entity mappings | ✅ Complete |
| Identity configuration (users, roles, lockout, password policies) | ✅ Complete |
| Database migrations (`initialdb`) | ✅ Complete |
| Service layer interfaces and implementations | ✅ Complete |
| Number sequence service for document numbering | ✅ Complete |

---

## Key Decisions

- **Framework:** ASP.NET Core on .NET 10 with the `Microsoft.NET.Sdk.Web` SDK.
- **ORM:** Entity Framework Core 10 with SQL Server provider.
- **Authentication:** ASP.NET Core Identity with configurable password and lockout policies read from `appsettings.json`.
- **API Serialization:** Newtonsoft.Json via `Microsoft.AspNetCore.Mvc.NewtonsoftJson` for Syncfusion compatibility.
- **Email:** SendGrid SDK integrated through `IEmailSender` abstraction.
