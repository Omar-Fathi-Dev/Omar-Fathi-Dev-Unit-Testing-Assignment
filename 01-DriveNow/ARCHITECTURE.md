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

---

# Task 4 – Architecture Changes

## 1. Application Dependencies

**Rule:** `DriveNow.Application` must not depend on Infrastructure or ConsoleApp.

**Result:** PASSED

Application services now depend on interfaces instead of Infrastructure implementations.

The following services were updated:

- `BookingService`
- `PricingService`
- `ReturnService`
- `CancellationService`

Repository and external service dependencies are now injected through constructors using interfaces.

The interfaces are located in:

`DriveNow.Application.Abstractions`

Infrastructure implements these interfaces, so the Application project no longer needs to reference Infrastructure.

## 2. Interface Placement

The external dependency interfaces were moved to `DriveNow.Application.Abstractions`.

This location was selected because:

- Application services need these abstractions.
- Infrastructure provides their concrete implementations.
- Application must not depend on Infrastructure.
- The namespace does not end with `Services`, which avoids conflicts with the architecture naming rule.

The interfaces include:

- `ICustomerRepository`
- `IVehicleRepository`
- `IBookingRepository`
- `IPromoCodeRepository`
- `IPaymentGateway`
- `ISmsSender`
- `IEmailSender`
- `IAuditLog`
- `IReferenceGenerator`
- `IFileSystem`
- `IDelayProvider`
- `IRandomProvider`
- `IConsoleWriter`

## 3. Repository Naming

**Result:** PASSED

The repository naming violations were corrected.

`CustomerRepo` was renamed to `CustomerRepository`.

All repository classes now end with `Repository` and live in a namespace ending with `Repositories`.

## 4. Service Naming

**Result:** PASSED

The service namespace violations were corrected.

- `ReceiptService` was moved to `DriveNow.Application.Services`.
- `CancellationService` was moved to `DriveNow.Application.Services`.
- `RentalMath` remains in `DriveNow.Domain.Services` because it is a pure static calculation class, not a service class. It was moved to `DriveNow.Domain.Calculations`.

## 5. Shared In-Memory Database

`InMemoryDatabase` was changed from a static class to an instance class.

All repositories receive the same `InMemoryDatabase` instance through constructor injection:

- `CustomerRepository`
- `VehicleRepository`
- `BookingRepository`
- `PromoCodeRepository`

This ensures that all repositories work with the same data instead of creating separate database instances.

## 6. External Dependencies

External operations were moved behind interfaces so that tests can replace real implementations with test doubles.

Examples:

| Interface | Infrastructure implementation |
|---|---|
| `IFileSystem` | `PhysicalFileSystem` |
| `IDelayProvider` | `PhysicalDelayProvider` |
| `IRandomProvider` | `RandomProvider` |
| `IReferenceGenerator` | `RandomReferenceGenerator` |
| `IPaymentGateway` | `PaymentGateway` |
| `ISmsSender` | `SmsSender` |
| `IEmailSender` | `EmailSender` |
| `IAuditLog` | `AuditLog` |
| `IConsoleWriter` | `ConsoleWriter` |

`TimeProvider` is injected into services that need the current date or time.

This allows tests to control time, random values, delays, and file system operations without relying on real external resources.

## 7. Composition Root

The ConsoleApp is responsible for creating and connecting the real implementations.

All dependencies are wired manually in `Program.cs` using constructor injection. No DI container is used.

The same `InMemoryDatabase` instance is shared by all repositories.

The same `ReceiptService` instance is also shared between `ReturnService` and the ConsoleApp.

## 8. Final Architecture Test Result

**Result:** PASSED – 12/12 tests

After the changes, all architecture tests passed.

The Application project no longer depends on Infrastructure, repository and service naming rules are satisfied, and the project dependency boundaries are respected.

## 9. Build and Console Verification

**Build:** PASSED

The solution built successfully in Release configuration with no errors.

**Console:** Basic operations verified successfully.

The following operations were tested:

- List vehicles.
- List customers.
- List bookings.
- Exit the application.

The original seeded data appeared correctly.

Further verification of operations that use external services is still required.

## Conclusion

Task 4 removed the hidden dependencies identified in the source code and corrected the architecture violations from Task 3.

The Application layer now depends on abstractions, Infrastructure provides the real implementations, and ConsoleApp connects everything through manual constructor injection.