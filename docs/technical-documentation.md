# Technical Documentation

## Purpose

This document complements the top-level README by giving technical reviewers and collaborators a compact reference point for how the system is structured, how to work with it locally, and where the current implementation boundaries are.

## Current Implementation Scope

The repository currently contains a substantial MVP foundation with the following implemented areas:

- authentication and authorization
- patient registration
- duplicate patient detection
- patient search and retrieval
- visit capture
- audit logging
- reporting
- deployment and testing documentation
- shared desktop design system

## Module Overview

### Authentication

Responsibilities:

- user login
- PBKDF2 password verification
- session creation and logout
- role-based feature access
- login audit tracking
- temporary lockout after repeated failures

Key files:

- `src/ClinicAdmin.Infrastructure/Security/AuthenticationService.cs`
- `src/ClinicAdmin.Infrastructure/Security/Pbkdf2PasswordHasher.cs`
- `src/ClinicAdmin.Infrastructure/Security/UserSessionService.cs`
- `src/ClinicAdmin.Application/Authorization/AuthorizationService.cs`

### Patients

Responsibilities:

- new patient registration
- duplicate warning/confirmation flow
- patient search by multiple identifiers
- profile retrieval

Key files:

- `src/ClinicAdmin.Application/Patients/Commands/RegisterPatient/*`
- `src/ClinicAdmin.Application/Patients/Queries/SearchPatients/*`
- `src/ClinicAdmin.Application/Patients/DuplicateDetection/*`

### Visits

Responsibilities:

- register patient arrival
- maintain visit state and queue status
- capture reason, notes, department, assigned staff
- visit history retrieval

Key files:

- `src/ClinicAdmin.Application/Visits/Commands/RegisterVisit/*`
- `src/ClinicAdmin.Domain/Visits/*`

### Auditing And Logging

Responsibilities:

- audit login attempts and workflow actions
- persist structured audit records
- expose audit data to managers/admins
- log operational events through Serilog

Key files:

- `src/ClinicAdmin.Infrastructure/Auditing/AuditService.cs`
- `src/ClinicAdmin.Application/Auditing/AuditLogQueryService.cs`
- `src/ClinicAdmin.Desktop/ViewModels/AuditLogViewModel.cs`

### Reporting

Responsibilities:

- daily registrations
- visits per day
- common visit reasons
- staff activity summary
- patient visit history summary

Key files:

- `src/ClinicAdmin.Application/Reports/Queries/ReportingService.cs`
- `src/ClinicAdmin.Desktop/ViewModels/ReportsViewModel.cs`
- `src/ClinicAdmin.Desktop/Services/ReportExportService.cs`

## Environment Strategy

### Development

- SQLite
- dev-oriented logging
- demo-safe testing and local iteration

### Production

- PostgreSQL
- file-based structured logging
- seeded default users disabled
- stronger auth controls enabled

## Database Strategy

The application supports:

- SQLite for local development/demo
- PostgreSQL for production

Current limitation:

- the project still requires the initial EF Core migration set to be generated and checked in

Until that is complete, production rollout should be considered preparation-ready but not migration-complete.

## Logging Strategy

Serilog is configured through application settings. Production is intended to use rolling file logs rather than console-first output. Logs are intended for operational troubleshooting, not for storing raw patient content.

## Security And Privacy Notes

The codebase currently includes:

- PBKDF2 password hashing
- role-based navigation
- audit logging
- temporary login lockout
- production seeding controls

Still recommended:

- idle session timeout
- stronger production secret management
- backup restore validation
- explicit production migration flow

## Testing Summary

Current automated tests cover:

- domain rules
- validators
- authentication and password hashing
- duplicate detection
- patient registration
- visit capture
- audit persistence and querying
- reporting aggregation
- desktop view-model behavior

Reference:

- [Testing Strategy](testing/testing-strategy.md)

## Technical Reviewer Notes

This repository is designed to demonstrate:

- architectural separation
- workflow-driven product thinking
- public-sector admin problem solving
- practical WPF engineering
- QA and deployment discipline

The app is intentionally being built as a strong single-clinic foundation before introducing multi-branch synchronization, richer file logistics, and broader hospital operations support.
