# Clinic Administration Architecture

## 1. Architecture Overview

The application should use a layered, modular monolith architecture built on WPF, MVVM, and EF Core. The WPF desktop client remains a thin presentation shell. Business workflows live in the application layer, business rules and invariants live in the domain layer, and EF Core plus provider-specific behavior live in infrastructure.

The recommended style is:

- MVVM for presentation concerns
- CQRS-lite inside the application layer
- EF Core as the persistence implementation
- PostgreSQL as the production database
- SQLite as the development and demo database
- explicit abstractions for time, current user, facility context, auditing, and sync journaling

This keeps the codebase testable, scalable by module, and easy to evolve into future offline sync and multi-branch scenarios without splitting into premature microservices.

## 2. Recommended Project Structure

The current five-project solution is the right foundation and should be kept:

- `ClinicAdmin.Desktop`: WPF UI, views, view models, navigation, UI-only services
- `ClinicAdmin.Application`: use cases, validation, orchestration, ports, query/read models
- `ClinicAdmin.Domain`: entities, value objects, enums, domain policies, invariants
- `ClinicAdmin.Infrastructure`: EF Core, database provider configuration, auditing, logging adapters, sync persistence, security adapters
- `ClinicAdmin.Contracts`: DTOs exchanged across boundaries

Recommended internal structure:

```text
src/
  ClinicAdmin.Desktop/
    App.xaml
    App.xaml.cs
    Views/
    ViewModels/
    Navigation/
    Services/
  ClinicAdmin.Application/
    Abstractions/
    Common/
      Behaviors/
      Exceptions/
      Results/
      Validation/
    Patients/
      Commands/
      Queries/
    Files/
      Commands/
      Queries/
    Facilities/
  ClinicAdmin.Domain/
    Common/
    Patients/
    Files/
    Auditing/
    Facilities/
  ClinicAdmin.Infrastructure/
    Configuration/
    Persistence/
      Configurations/
      Migrations/
    Auditing/
    Security/
    Clock/
    Sync/
  ClinicAdmin.Contracts/
    Patients/
    Files/
tests/
  ClinicAdmin.Domain.Tests/
  ClinicAdmin.Application.Tests/
  ClinicAdmin.Infrastructure.Tests/
  ClinicAdmin.Desktop.Tests/
```

## 3. Layers And Responsibilities

### Presentation Layer

- owns windows, views, dialogs, navigation, and view models
- converts UI events into application commands and queries
- displays validation and error feedback
- does not reference EF Core entities directly in XAML

### Application Layer

- owns use cases such as register patient, search patient, track file, transfer file
- coordinates transactions and orchestration
- executes validation before domain changes are persisted
- depends only on domain plus abstractions

### Domain Layer

- owns core entities such as `Patient` and `FileRecord`
- enforces invariants and state transitions
- remains persistence-agnostic and UI-agnostic

### Infrastructure Layer

- implements EF Core persistence and entity configurations
- selects PostgreSQL or SQLite based on configuration
- implements auditing, user context, facility context, system clock, and future sync journal persistence
- owns migrations and provider-specific tuning

### Contracts Layer

- provides DTOs for list screens, search results, details, and imports/exports
- shields the UI from domain entity leakage

## 4. Key Design Patterns

- MVVM for WPF composition and testable UI behavior
- CQRS-lite for separation between write workflows and read/search workflows
- composition root in `App.xaml.cs` for all DI wiring
- ports and adapters for infrastructure dependencies
- aggregate-focused domain model instead of an anemic entity set
- result and validation objects for predictable UI feedback
- outbox or sync-journal pattern as the future integration seam for offline or multi-branch sync

## 5. Dependency Injection Strategy

Use `Microsoft.Extensions.Hosting` as the application host and a single composition root in the desktop project.

Recommended lifetimes:

- singleton: configuration, navigation services, clock implementations, current user context, facility context
- scoped: EF Core `DbContext`, application services that coordinate a unit of work, audit persistence services
- transient: handlers, validators, lightweight view models where appropriate

Guidance:

- avoid injecting scoped services directly into long-lived singleton view models
- create scopes per workflow or per window when a workflow needs database access
- keep all registrations in `AddApplication()` and `AddInfrastructure()` extension methods

## 6. Database Access Approach

Use EF Core with one `ClinicAdminDbContext` in infrastructure.

Recommended approach:

- `DbContext` is the unit of work
- application layer depends on `IApplicationDbContext`
- entity type configuration classes live in infrastructure
- use PostgreSQL in production and SQLite for local/demo mode
- keep migrations in infrastructure
- use read-optimized queries for search screens
- add indexes early for patient search, duplicate detection, file number lookup, and facility scoping

Provider strategy:

- PostgreSQL: primary production provider
- SQLite: local single-user or demo provider with a file-based database

## 7. Validation Strategy

Use layered validation:

- view model validation for immediate UX feedback
- application validation for use-case rules and required fields
- domain validation for invariants that must never be bypassed

Validation should produce structured errors keyed by property or rule so the WPF UI can bind and display them consistently.

## 8. Logging Strategy

Use `ILogger<T>` throughout the application and infrastructure layers with structured logging.

Recommended log events:

- application startup and shutdown
- command and query execution boundaries
- validation failures
- database provider selection and connection mode
- audit creation
- sync journal enqueue and processing outcomes
- unhandled exceptions

For local development, `Debug` and console logging are enough. In production, route logs to a central sink or Windows/EventLog compatible target through an adapter such as Serilog later if needed.

## 9. Audit Strategy

Audit at the business-action level, not only at the SQL level.

Capture:

- action name
- entity type and entity id
- facility id
- current user
- timestamp in UTC
- relevant before/after summary when appropriate

The initial implementation can log audit entries through a service abstraction. The next step should be persisting them in a dedicated audit table for reporting and traceability.

## 10. Error Handling Strategy

Use predictable exception boundaries:

- validation exceptions become user-friendly field errors
- domain exceptions become business-rule messages
- infrastructure exceptions are logged and surfaced as safe, non-technical UI messages
- unhandled exceptions are caught at the application boundary, logged, and shown through a user notification service

Avoid showing raw stack traces to end users.

## 11. Configuration Strategy

Use layered configuration with environment overrides:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`
- environment variables for deployment overrides

Bind configuration into typed options such as:

- `DatabaseOptions`
- `AuditOptions`
- `FacilityOptions`
- `SyncOptions`

## 12. Recommendations For Future Sync And Multi-Branch Support

Design now for branch-aware data even if phase one is single site:

- keep `FacilityId` on aggregates and operational records
- add a `SyncJournal` or outbox table for outbound change tracking
- prefer immutable operational events for file movement history
- store timestamps in UTC everywhere
- introduce optimistic concurrency tokens before multi-writer sync starts
- define conflict rules separately for demographic updates, file movement, and branch reassignment
- keep branch metadata in configuration plus database tables, not hardcoded in the UI

The long-term path should be:

1. single-site desktop with branch-aware schema
2. central server or exchange process consuming sync journal records
3. conflict handling and replay for intermittent connectivity
4. branch-level reporting and reconciliation
