# Clinic Administration

WPF desktop application for South African public clinics focused on fast patient registration, duplicate prevention, rapid search, and physical file tracking.

## Core workflows

### Login

```mermaid
flowchart TD
    A["Launch app"] --> B["Login screen"]
    B --> C["Enter username and password"]
    C --> D{"Credentials valid?"}
    D -- No --> E["Show inline error and keep focus in password field"]
    E --> C
    D -- Yes --> F{"Role authorized?"}
    F -- No --> G["Show access denied message"]
    F -- Yes --> H["Open dashboard or last-used task screen"]
```

### New patient registration

```mermaid
flowchart TD
    A["Open Patient Search"] --> B["Search first"]
    B --> C{"Existing patient found?"}
    C -- Yes --> D["Open patient profile"]
    C -- No --> E["Select Register New Patient"]
    E --> F["Capture core demographics"]
    F --> G["Run duplicate detection"]
    G --> H{"Possible duplicate?"}
    H -- Yes --> I["Show duplicate review panel"]
    I --> J{"Use existing patient?"}
    J -- Yes --> D
    J -- No --> K["Authorized override or manual review"]
    H -- No --> L["Save patient"]
    K --> L
    L --> M["Create or link file"]
    M --> N["Capture new visit or return to search"]
```

### Existing patient search

```mermaid
flowchart TD
    A["Open Patient Search"] --> B["Enter ID, file no, phone, name, or DOB"]
    B --> C["View ranked results"]
    C --> D{"Correct patient found?"}
    D -- No --> E["Refine search or register new patient"]
    D -- Yes --> F["Open patient summary"]
    F --> G["Check file status and next action"]
```

### New visit capture

```mermaid
flowchart TD
    A["Open patient profile"] --> B["Select New Visit"]
    B --> C["Capture visit type, date/time, destination, reason"]
    C --> D{"Active visit already exists?"}
    D -- Yes --> E["Warn user and confirm next action"]
    D -- No --> F["Save visit"]
    E --> F
    F --> G["Update patient status or queue state"]
```

### Patient history retrieval

```mermaid
flowchart TD
    A["Search patient"] --> B["Open patient profile"]
    B --> C["Select Visits or History tab"]
    C --> D["View prior visits, file events, and admin updates"]
    D --> E["Filter by date or event type"]
    E --> F["Use details for follow-up or issue resolution"]
```

## Solution layout

- `src/ClinicAdmin.Desktop`: WPF presentation layer
- `src/ClinicAdmin.Application`: use cases, interfaces, validation, orchestration
- `src/ClinicAdmin.Domain`: core entities and business rules
- `src/ClinicAdmin.Infrastructure`: EF Core, persistence, logging, security, configuration
- `src/ClinicAdmin.Contracts`: DTOs and contracts shared across boundaries
- `tests/*`: unit and integration test projects
- `docs/*`: architecture and discovery documentation

## Notes

- Production database target: PostgreSQL
- Development/demo database target: SQLite
- Architecture guidance lives in `docs/architecture/architecture-overview.md`
- Initial internal folder plan lives in `docs/architecture/solution-structure.md`
