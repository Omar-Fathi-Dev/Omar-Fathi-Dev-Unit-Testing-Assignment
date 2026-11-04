# BUGS.md

| # | R# | Rule | Requirement | Failing Test | Expected | Actual | Fix Commit |
|---|---|---|---|---|---|---|---|
| 1 | — | BR-41 | A late return of exactly 240 minutes must be charged at 10% of the daily rate per started hour, not the full daily rate. | `ProcessReturn_LateByBoundaryMinutes_CalculatesLateFee` | 400 EGP | 1,000 EGP | `fix: correct BR-41 late fee boundary at 240 minutes` |
| 2 | — | BR-03 | Exactly 60 leftover minutes after a full rental day must not add another rental day. | `CalculateRentalDays_ExactlySixtyMinutesOverOneDay_ReturnsOneDay` | 1 day | 2 days | `fix: correct BR-03 rental duration boundary` |
| 3 | — | BR-33 | Rentals lasting exactly 7 days must receive the 10% duration discount. | `Calculate_Duration_ReturnsExpectedDiscount` | 10% discount | 0% discount | `fix: correct BR-33 seven-day duration discount boundary` |
| 4 | — | BR-39 | The first additional driver is free for Gold and Platinum members, but not Silver members. | `Calculate_AdditionalDrivers_ReturnsExpectedCost` | 200 EGP for a Silver member with 1 additional driver | 0 EGP | `fix: correct BR-39 additional driver loyalty discount` |
| 5 | — | BR-21 | A new booking may start exactly when the previous booking ends. | `CreateBooking_StartsWhenPreviousBookingEnds_AllowsBooking` | Booking is allowed | Booking is rejected due to an overlap check | `fix: correct BR-21 back-to-back booking overlap check` |
| 6 | — | BR-51 | Cancellation exactly 48 hours before pickup must have no cancellation fee. | `Cancel_TimeBeforePickupBoundary_CalculatesCancellationFee` | 0 EGP | 500 EGP | `fix: correct BR-51 cancellation fee 48-hour boundary` |

## Notes

- Each bug was identified through a test against the relevant business rule.
- The test was committed before the corresponding fix.
- Each fix was made separately to keep the bug history clear.
