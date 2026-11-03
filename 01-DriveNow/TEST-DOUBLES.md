# Test Doubles Review

**Student Name:** Omar Fathy  
**Group/ID:** 3

## R1 — Hand-Written Test Doubles

The following test doubles were implemented manually without using a mocking library.

| Test Double | Kind | Test That Uses It | Why This Kind? |
|---|---|---|---|
| `StubCustomerRepository` | Stub | `CreateBooking_CustomerDoesNotExist_ThrowsCustomerNotFoundException` | Returns controlled customer data so the test can simulate a missing customer. |
| `StubBookingRepository` | Fake | Booking overlap and booking-related tests | Provides a working in-memory repository with simplified storage and query behavior. |
| `SpyPromoCodeRepository` | Spy | `CreateBooking` promo-code test in BR-27 | Records promo-code updates so the test can verify that the expected update happened. |
| `HandWrittenMockPaymentGateway` | Mock | `CreateBooking_WithDeposit_ChargesExpectedAmount` | Checks the expected customer ID, amount, and description, then verifies that the payment call happened correctly. |
| `DummyReferenceGenerator` | Dummy / Guard | `CreateBooking_CustomerDoesNotExist_DoesNotGenerateReference` | It is supplied as a dependency that should not be used. It throws if the service unexpectedly calls it. |

## R2 — Moq

Two tests were rewritten using Moq:

- `CreateBooking_CustomerDoesNotExist_WithMoq`
- `CreateBooking_VehicleDoesNotExist_WithMoq`

The tests use:

- `Setup` to define the dependency's behavior.
- `Returns` to provide the required result.
- `Verify` with `Times.Once` to check that the expected method was called.

The Platinum deposit test also uses `Times.Never` to verify that no payment charge is made when the deposit is zero.

### Hand-Written Doubles vs. Moq

Hand-written doubles make the behavior explicit and allow custom verification logic. However, they require extra classes and code.

Moq makes tests shorter because setup and verification are written directly inside the test. On the other hand, tests can become harder to read when setups contain many expressions, and they may become fragile when they verify implementation details that are not important to the business rule.

## R6 — AutoFixture vs. Bogus

AutoFixture and Bogus can both help create test data, but they serve different purposes.

| Tool | Main Purpose | When to Use It |
|---|---|---|
| AutoFixture | Automatically creates objects and fills their properties with generated values. | Useful when a test needs an object but does not depend on most of its property values. |
| Bogus | Generates realistic fake data using rules such as names, emails, and dates. | Useful when tests need realistic-looking data or controlled data-generation rules. |

**Examples from this test suite:**

- **AutoFixture:** `ValidCustomerCustomization_CreatesCustomerThatMeetsBusinessRules` uses `ValidCustomerCustomization` to create a Customer that meets the business rules. `CreateBooking_CustomerAgeBoundary_EnforcesMinimumAge` also uses `InlineAutoData` to provide age values while generating the remaining request data anonymously.
- **Bogus:** No test currently uses Bogus. It can be considered if the suite later needs realistic customer data.

**What I learned:** Automatically generated values are not always suitable for a test. For example, the anonymous `BookingRequest` initially contained an invalid promo code and an excessive number of additional drivers. I had to override those properties so the test could focus on the age boundary.

## R7 — Snapshot Testing with Verify

I used Verify.Xunit to create a snapshot of a Booking returned by BookingService.

The first test run generated a `.received.txt` file. I reviewed the output and accepted it as the `.verified.txt` snapshot.

The first problem with snapshots is that they can contain unstable values, such as the current date and time or randomly generated references. These values can cause snapshot tests to fail even when the actual behavior has not changed.

To avoid this, I used:
- A fixed TimeProvider to control the current date and time.
- A StubReferenceGenerator that returns a fixed reference.

The snapshot now produces consistent output across test runs.