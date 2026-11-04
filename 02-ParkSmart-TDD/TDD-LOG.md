# ParkSmart TDD Log

**Student Name:** Omar Fathy  
**Group/ID:** 3  
**Branch:** `assignment/1-9`

## TDD Workflow

Each stage follows the Red → Green → Refactor cycle:

- **Red:** Write a test for a new behavior and confirm that it fails for the expected reason.
- **Green:** Write the minimum production code needed to make the test pass.
- **Refactor:** Improve the code structure without changing its behavior, then run the tests again.

## TDD Progress Log

The following table records the commits and test methods found in the repository.

**Note about Red failure lines:** GitHub preserves the test source and commit history, but it does not preserve the terminal output generated when the Red tests were executed. Therefore, the exact runtime failure messages must be added from the original test output. They are intentionally not fabricated here.

| Commit | Test Name | Why It Failed (Real Failure Line) | What You Wrote | Refactor |
|---|---|---|---|---|
| **S1** Red: `6fc7312`<br>Green: `23ff28f`<br>Refactor: `1540134` | `Calculate_ParkingDurationOfFifteenMinutesOrLess_ReturnsZero`<br>`Calculate_ParkingDurationOfFifteenMinutesAndOneSecond_ReturnsPositiveFee` | Exact Red output not preserved in GitHub. The new tests checked the free-parking boundary and the first second after it. | Implemented the 15-minute free parking check. | Extracted `FreeParkingDurationInMinutes` as a constant. |
| **S2** Red: `451bc26`<br>Green: `e7d75ef`<br>Refactor: `31034cb` | `Calculate_ParkingDuration_ReturnsFeePerStartedHour` | Exact Red output not preserved in GitHub. The tests checked started-hour billing at 16, 60, 61, and 180 minutes. | Implemented started-hour billing using `Math.Ceiling`. | Extracted the hourly parking rate into a constant. |
| **S3** Red: `8008dc6`<br>Green: `c31039b`<br>Refactor: N/A | `Calculate_ExitTimeEqualsEntryTime_ReturnsZero`<br>`Calculate_ExitTimeBeforeEntryTime_ThrowsArgumentException` | Exact Red output not preserved in GitHub. The important new behavior was rejecting an exit time earlier than the entry time with the required exception message. | Added exit-time validation using `ArgumentException`. | No separate refactor commit was made. |
| **S4** Red: `b21be24`<br>Green: `8381ac5`<br>Refactor: `b3a7033` | `Calculate_ParkingAcrossDailyBlocks_AppliesDailyCap` | Exact Red output not preserved in GitHub. The tests exposed the need to apply the daily cap independently to each 24-hour block. | Implemented fee calculation across separate 24-hour blocks and applied the daily cap to each block. | Extracted daily-block fee calculation into a helper method. |
| **S5** Red: `4e92ffb`<br>Green: `9e00da2`<br>Refactor: `9b239e7` | `Calculate_VehicleType_ReturnsCorrectFee` | Exact Red output not preserved in GitHub. The tests checked whether motorcycles and trucks received their own rates and caps. | Implemented vehicle-specific hourly rates and daily caps. | Extracted vehicle pricing configuration into a helper method. |
| **S6** Red: `b134566`<br>Green: `1da9547`<br>Refactor: `531499e` | `Calculate_NightParking_ReturnsLowerOfNormalAndFlatRate` | Exact Red output not preserved in GitHub. The tests covered the night-rate boundaries, vehicle types, and parking just outside the allowed period. | Implemented vehicle-specific night flat rates and applied the lower of the normal fee and night rate. | Extracted the night-parking condition into `IsNightParking`. |
| **S7** Red: `2f24840`<br>Green: `86d5395`<br>Refactor: `739a0a6` | `Calculate_LostTicket_AppliesMinimumFee` | Exact Red output not preserved in GitHub. The tests checked lost-ticket minimum fees for different vehicle types and parking durations. | Implemented vehicle-specific lost-ticket minimum fees and applied the minimum after the other parking calculations. | Consolidated vehicle pricing information into one configuration method. |
| **S8** Red: `ec04fa0`<br>Green: `8d6062f`<br>Refactor: `8f20f6a` | `Calculate_FridayParking_AppliesTwoHourFreePeriod`<br>`Calculate_ThursdayParking_DoesNotApplyFridayFreePeriod` | Exact Red output not preserved in GitHub. The tests checked the two-hour Friday boundary and ensured Thursday retained the normal free period. | Implemented the Friday-specific two-hour free period. | Extracted free-period selection into `GetFreeParkingDuration`. |
| **S9** Red: `90814a8`<br>Green: `3738a8e`<br>Refactor: `8b83613` | `Calculate_SubscriberParkingWithinFreeWindow_ReturnsZero`<br>`Calculate_SubscriberParkingOutsideFreeWindow_AppliesFiftyPercentDiscount`<br>`Calculate_SubscriberParkingOnSaturday_AppliesFiftyPercentDiscount`<br>`Calculate_SubscriberWithLostTicket_AppliesMinimumAfterDiscount` | Exact Red output not preserved in GitHub. The tests covered subscriber free parking, the 50% discount, Saturday pricing, and the order of applying the lost-ticket minimum. | Implemented subscriber free parking and the 50% discount. Applied the lost-ticket minimum after subscriber pricing. | Extracted subscriber pricing into `ApplySubscriberPricing`. |
| **S10** Red: `2367793`<br>Green: `51754e4`<br>Refactor: `b049474` | `Calculate_WithEvCharging_AddsChargingFeeAfterParkingPricing`<br>`Calculate_WithNegativeChargingKwh_ThrowsArgumentException` | Exact Red output not preserved in GitHub. The tests checked charging fees after parking pricing, charging during a free subscriber session, and rejection of negative kWh. | Added EV charging fees after parking calculations and validated negative charging kWh. | Extracted the charging calculation into `CalculateChargingFee` and the rate into `ChargingRatePerKwh`. |



## Final Reflection

### 1. Which stage forced the biggest change to earlier code?

**S10 — EV Charging** caused the biggest change. Free parking could no longer return zero immediately because charging fees still had to be added.

I updated the calculation flow and extracted `CalculateChargingFee`.

### 2. What did this teach you about designing up front?

I learned that new requirements can change earlier code. TDD helped me handle these changes step by step.

I also learned that the order of business rules matters, especially when applying discounts, minimum fees, and extra charges.