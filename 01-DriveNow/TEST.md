# DriveNow Test Self-Review

**Student Name:** Omar Fathy  
**Group/ID:** 3

## RentalMath — BR-01, BR-02, BR-03

### 1. Business Rules to Test Mapping

| Business Rule | Test Method |
|---|---|
| BR-01 — Calculate Age | `CalculateAge_BirthdayHasNotOccurred_ReturnsPreviousAge` |
| BR-01 — Calculate Age | `CalculateAge_BirthdayHasOccurred_ReturnsCorrectAge` |
| BR-01 — Calculate Age | `CalculateAge_InLeapYear_ReturnsCorrectAge` |
| BR-01 — Calculate Age | `CalculateAge_InNotLeapYear_ReturnsCorrectAge` |
| BR-02 — Calculate License Years | `CalculateLicenseYears_BoundaryDates_ReturnsCorrectYears` |
| BR-03 — Calculate Rental Days | `CalculateRentalDays_ReturnAtBeforeOrEqualPickup_ThrowsArgumentException` |
| BR-03 — Calculate Rental Days | `CalculateRentalDays_ValidDuration_ReturnsCorrectedDays` |

### 2. Boundaries Tested

**BR-01 — CalculateAge**

- Birthday has not occurred yet: August 11, 2001 → May 3, 2026.
- Birthday has already occurred: August 11, 2001 → August 12, 2026.
- Leap year: February 28 and February 29, 2024, for someone born on February 29, 2000.
- Non-leap year: February 27 and February 28, 2023, for someone born on February 29, 2000.

**BR-02 — CalculateLicenseYears**

- License issued one day before the anniversary: May 14, 2020 → May 15, 2024.
- License issued on the anniversary: May 15, 2020 → May 15, 2024.
- License issued one day after the anniversary: May 16, 2020 → May 15, 2024.

**BR-03 — CalculateRentalDays**

- Return before pickup.
- Return at exactly the pickup time.
- 24 hours.
- 24 hours and 59 minutes.
- 25 hours.
- 25 hours and 1 minute.
- 48 hours and 1 minute.

### 3. Test Data and Assertions

- Used xUnit `[Fact]`, `[Theory]`, and `[InlineData]`.
- Used `[Trait("Rule", "...")]` to associate tests with their business rules.
- Used FluentAssertions for result assertions and exception assertions.
- Used Arrange, Act, and Assert sections in the test methods.

### 4. My Self-Assessment

**My verdict:** Covered based on my understanding, I added tests for the main scenarios and the boundaries I identified.

## AI Review

The AI reviewed the seven existing test methods against BR-01, BR-02, and BR-03.

The review identified the following potential coverage gaps:

- **BR-01:** Birthday exactly on the evaluation date.
- **BR-02:** License issued on the evaluation date, and February 29 issue dates in non-leap years.
- **BR-03:** Rental durations shorter than 24 hours, exactly 60 leftover minutes, and multiple full days with leftover time greater than 60 minutes.

The AI also questioned whether the leap-year and non-leap-year age tests were redundant.

## AI Suggestions and Decisions

| AI Suggestion | Decision | Reason |
|---|---|---|
| Test February 29 birthday on February 27 in a non-leap year | Rejected | Already covered by `CalculateAge_InNonLeapYear_ReturnsCorrectAge`. |
| Test February 29 birthday on February 28 in a non-leap year | Rejected | Already covered by `CalculateAge_InNonLeapYear_ReturnsCorrectAge`. |
| Test birthday exactly on the evaluation date | Accepted | Adds coverage for the exact birthday boundary. |
| Test birthday one day away | Rejected | The existing `CalculateAge_BirthdayHasNotOccurred_ReturnsPreviousAge` already covers a birthday that has not occurred. |
| Test license issued on the evaluation date (0 years) | Accepted | Verifies the zero full-years boundary. |
| Test license anniversary exactly today | Rejected | Already covered by `CalculateLicenseYears_BoundaryDates_ReturnsCorrectYears`. |
| Test license anniversary tomorrow | Rejected | Already covered by `CalculateLicenseYears_BoundaryDates_ReturnsCorrectYears`. |
| Test February 29 license issue date in a non-leap year | Accepted | Adds coverage for the leap-day rule applied to license years. |
| Test rental duration shorter than 24 hours | Accepted | Verifies the minimum rental duration of one day. |
| Test exactly 24 hours | Rejected | Already covered by `CalculateRentalDays_ValidDuration_ReturnsCorrectedDays`. |
| Test 24 hours and 59 minutes | Rejected | Already covered by `CalculateRentalDays_ValidDuration_ReturnsCorrectedDays`. |
| Test exactly 60 leftover minutes | Accepted as a correction | The existing test has an incorrect expected result. It should expect 1 rental day, not 2. |
| Test 24 hours and 61 minutes | Rejected | Already covered by `CalculateRentalDays_ValidDuration_ReturnsCorrectedDays`. |
| Test multiple full days with leftover time greater than 60 minutes | Accepted | Adds coverage for the rounding rule across multiple rental days. |
| Remove one of the leap-year age tests as redundant | Rejected | Leap-year and non-leap-year scenarios verify different calendar behavior. |

