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

