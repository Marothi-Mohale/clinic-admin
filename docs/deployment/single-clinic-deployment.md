# Single-Clinic Deployment Guide

## 1. Deployment Architecture

For the first production rollout, deploy the application in a single-clinic model:

- `Reception/records workstations`: Windows desktop clients running the WPF application
- `Clinic database host`: PostgreSQL database on a clinic server or a secure on-prem Windows/Linux host
- `File/log storage`: local application log folder on each workstation with regular collection by support staff if needed
- `Backup target`: encrypted external backup target, NAS, or secure district-managed backup location

Recommended first rollout topology:

1. PostgreSQL hosted on one clinic-local server or trusted on-prem VM
2. 2-10 Windows clients connecting over the clinic LAN
3. One logical facility per deployment
4. No cross-branch sync enabled initially

## 2. Configuration Plan

Use environment-specific appsettings files plus environment variables:

- `appsettings.json`
  Purpose: shared defaults
- `appsettings.Development.json`
  Purpose: local development with SQLite and demo-safe behavior
- `appsettings.Production.json`
  Purpose: PostgreSQL, stricter logging, seeding disabled

Production configuration priorities:

- Set `Database:Provider=PostgreSQL`
- Replace `Database:ConnectionString` with the real production connection string
- Keep `Seeding:SeedDefaultUsers=false`
- Keep authentication lockout enabled
- Store production secrets outside source-controlled files where possible using environment variables such as:
  - `CLINICADMIN_Database__ConnectionString`

## 3. Migration Strategy

Current state:

- The app currently uses `EnsureCreatedAsync()` in [ClinicAdminDbInitializer.cs](../../src/ClinicAdmin.Infrastructure/Persistence/ClinicAdminDbInitializer.cs)
- No EF Core migrations are checked in yet

Required production migration approach:

1. Create the initial EF Core migration from a machine with the .NET SDK installed
2. Store migrations under [Persistence/Migrations](../../src/ClinicAdmin.Infrastructure/Persistence/Migrations)
3. Validate the migration against PostgreSQL in a staging environment
4. Change production startup from schema creation to migration application
5. Run migrations before or during controlled deployment, not ad hoc during clinic hours

Recommended command flow once the SDK is available:

```powershell
dotnet ef migrations add InitialProductionSchema --project src/ClinicAdmin.Infrastructure --startup-project src/ClinicAdmin.Desktop
dotnet ef database update --project src/ClinicAdmin.Infrastructure --startup-project src/ClinicAdmin.Desktop
```

For the first clinic rollout, prefer one of these:

- `Preferred`: run migrations manually as part of deployment
- `Acceptable`: application startup applies migrations only in controlled IT-managed installs

## 4. Packaging Approach

Recommended first packaging path:

- Build a self-contained Windows x64 publish
- Package with MSIX, WiX, or Inno Setup depending on local IT constraints

Suggested order of preference:

1. `MSIX`
   Best if clinic desktops support modern Windows app deployment and signing
2. `WiX Toolset`
   Best if MSI deployment is required by IT
3. `Inno Setup`
   Practical for simple controlled single-clinic installations

Publish recommendation:

```powershell
dotnet publish src/ClinicAdmin.Desktop -c Release -r win-x64 --self-contained true
```

Installer should include:

- desktop app binaries
- default production config template
- log folder creation
- prerequisite check notes for PostgreSQL connectivity
- optional shortcut creation

## 5. Step-by-Step Deployment Guide

### Database host

1. Install PostgreSQL on the clinic server or approved host
2. Create database `clinicadmin`
3. Create a dedicated application login with least-privilege access
4. Restrict database access to clinic LAN ranges only
5. Confirm firewall rules allow only required client hosts

### Workstation preparation

1. Confirm supported Windows version and updates
2. Create or confirm a secure local install folder
3. Create a writable log folder with restricted ACLs
4. Place production configuration with the correct connection string
5. Confirm `DOTNET_ENVIRONMENT=Production` if using environment-based configuration

### Application deployment

1. Publish the release build
2. Install using the chosen installer package
3. Copy or override production configuration
4. Run database migration/update
5. Start the application
6. Create or provision initial admin user through controlled process
7. Verify seeded demo users are not enabled in production

## 6. Post-Deployment Verification Checklist

- application launches successfully
- login works with the intended production admin account
- demo/default users are not present
- patient search opens without errors
- new patient registration succeeds
- visit registration succeeds
- audit entries are written
- log files are created in the expected folder
- PostgreSQL connection is stable from at least two clinic workstations
- backup job is configured and documented

## 7. Rollback Considerations

Application rollback:

- keep the previous signed installer/package available
- keep previous configuration backups
- record current app version before upgrade

Database rollback:

- take a full backup before applying any schema migration
- do not rely on uninstall as a rollback strategy
- test migration rollback or restore in a staging environment first

If deployment fails after schema change:

1. stop client access
2. restore the last known-good database backup
3. reinstall or repoint to the last stable app version
4. re-verify login, registration, and audit logging

## 8. Backup Recommendations

- nightly PostgreSQL full backup
- more frequent transaction/WAL backup if clinic volume requires it
- retain daily backups for at least 14-30 days
- store backups off the primary machine
- encrypt backup archives
- test restore regularly

## 9. Common Risks And Mitigations

- Wrong connection string
  Mitigation: test with a production-like staging config first
- Demo seeding left on
  Mitigation: enforce `Seeding:SeedDefaultUsers=false` in production config
- No migration history
  Mitigation: create and validate initial EF migrations before rollout
- Weak log folder permissions
  Mitigation: restrict filesystem ACLs to approved local users/support
- Database server downtime
  Mitigation: document restart procedure and backup/restore steps
- Partial deployment across workstations
  Mitigation: version all clients together during the initial single-clinic rollout