## Evaluation of AI Review

The AI review was helpful in identifying missing boundary scenarios and highlighting the importance of testing leap-day behavior and rental-day rounding.

However, some suggestions were already covered by the existing tests. For example, February 29 birthdays on February 27 and February 28 in a non-leap year, the license anniversary boundaries, and several rental-duration boundaries were already included.

The AI also questioned whether the leap-year and non-leap-year age tests were redundant. I rejected this suggestion because they test different calendar conditions.

One important finding was the incorrect expected result for exactly 25 hours. According to BR-03, exactly 60 leftover minutes must not add another rental day, so the expected result should be 1.

I compared the AI suggestions with the existing tests and the README business rules, then selected only the suggestions that add meaningful coverage.



### R4 — Bogus

I created `CustomerFaker` and `VehicleFaker` to generate test data.

I used a fixed seed (`123`) so the generated data can be reproduced when a test fails. This makes debugging easier because the test uses the same generated data each time.

**Random data is suitable when:**
Testing a scenario where the exact customer name or email does not affect the expected result.

**Fixed data is necessary when:**
Testing age-based pricing or loyalty discounts, because the expected result depends on the customer's exact age or loyalty tier.

I also added a test using the Faker classes to generate customer and vehicle data.


## AI Self-Review — PricingService
### AI Suggestions and Decisions

| AI Suggestion | Decision | Reason |
|---|---|---|
| Test zero or negative rental days in BR-30 | Rejected | Rental duration validation belongs to BR-03, not PricingService. |
| Add more daily rates and rental durations | Rejected | Existing tests already cover different rates and durations. |
| Add Saturday-start rental coverage in BR-31 | Accepted | Saturday is a weekend day and needs explicit coverage. |
| Add age 21 and 24 boundary tests in BR-32 | Accepted | These are the exact boundaries of the young-driver age range. |
| Test birthday exactly on pickup date | Accepted | Verifies age calculation at the birthday boundary. |
| Add 6-day and 13-day tests in BR-33 | Accepted | These complete the required duration boundaries. |
| Add a Bronze or no-tier test in BR-34 | Rejected | Existing tests already cover None, Silver, Gold, and Platinum. |
| Test invalid or zero promo percentages | Rejected | These scenarios are not specified by the business rules. |
| Test all Promo validation failures in BR-36 | Accepted | The five main failure conditions are covered, and the additional boundary cases improve validation coverage. |
| Test case-insensitive Promo lookup | Accepted | BR-36 explicitly requires case-insensitive lookup. |
| Test inclusive Promo validity dates | Accepted | Both the start and end dates are inclusive. |
| Test null allowed categories | Accepted | A null category list means the Promo is valid for all categories. |
| Test multiple Promo failures together | Accepted | Verifies that the first failed validation condition determines the error. |
| Test discount combinations below, exactly at, and above 25% | Accepted | These cases verify the discount cap boundary. |
| Verify that only the Promo discount is reduced by the cap | Accepted | BR-37 requires duration and loyalty discounts to remain unchanged. |
| Test all insurance plans with Luxury vehicles | Rejected | Standard Luxury is already covered. Additional Luxury combinations were considered lower priority. |
| Test zero additional drivers | Rejected | Existing tests cover additional-driver pricing and free-driver behavior by loyalty tier. |
| Test Silver customers paying for every additional driver | Accepted | BR-39 gives the free first driver only to Gold and Platinum customers. |
| Add more full-pricing combinations | Rejected | The existing worked examples already combine several pricing components. |

### Evaluation of AI Review

The AI review identified useful boundary cases, especially for Promo validation and the 25% discount cap.

