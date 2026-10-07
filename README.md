# AcxiomCRM — ASP.NET Core MVC Application

## Overview
A full-featured Customer Relationship Management (CRM) system built for a college project assignment.

**Tech Stack:** ASP.NET Core 8 MVC · C# · Entity Framework Core · SQLite · ASP.NET Core Identity · Bootstrap 5 · Chart.js · REST APIs · Swagger

---

## Features

- **Authentication & Authorization** — ASP.NET Core Identity with Role-Based Access (Admin / Manager / SalesExecutive)
- **Customer Management** — Full CRUD with email/phone uniqueness validation
- **Lead Management** — Pipeline management with Lead Conversion into Customers & Opportunities
- **Opportunity Management** — Sales pipeline with weighted value calculations
- **Follow-Up Management** — Schedule, complete, and reschedule sales follow-ups
- **Activity Tracking** — Log calls, meetings, emails, and tasks
- **Dashboard** — KPI cards + Chart.js charts (Lead Status, Pipeline Stages, Monthly Revenue)
- **Audit Logging** — Tracks all security events and CRUD operations
- **REST APIs** — `/api/customers`, `/api/leads`, `/api/opportunities` with Swagger UI at `/swagger`
- **Reports** — 8 report types (Customer, Lead, Opportunity, Pipeline, Follow-Up, Conversion, Activity, Audit)

---

## Getting Started

### Prerequisites
- .NET 8 SDK (`curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0`)

### Run Locally
```bash
git clone https://github.com/YOUR_USERNAME/axciom.git
cd axciom
export PATH="$HOME/.dotnet:$PATH"
dotnet run --urls "http://localhost:5000"
```

Navigate to **http://localhost:5000**

---

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | `admin@acxiomcrm.com` | `AcxiomPass123!` |
| **Manager** | `manager@acxiomcrm.com` | `AcxiomPass123!` |
| **Sales Executive** | `sales@acxiomcrm.com` | `AcxiomPass123!` |

---

## Database

Uses **SQLite** via EF Core. The database is automatically created and seeded with demo data on first startup — no manual setup required.

---

## API Documentation

Swagger UI available at `/swagger` when running locally.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/customers` | List all customers |
| GET | `/api/customers/{id}` | Get customer by ID |
| POST | `/api/customers` | Create customer |
| PUT | `/api/customers/{id}` | Update customer |
| DELETE | `/api/customers/{id}` | Delete customer |
| GET | `/api/leads` | List all leads |
| POST | `/api/leads` | Create lead |
| GET | `/api/opportunities` | List all opportunities |
| POST | `/api/opportunities` | Create opportunity |

---

## Project Structure

```
axciom/
├── Controllers/          # MVC controllers (Account, Customer, Lead, Opportunity, ...)
│   └── Api/              # REST API controllers
├── Data/                 # DbContext + DbInitializer (seeding)
├── Models/               # EF Core entity models + DTOs
├── Services/             # Business logic (CrmService, AuditLogService)
├── ViewModels/           # ViewModels for MVC views
├── Views/                # Razor views (Bootstrap 5 UI)
├── AcxiomCRM.Tests/      # xUnit acceptance tests (6/6 passing)
└── Program.cs            # App configuration + DI setup
```

---

## Running Tests

```bash
dotnet test AcxiomCRM.Tests/AcxiomCRM.Tests.csproj
```

**Result:** `Passed! - Failed: 0, Passed: 6, Total: 6`

---

## Security Features

- ASP.NET Core Identity password hashing (PBKDF2)
- Password policy: uppercase, lowercase, digit, special char, min length 6
- Account lockout after 5 failed attempts (5-minute lockout)
- Anti-forgery tokens on all POST forms
- EF Core parameterized queries (SQL injection prevention)
- Server-side authorization enforced (not just UI hiding)
- Role-based access scoping for SalesExecutive data isolation
