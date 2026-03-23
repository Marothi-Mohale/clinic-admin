# Clinic Administration Testing Strategy

## 1. Unit Testing Strategy

- Focus unit tests on domain rules, validators, duplicate detection, authorization rules, reporting aggregation logic, and view model behavior.
- Keep unit tests fast, deterministic, and isolated from file system, database, and clock dependencies where possible.
- Prioritize:
  - patient registration validation
  - duplicate prevention rules
  - authentication request validation
  - visit workflow state changes
  - report view model filter and export behavior
- Use fake clocks, fake facility context, fake session services, and in-memory collections for pure unit tests.

## 2. Integration Testing Strategy

- Cover high-risk workflows across multiple layers using EF Core with SQLite in-memory where possible.
- Prefer integration tests for:
  - login + audit persistence
  - patient registration + duplicate checks + audit
  - visit registration + audit + reporting
  - audit query filters
  - reporting aggregation against realistic persisted data
- Keep integration tests small but realistic: seed only the data required for the scenario.

## 3. UI Testing Recommendations

- Use automated UI testing sparingly on the most critical workflows:
  - login
  - patient search-before-create
  - new patient registration
  - new visit capture
  - report loading and export
- Recommended approach:
  - keep most UI logic in view models and test it there
  - add smoke UI tests with a Windows UI automation tool later for shell navigation and key workflows
  - run manual exploratory testing for focus order, keyboard behavior, validation feedback, and high-volume receptionist flows

## 4. Critical Business Scenarios

- Receptionist logs in and searches for an existing patient in under a few steps.
- Receptionist registers a new patient with valid details and receives success confirmation.
- Duplicate patient with exact national ID is blocked from registration.
- Duplicate patient with soft match requires review/confirmation.
- Receptionist captures a new visit for a patient and the visit appears in history.
- Manager loads operational reports and sees registration and visit counts for the selected period.
- Admin reviews audit entries for logins, registrations, and visit updates.

## 5. Negative Test Cases

- blank username or password
- inactive user login attempt
- wrong password
- patient registration with missing required fields
- patient registration with duplicate patient number
- visit registration for unknown patient
- second active visit for same patient
- reporting with invalid date range
- export when no report data is loaded

## 6. Duplicate Detection Test Cases

- exact national ID match blocks creation
- exact passport match blocks creation
- phone number normalized from `+27` to `0` format matches correctly
- exact full name only shows warning
- surname + DOB + similar first name requires manual review
- shared surname only does not trigger duplicate action
- candidate in another facility is ignored
- registration handler refuses blocked duplicate and does not persist patient

## 7. Authentication / Authorization Test Cases

- valid login creates session and audit entry
- invalid login creates failed audit entry
- validation failure on login is audited
- logout clears session and records audit event
- Admin can access reports and audit
- Receptionist cannot access administration
- Manager can access reports and audit but not administration

## 8. Performance Test Scenarios

- patient search with realistic patient volumes
- registration save under busy-session conditions
- duplicate detection against large candidate sets
- report generation over 7-day and 30-day windows
- audit log query with filters and date ranges
- repeated visit capture during peak simulated front-desk load

## 9. Reliability And Recovery Tests

- application restart after failed save
- restart after abrupt shutdown with existing SQLite data
- audit persistence after exception in surrounding workflow
- log file creation and rollover
- export failure due to invalid or locked destination
- database unavailable or corrupted startup path shows graceful error

## 10. Test Data Strategy

- Use deterministic seed patterns:
  - unique facility IDs
  - realistic South African-style patient numbers, IDs, and mobile numbers
  - common surnames and similar-name duplicates for fuzzy matching
- Maintain separate seed groups for:
  - authentication
  - duplicate detection
  - registration/search
  - visits/history
  - reporting/audit
- Include both clean and messy data:
  - missing DOB
  - passport-only patients
  - shared phone numbers
  - common surnames
  - inactive users
- Keep test data small for unit tests and scenario-shaped for integration tests.

## Highest-Value Tests First

The first automation priority should be:

1. authentication success/failure and audit
2. duplicate prevention at registration
3. visit workflow duplicate-active-visit protection
4. audit persistence and querying
5. reporting aggregation from persisted registrations and visits
6. one end-to-end integration flow across login, registration, visit, audit, and reporting
