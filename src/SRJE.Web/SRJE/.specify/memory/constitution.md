<!--
=== Sync Impact Report ===
Version change: N/A (initial) -> 1.0.0
Modified principles: N/A (first ratification)
Added sections:
  - Core Principles (5): Simplicity First, Preview-then-Commit,
    Audit Everything, Domain Integrity, Surgical Changes
  - Development Workflow
  - Governance
Removed sections:
  - [SECTION_3_NAME] template slot removed (not needed)
Templates requiring updates:
  - .specify/templates/plan-template.md — dynamic Constitution Check,
    no structural update needed
  - .specify/templates/spec-template.md — no update needed
  - .specify/templates/tasks-template.md — no update needed
Follow-up TODOs: None
========================
-->

# SRJE Constitution

## Core Principles

### I. Simplicity First

- All code MUST solve only the stated problem. No speculative features,
  no premature abstractions.
- Single-use code MUST NOT be wrapped in helper classes or generic
  abstractions. Three similar lines are preferable to a premature helper.
- If an implementation exceeds the minimum necessary complexity, it MUST
  be simplified. Apply the senior engineer test: would an experienced
  developer call this overcomplicated?
- Error handling MUST only exist at system boundaries (controller inputs,
  file parsing). Internal code MUST trust framework guarantees (EF Core,
  DI container) without defensive checks for impossible scenarios.
- Every line of code MUST be justifiable against the requirement it
  fulfills. If it cannot be traced to a requirement, it MUST be removed.

### II. Preview-then-Commit

- All destructive or data-mutating import operations MUST implement a
  two-phase flow: preview (dry-run) followed by explicit user
  confirmation before persisting.
- Preview endpoints MUST return read-only DTOs that describe what will
  happen without modifying any database state.
- Confirm endpoints MUST be separate API calls that write to the database
  only after the user has reviewed the preview.
- No import or bulk-write operation MAY bypass the preview step.
  Single-record CRUD operations are exempt.
- Preview DTOs MUST include per-record status indicators (OK, ERROR,
  ADVERTENCIA, NUEVO) so the user can make an informed confirmation
  decision.

### III. Audit Everything

- All data mutations from import operations MUST be logged to LogCarga
  (header) and LogCargaDetalle (per-record) tables.
- Individual field-level changes on existing records MUST be tracked in
  the AuditoriaCambios table with old value, new value, field name,
  timestamp, and user identity.
- Audit records MUST capture the acting user and source for every write
  operation.
- Audit logging MUST NOT be conditional or toggleable. It is always on
  in all environments.
- No bulk data operation MAY succeed without producing a corresponding
  LogCarga entry. A missing audit trail is a bug.

### IV. Domain Integrity

- Chilean RUT values MUST be validated using RutHelper (modulo-11
  algorithm) at every system boundary where RUTs enter the system.
- RUTs MUST be stored as numeric values without formatting in the
  database. Formatting (dots, hyphens) is applied only at display time.
- Fixed-width file parsers (remuneraciones, TEMGE, nuevas cuentas) MUST
  match Chilean payroll format specifications exactly. Column positions,
  lengths, and padding MUST NOT deviate from the spec.
- Configuration values (CodEmpresa, CodBancoEstado, LargoCtaEstado,
  LargoCtaOtBanco, LargoNombreTemge) MUST be sourced from
  appsettings.json, never hardcoded.
- Oracle table and column names MUST use UPPER_CASE naming convention
  via EF Core Fluent API. Entity mappings MUST NOT rely on conventions
  or annotations for Oracle naming.

### V. Surgical Changes

- Code modifications MUST be limited to files and lines directly related
  to the current request. Adjacent code, comments, or formatting MUST
  NOT be changed.
- Domain code MUST use Spanish variable and method names. Infrastructure
  code MUST use English names. These conventions MUST NOT be mixed or
  refactored.
- Docstrings, type annotations, or comments MUST NOT be added to code
  that was not otherwise modified by the current change.
- Only imports and variables made unused by the current change MAY be
  removed. Pre-existing dead code MUST NOT be deleted unless explicitly
  requested.
- Every changed line MUST trace directly to the user's request. If a
  reviewer cannot identify which requirement drove a change, the change
  MUST be reverted.

## Development Workflow

- Before any implementation begins, define what "done" looks like: a
  failing test for bugs, acceptance criteria for features, passing tests
  before and after for refactors.
- After completing any code change, run
  `dotnet build src/SRJE.Web/SRJE.Web.csproj` to verify backend
  compilation and `npm run build` (in ClientApp) to verify frontend
  compilation. A change that breaks the build MUST NOT be committed.
- All backend tests MUST pass via
  `dotnet test tests/SRJE.Tests/SRJE.Tests.csproj` before a change is
  considered complete.
- Database schema changes MUST be accompanied by a numbered SQL migration
  script in `oracle/`. Schema changes MUST NOT be applied only via EF
  Core; the Oracle script is the source of truth.
- When a request is ambiguous, ask for clarification before implementing.
  State assumptions explicitly, especially when changes could affect
  parsers, import flows, or audit logging.

## Governance

- This constitution supersedes all informal practices. When a principle
  conflicts with convenience, the principle wins.
- Amendments require: (1) a stated rationale, (2) review of impact on
  all 5 principles, (3) version increment following semantic versioning
  (MAJOR for principle removals/redefinitions, MINOR for
  additions/expansions, PATCH for clarifications).
- The Sync Impact Report (HTML comment at the top of this file) MUST be
  updated with every amendment.
- All implementation plans MUST include a Constitution Check section
  validating compliance with these principles before work begins.
- Use CLAUDE.md as the runtime development guidance file. The
  constitution defines the "what" and "why"; CLAUDE.md provides the
  "how" for day-to-day coding.

**Version**: 1.0.0 | **Ratified**: 2026-05-13 | **Last Amended**: 2026-05-13
