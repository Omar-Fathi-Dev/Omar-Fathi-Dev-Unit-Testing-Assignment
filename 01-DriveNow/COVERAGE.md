# Task 9 — Code Coverage

## 1. Coverage Configuration

Coverage was measured using Coverlet and ReportGenerator.

- `DriveNow.ConsoleApp` was excluded because it is the application entry point, not the target of the unit-testing requirements.
- Domain and Application are the main projects evaluated against the coverage targets.
- Infrastructure coverage is reported for information only.

## 2. Coverage Progress

| Stage | Project | Line Coverage | Branch Coverage |
|---|---|---:|---:|
| Baseline (LegacyTests only) | DriveNow.Domain | 3.5% | 0% |
| Baseline (LegacyTests only) | DriveNow.Application | 8.5% | 3.2% |
| After Task 1 | DriveNow.Domain | 3.5% | 0% |
| After Task 1 | DriveNow.Application | 8.5% | 3.2% |
| After Task 5 | DriveNow.Domain | 3.5% | 0% |
| After Task 5 | DriveNow.Application | 7.3% | 3.3% |
| After Task 6 | DriveNow.Domain | 3.5% | 0% |
| After Task 6 | DriveNow.Application | 6.7% | 3.3% |
| After Task 7 | DriveNow.Domain | 3.5% | 0% |
| After Task 7 | DriveNow.Application | 6.7% | 3.3% |
| After Task 8 | DriveNow.Domain | 98.02% | 88.89% |
| After Task 8 | DriveNow.Application | 99.34% | 95% |

The baseline and intermediate measurements used LegacyTests only. The final measurement after Task 8 used all available test projects. Therefore, the results are not directly comparable across every stage.

## 3. Coverage Targets

After Task 8, both target projects exceeded the required coverage:

| Project | Line Target | Actual | Branch Target | Actual |
|---|---:|---:|---:|---:|
| DriveNow.Domain | 90% | 98.02% | 85% | 88.89% |
| DriveNow.Application | 90% | 99.34% | 85% | 95% |

## 4. R5 — Coverage Is Not Correctness

In `PricingService`, the discount calculation could execute all lines and branches while still producing an incorrect result for a business-rule scenario.

The 25% discount cap must apply to the combined duration, loyalty, and promo discounts. Executing the calculation does not guarantee that the final discount respects this rule.

This demonstrates that code coverage measures code execution, not business-rule correctness.

## 5. R6 — Intentionally Uncovered Branch

Some branches in `LoyaltyService.UpgradeTier` remain uncovered, particularly the Platinum and Gold upgrade thresholds.

These cases were not added in this task because the available time was prioritized toward the main business rules and broader coverage requirements. They remain useful candidates for future boundary-value tests.