Some suggestions were already covered by existing tests, while others had lower priority compared with the main business-rule boundaries.

I selected the suggestions that improve important rule coverage and rejected the remaining suggestions to keep the test suite focused and avoid unnecessary duplication.
### Evaluation of AI Review

The AI review identified useful missing boundary cases, especially for age, rental duration, Promo validation, discount capping, and additional drivers.

Some suggestions were already covered by existing tests, while others were outside the specified business rules.

The review also helped identify interactions between pricing components that should be tested together, not only independently.

I checked the suggestions against the assignment requirements and business rules before deciding which ones to accept.



## AI Self-Review — BookingService (BR-10 to BR-27)

### AI Review Summary

The AI reviewed the existing BookingService test names against BR-10 to BR-27.

The review identified potential gaps in:

- Eligibility boundaries for Luxury and Van vehicles.
- Pickup time and rental duration boundaries.
- Customer active booking limits.
- Deposit combinations.
- Booking reference uniqueness.
- Booking properties and post-booking side effects.

The review also suggested testing customer booking overlaps. However, BR-22 specifies a limit on the number of open bookings for a customer, not customer booking overlaps.

### AI Suggestions and Decisions

| AI Suggestion | Decision | Reason |
|---|---|---|
| Test Luxury eligibility with age 26 and exactly 3 license years | Accepted | Verifies that a customer meeting both minimum requirements is allowed to rent a Luxury vehicle. |
| Test Luxury eligibility with age 26 and less than 3 license years | Rejected | The existing age-25 rejection test and the accepted valid case provide sufficient coverage for now. |
| Test Van eligibility with exactly 2 license years | Accepted | Verifies the exact minimum license-duration boundary. |
| Test Pickup exactly 2 hours from now | Accepted | Verifies the exact minimum pickup-time boundary. |
| Test Pickup 1 hour and 59 minutes from now | Rejected | The existing earlier-pickup rejection test already covers the failure scenario. |
| Test return exactly at pickup time | Rejected | Lower priority than verifying the maximum allowed rental duration. |
| Test rental duration of exactly 30 days | Accepted | Verifies that the maximum allowed rental duration is accepted. |
| Test rental duration of 30 days and 1 minute | Rejected | The existing test already covers a rental period exceeding 30 days. |
| Test one existing open booking allows another booking | Accepted | Verifies the BR-22 boundary below the maximum of two open bookings. |
| Add separate customer-overlap tests | Rejected | BR-22 is about the number of open bookings, not overlapping bookings for the same customer. |
| Add more vehicle-overlap tests | Rejected | Existing tests already cover vehicle overlap rejection and back-to-back bookings. |
| Test more PricingService combinations | Rejected | This would significantly expand the scope, while the current tests already cover a basic pricing scenario. |
| Create multiple bookings and verify unique references | Rejected | Reference uniqueness is not explicitly required by BR-25. |
| Split booking property assertions into separate tests | Rejected | The existing test already checks the required booking properties. |
| Test every Deposit combination | Rejected | Testing every vehicle category, insurance plan, and loyalty tier would add many cases without being necessary for the current review. |
| Explicitly verify the audit log message | Accepted | BR-27 requires the audit log to record `Booking {reference} created`. |
| Split every notification and side effect into separate tests | Rejected | The existing test already checks the main post-booking side effects together. |

### Evaluation of AI Review

The AI review was useful in identifying missing boundary cases, particularly for Luxury eligibility, Van license duration, pickup time, rental duration, and the customer booking limit.

However, some suggestions were unnecessary or based on an incorrect interpretation of the business rules.

For example, the AI suggested testing customer booking overlaps under BR-22. After checking the README, I confirmed that BR-22 only limits a customer to fewer than two open bookings. Vehicle overlap is handled separately by BR-21.

The AI also suggested testing reference uniqueness and every possible Deposit combination. I rejected these suggestions because they are not necessary for the current scope or would add too many tests.

I compared the suggestions against the README and existing tests, then selected only the cases that add meaningful coverage.

### Planned Improvements

The following test cases were selected for implementation:

1. BR-16: A customer aged 26 with exactly 3 license years can rent a Luxury vehicle.
2. BR-17: A customer with exactly 2 license years can rent a Van.
3. BR-18: A Pickup booking exactly 2 hours from now is accepted.
4. BR-19: A rental period of exactly 30 days is accepted.
5. BR-22: A customer with one existing open booking can create another booking.
6. BR-27: Verify that the audit log records `Booking {reference} created`.

