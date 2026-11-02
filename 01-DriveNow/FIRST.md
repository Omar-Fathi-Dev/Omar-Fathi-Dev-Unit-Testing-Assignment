# BookingService — FIRST Audit

**Student Name:** Omar Fathy  
**Group/ID:** 3

## R1 — Test Runs

Command:

```bash
dotnet test DriveNow.sln
```

| Run | Test Time | Build Time | Passed | Failed |
|---|---:|---:|---:|---:|
| 1 | 41.5s | 61.3s | 31 | 0 |
| 2 | 7.3s | 16.3s | 31 | 0 |
| 3 | 5.5s | 12.1s | 31 | 0 |
| 4 | 8.5s | 19.0s | 31 | 0 |
| 5 | 5.3s | 11.9s | 31 | 0 |

All five runs passed. No tests failed.

## R2 — FIRST Analysis

### Fast

The first run took 41.5 seconds. The other runs took between 5.3 and 8.5 seconds.

The first run was slower because it also restored and built the projects.

### Independent

The tests use the same static data in `InMemoryDatabase`.

One test can change this data and affect another test.

However, all five runs passed.

### Repeatable

All five runs passed with no failures.

But the code uses `DateTime.Now` and `Random`. This may cause different results at different times.

### Self-validating

The tests use FluentAssertions to check the results.

Each test can pass or fail automatically.

### Timely

I wrote these tests after the production code was already written.

This task is about testing existing code, not writing the tests first.


## R3 — Offending Production Code Lines

| FIRST Violation | File | Line | Offending Code | Root Problem |
|---|---|---:|---|---|
| Fast | `PaymentGateway.cs` | 27 | `Thread.Sleep(1500);` | Blocks the test thread for 1.5 seconds. |
| Fast | `SmsSender.cs` | 7 | `Thread.Sleep(500);` | Blocks the test thread for 0.5 seconds. |
| Independent | `InMemoryDatabase.cs` | 9 | `public static List<Booking> Bookings = [];` | Tests share the same static booking data. |
| Independent | `BookingService.cs` | 64 | `_bookings.Add(booking);` | Creates a booking in the shared data. |
| Repeatable | `PaymentGateway.cs` | 28 | `if (new Random().Next(10) == 0) throw new PaymentFailedException("Gateway timeout");` | Can fail randomly with a 10% chance. |
| Repeatable | `BookingService.cs` | 48 | `Reference = "DN-" + ... + RandomCode()` | Generates a different booking reference. |
| Repeatable | `BookingService.cs` | 47 | `CreatedAt = DateTime.Now` | The creation time changes between test runs. |
| Timely | `BookingService.cs` | 47, 104 | `DateTime.Now` | Uses the machine clock directly, making time difficult to control in tests. |

`AuditLog.cs` line 9 also writes to a real file using `File.AppendAllText`. This is an external side effect that can leave files behind during tests.

**Self-validating:** The tests use FluentAssertions, so they can report pass or fail automatically. The audit log is a side-effect problem, not a direct Self-validating violation.

