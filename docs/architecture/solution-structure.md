# Initial Solution Structure

This document maps the intended near-term structure for the clinic administration desktop application.

## Source Projects

- `src/ClinicAdmin.Desktop`
  - `Views`: windows and user controls
  - `ViewModels`: MVVM view models
  - `Navigation`: shell and workflow navigation services
  - `Services`: UI-facing services such as notifications and exception presentation
- `src/ClinicAdmin.Application`
  - `Abstractions`: ports for time, auditing, user context, facility context, sync journaling, and persistence
  - `Common/Validation`: validators and validation result types
  - `Common/Exceptions`: application-level exceptions
  - `Common/Results`: predictable operation results for UI workflows
  - `Patients/Commands`: write workflows
  - `Patients/Queries`: read workflows
- `src/ClinicAdmin.Domain`
  - `Common`: entity base classes and shared domain primitives
  - `Patients`: patient aggregate logic
  - `Files`: physical file tracking domain
  - `Auditing`: future domain audit types if needed
- `src/ClinicAdmin.Infrastructure`
  - `Configuration`: typed options
  - `Persistence`: `DbContext`, EF mappings, and migrations
  - `Clock`: time abstraction implementations
  - `Auditing`: audit service implementations
  - `Security`: current user and facility context adapters
  - `Sync`: sync journal and future branch replication components
- `src/ClinicAdmin.Contracts`
  - DTOs for list/detail/search operations

## Test Projects

- `tests/ClinicAdmin.Domain.Tests`: domain invariants and entity behavior
- `tests/ClinicAdmin.Application.Tests`: handlers, validators, orchestration
- `tests/ClinicAdmin.Infrastructure.Tests`: EF mappings, provider behavior, audit persistence
- `tests/ClinicAdmin.Desktop.Tests`: view model behavior and UI service adapters