These cases are planned improvements and have not yet been implemented.


# Task 7 — Pickups, Returns and Cancellations

## 1. Test Coverage

The following table maps the Task 7 business rules to the tests written for them.

| Business Rule | Test(s) | Coverage |
|---|---|---|
| BR-28 — Start Rental | `StartRental_PickupTimeBoundary_EnforcesOneHourRule` | Covers the one-hour pickup boundary: 61 minutes before, exactly 60 minutes before, and 59 minutes before pickup. |
| BR-41 — Late Return Fee | `ProcessReturn_LateByBoundaryMinutes_CalculatesLateFee`, `ProcessReturn_EarlyReturn_DoesNotChargeLateFee` | Covers early returns and late-fee calculations around the hourly and daily thresholds. |
| BR-42–BR-46 — Return Processing | `ProcessReturn_ExtraMileage_UsesCategoryRate`, `ProcessReturn_MissingFuel_UsesCategoryRate`, `ProcessReturn_DamageCost_IsCappedByInsuranceDeductible`, `ProcessReturn_EarlyReturn_DoesNotRefundUnusedRentalDays`, `ProcessReturn_ValidReturn_UpdatesEntitiesAndSendsReceipt`, `ProcessReturn_BookingDoesNotExist_ThrowsBookingNotFoundException`, `ProcessReturn_NonActiveBooking_ThrowsInvalidBookingStateException`, `ProcessReturn_InvalidMileageOrFuel_ThrowsArgumentException` | Covers the main return calculations, validation, booking state, entity updates, and receipt notification. |
| BR-50–BR-53 — Cancellation | `Cancel_TimeBeforePickupBoundary_CalculatesCancellationFee`, `Cancel_PlatinumCustomer_DoesNotPayCancellationFee`, `Cancel_ReservedBooking_UpdatesStatusPromoAndSendsNotifications`, `Cancel_BookingDoesNotExist_ThrowsBookingNotFoundException`, `Cancel_BookingIsNotReserved_ThrowsInvalidBookingStateException`, `Cancel_PickupTimeReached_ThrowsInvalidBookingStateException`, `Cancel_DepositComparedToFee_CalculatesRefundAndAdditionalCharge` | Covers cancellation fee boundaries, Platinum exemption, validation, booking updates, notifications, and deposit calculations. |

## 2. AI Review

The AI reviewed the test names against the Task 7 business rules and identified possible gaps in pickup boundaries, late-return fees, return calculations, and cancellation rules.

It also suggested additional cases for booking states, insurance deductibles, category rates, and cancellation side effects.

## 3. AI Suggestions and Decisions

| AI Suggestion | Decision | Reason |
|---|---|---|
| Test starting a non-Reserved booking | Accepted | Verifies that a booking must have the correct status before starting a rental. |
| Add more BR-28 time boundary tests | Rejected | The existing Theory test already covers 61 minutes before, exactly one hour before, and 59 minutes before pickup. |
| Add more BR-41 boundary values | Rejected | The existing test already covers the important hourly and daily fee thresholds, including 240 and 241 minutes. |
| Test 480 and 481 minutes | Rejected | These values do not add enough coverage to justify another test at this stage. |
| Add more return category and insurance combinations | Rejected | The current tests cover the main category-rate calculations and the insurance deductible cap. |
| Test more cancellation time boundaries | Rejected | The existing test already covers the cancellation fee thresholds. |
| Add more deposit comparison tests | Rejected | The existing test covers the refund, exact-match, and additional-charge scenarios. |
| Add separate audit and notification tests | Rejected | Existing tests already verify the main cancellation and return side effects. |
| Test every loyalty tier | Rejected | The current tests cover the Platinum exemption. Additional tiers were not considered necessary for this review. |

## 4. Evaluation of AI Review

The AI review was useful because it highlighted areas that should be checked, especially booking status validation and return-cost calculations.

However, some suggestions were already covered by existing tests. For example, the AI recommended adding more BR-28 and BR-41 boundary tests, even though the current tests already cover those boundaries.

I checked each suggestion against the existing test names and test data instead of accepting all recommendations. I accepted only the non-Reserved booking scenario because it adds a meaningful check without significantly increasing the test suite.

This review showed that AI suggestions should be verified against the actual tests and business rules before making changes.