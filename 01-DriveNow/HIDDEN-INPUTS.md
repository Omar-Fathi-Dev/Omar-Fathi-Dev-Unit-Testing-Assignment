# Hidden Inputs Review

**Student Name:** Omar Fathy  
**Group/ID:** 3

## Overview[TEST-DOUBLES.md](TEST-DOUBLES.md)

This document lists the hidden inputs found in the source code and explains how they were made controllable for testing.

A hidden input is a dependency that a method uses without receiving it explicitly. Examples include the current date, random values, delays, and file system access.

## Hidden Inputs

| Class | Line | Hidden Input | Why It Hurts a Test | How It Was Removed |
|---|---:|---|---|---|
| `InMemoryDatabase` | Constructor | Current date and time | Tests may depend on the actual system date, making results difficult to control and repeat. | Injected `TimeProvider` and used it to obtain the current date. |
| `BookingService` | Constructor | Current date and time | Business rules depending on the current date cannot be tested with a controlled date. | Injected `TimeProvider`. |
| `CancellationService` | Constructor | Current date and time | Tests cannot control date-dependent cancellation behavior. | Injected `TimeProvider`. |
| `PricingService` | Constructor | Current date and time | Date-dependent pricing rules may produce different results on different days. | Injected `TimeProvider`. |
| `ReturnService` | Constructor | Current date and time | Tests cannot control the current time when checking return behavior. | Injected `TimeProvider`. |
| `EmailSender` | Constructor | File system and current date/time | Tests may write real files and depend on the system clock. | Injected `IFileSystem` and `TimeProvider`. |
| `AuditLog` | Constructor | File system and current date/time | Tests may modify real log files and depend on the system clock. | Converted it from a static class to an instance class and injected `IFileSystem` and `TimeProvider`. |
| `PaymentGateway` | Constructor | Delay and random values | Tests may wait for real delays and cannot reliably control payment failures. | Injected `IDelayProvider` and `IRandomProvider`. |
| `SmsSender` | Constructor | Delay | Tests may wait for real delays, making the test suite slower. | Injected `IDelayProvider`. |
| `RandomProvider` | 7 | Random number generator | Direct random generation makes test results unpredictable. | Wrapped `Random` behind `IRandomProvider`, which can be replaced in tests. |
| `RandomReferenceGenerator` | Constructor | Random values | Generated references are unpredictable during tests. | Injected `IRandomProvider`. |
| `PhysicalDelayProvider` | 9 | `Thread.Sleep` | Real waiting makes tests slow. | Moved the delay operation behind `IDelayProvider`, allowing tests to use a no-op implementation. |
| `PhysicalFileSystem` | 7, 16, 21, 26 | `AppContext.BaseDirectory`, `Directory`, and `File` | Direct file system access may create or modify real files during tests. | Moved file operations behind `IFileSystem`. |

## Notes

- `Program.cs` is the Composition Root. It creates the real implementations and injects them into the application.
- `ConsoleWriter` is an Infrastructure implementation of `IConsoleWriter`. Console output is part of the application interface, not a business rule.
- `Program.cs` uses `DateTime.Today` in `ListCustomers()` to display customer ages. This is presentation-only logic, not a business calculation.
- `RentalMath` and `LoyaltyService` remain static because they perform calculations using explicit inputs and do not depend on hidden state.
- The real implementations still use system resources when the application runs. Tests can replace their interfaces with controlled test doubles.

## Conclusion

The application now receives its external dependencies explicitly. This makes the business logic easier to test without relying on the real clock, random values, delays, or file system.