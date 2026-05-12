# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SRJE (Sistema de Retenciones Judiciales Electrónicas) — Chilean judicial withholding system for payroll processing. .NET 8 backend with Vue 3 SPA frontend, backed by Oracle database.

## Build & Run Commands

```bash
# Build backend
dotnet build src/SRJE.Web/SRJE.Web.csproj

# Run backend (port 5000)
dotnet run --project src/SRJE.Web

# Run tests
dotnet test tests/SRJE.Tests/SRJE.Tests.csproj

# Run a single test
dotnet test tests/SRJE.Tests/SRJE.Tests.csproj --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Frontend dev server (port 5173, proxies /api to backend)
cd src/SRJE.Web/ClientApp && npm run dev

# Frontend production build (outputs to wwwroot/)
cd src/SRJE.Web/ClientApp && npm run build
```

For local development, run backend and frontend separately. The Vite dev server proxies `/api` requests to the backend.

## Architecture

**Backend (src/SRJE.Web/):**
- **Controllers/** → API endpoints under `/api/`. Returns JSON, never redirects (401/403 for auth).
- **Services/** → Business logic. All registered as scoped DI services in Program.cs.
- **Infrastructure/Data/SrjeDbContext.cs** → EF Core with Oracle provider. Entity mappings use UPPER_CASE Oracle table/column naming convention via Fluent API.
- **Models/Entities/** → DB entities. **Models/Requests/** → input DTOs. **Models/ViewModels/** → output DTOs.
- **Parsers/** → Fixed-width file parsers for Chilean payroll formats (remuneraciones, TEMGE, nuevas cuentas).
- **Helpers/** → RutHelper (Chilean RUT validation/formatting), FixedWidthHelper.
- **Middleware/GlobalExceptionMiddleware.cs** → Catches ArgumentException (400), KeyNotFoundException (404), BusinessConflictException (409).
- **Models/Exceptions/** → Custom exceptions (BusinessConflictException for 409 conflicts).

**Frontend (src/SRJE.Web/ClientApp/):**
- Vue 3 Composition API + Vite + Vue Router + Pinia + Axios
- **src/api/** → Axios client with `/api` base URL, cookie credentials, 401→login redirect
- **src/stores/** → Pinia stores (auth, beneficiarios, funcionarios)
- **src/views/** → Page components. **src/components/** → Reusable components (RutInput, FileUpload, PreviewImportacion, AlertMessage, ConfirmModal).
- **src/router.js** → Vue Router with auth guard; all routes except `/login` require authentication.

## Key Domain Patterns

- **Preview-then-commit**: Import flows show a preview (dry-run) before persisting. Preview endpoints return DTOs; confirm endpoints write to DB.
- **Audit logging**: All imports log to LogCarga/LogCargaDetalle tables. Field changes tracked in AuditoriaCambios.
- **Pluggable authentication**: IAuthService interface. Currently uses DevAuthService with hardcoded users (admin/operador/consulta). Production should swap in LDAP/AD/OAuth implementation.
- **RUT handling**: Chilean national ID format. Always validate with RutHelper. Stored without formatting in DB.

## Configuration

Key settings in `appsettings.json` under `Srje` section: CodEmpresa, CodBancoEstado, LargoCtaEstado, LargoCtaOtBanco, LargoNombreTemge. Auth provider configured under `Auth.Provider`.

## Key Packages

- **Oracle.EntityFrameworkCore** — Oracle DB provider for EF Core
- **Newtonsoft.Json** — JSON serialization (used via `AddNewtonsoftJson`; null values ignored, reference loops ignored)
- **EPPlus** — Excel file generation
- **Serilog** — Structured logging to console + rolling daily files (`logs/srje-*.log`, 30-day retention)

## Database Scripts

SQL migration/setup scripts live in `oracle/` at the repo root. These are manually applied against the Oracle database.

## Testing

xUnit with Moq and FluentAssertions. Tests cover Helpers, Parsers, and Services. Service tests mock the DbContext.

## Coding Guidelines

### Think Before Coding
- When a request is ambiguous, ask for clarification before implementing. Do not guess intent.
- State assumptions explicitly. If a change could affect parsers, import flows, or audit logging, say so before proceeding.
- If a simpler approach exists (fewer queries, less code, reusing existing helpers), propose it first.
- Surface tradeoffs: "This changes the DB schema — do you want a migration script in `oracle/`?"

### Simplicity First
- Write only the minimum code that solves the stated problem. No speculative features.
- Don't create abstractions for single-use code. Three similar lines are better than a premature helper.
- If 200 lines can be reduced to 50, rewrite. Apply the senior engineer test: would an experienced developer call this overcomplicated?
- Skip error handling for impossible scenarios. Trust internal code and EF Core guarantees. Only validate at system boundaries (controller inputs, file parsing).

### Surgical Changes
- Only modify code directly related to the request. Don't improve adjacent code, comments, or formatting.
- Match existing style: Spanish variable names in domain code, English in infrastructure. Don't refactor conventions.
- Don't add docstrings, type annotations, or comments to code you didn't change.
- Remove only imports/variables made unused by your specific changes. Don't delete pre-existing dead code unless asked.
- Test: every changed line should directly trace to the user's request.

### Goal-Driven Execution
- Before implementing, define what "done" looks like:
  - "Add validation" → write a test for invalid input, then make it pass.
  - "Fix the bug" → write a test reproducing it, then fix.
  - "Refactor X" → ensure tests pass before and after.
- For multi-step tasks, verify each step before moving to the next.
- After finishing, run `dotnet build` and/or `npm run build` to confirm nothing is broken.
