# Architecture Tests

**Student Name:** Omar Fathy  
**Group/ID:** 3

## R1 – Domain Dependencies

**Rule:** `DriveNow.Domain` must not depend on any other DriveNow project.

**Result:** PASSED

The Domain project has no dependencies on Application, Infrastructure, or ConsoleApp.

## R2 – Application Dependencies

**Rule:** `DriveNow.Application` must depend only on `DriveNow.Domain`, not on Infrastructure or ConsoleApp.

**Result:** FAILED

The following classes depend on Infrastructure:

| Class | Infrastructure dependencies |
|---|---|
| `BookingService` | CustomerRepo, VehicleRepository, BookingRepository, PromoCodeRepository, PaymentGateway, SmsSender, EmailSender, AuditLog |
| `PricingService` | PromoCodeRepository |
| `ReturnService` | BookingRepository, VehicleRepository, CustomerRepo, PaymentGateway, EmailSender, AuditLog |
| `CancellationService` | BookingRepository, VehicleRepository, CustomerRepo, PromoCodeRepository, PaymentGateway, SmsSender, EmailSender, AuditLog |

**Why it matters for testing:**

Application services depend directly on Infrastructure implementations. This makes unit testing harder because tests may need to deal with external dependencies instead of testing business logic in isolation.

## R3 – Infrastructure Dependencies

**Rule:** `DriveNow.Infrastructure` must not depend on `DriveNow.ConsoleApp`.

**Result:** PASSED

No dependency on ConsoleApp was detected.

## R4 – Production Naming Rules

### Interfaces

**Rule:** Interface names must start with `I`.

**Result:** PASSED

No interface naming violations were detected in the Domain, Application, and Infrastructure assemblies.

### Repositories

**Rule:** Repository classes must end with `Repository` and live in a namespace ending with `Repositories`.

**Results:**

- Repository classes ending with `Repository` are in a namespace ending with `Repositories`: PASSED.
- Classes in a namespace ending with `Repositories` must end with `Repository`: FAILED.

**Offending type:**

- `CustomerRepo`

**Why it matters for testing:**

Consistent naming makes repository classes easier to identify and understand when creating test dependencies.

### Services

**Rule:** Service classes must end with `Service` and live in a namespace ending with `Services`.

**Results:**

- Classes ending with `Service` must live in a namespace ending with `Services`: FAILED.
    - `DriveNow.Application.Receipts.ReceiptService`
    - `DriveNow.Application.Bookings.CancellationService`

- Classes in a namespace ending with `Services` must end with `Service`: FAILED.
    - `DriveNow.Domain.Services.RentalMath`

**Why it matters for testing:**

Inconsistent naming makes it harder to identify service classes and understand their responsibilities.

### Exceptions

**Rule:** Exception classes must end with `Exception` and live in `DriveNow.Domain.Exceptions`.

**Result:** PASSED

No exception naming violations were detected.

## R5 – Test Naming Rules

### Test Classes

**Rule:** Test class names must end with `Tests`.

**Result:** PASSED

All test classes containing `[Fact]` or `[Theory]` methods follow this naming rule.

### Test Methods

**Rule:** Test methods must follow this pattern:

`Method_Scenario_ExpectedResult`

**Result:** PASSED

All test methods in `DriveNow.UnitTests` follow the selected naming pattern.

## Summary

The architecture tests detected violations in Application dependencies, repository naming, and service naming.

These tests will be used again after the production code changes in Task 4 to verify that the architecture rules are satisfied.