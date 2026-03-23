# EF Core Migrations

Generated Entity Framework Core migrations should be stored here once the .NET SDK is available in the environment.

Production note:

- The current application is still using `EnsureCreatedAsync()` for schema initialization.
- Before production deployment, create the initial migration set and switch the deployment process to use real EF Core migrations.
- See `docs/deployment/single-clinic-deployment.md` for the recommended rollout and migration approach.
