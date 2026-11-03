# Task 8 — The Legacy Tests

**Student Name:** Omar Fathy  
**Group/ID:** 3

## R1 — Predictions Before Running the Tests

Before running the original tests, I predicted the following results:

| Test Name | Prediction |
|---|---|
| `Build_CallsLineItemFactoryOnce` | Pass if the line item factory is called exactly once. |
| `Build_CallsTaxCalculatorOncePerLine` | Pass if the tax calculator is called once for each line item. |
| `Build_CallsCurrencyFormatterTwicePerLinePlusTwo` | Pass if the currency formatter is called twice for each line item, plus twice for VAT and Total. |
| `Build_CallsCollaboratorsInOrder` | Pass if the collaborators are called in the expected order. |
| `Build_ReturnsWhatTheRendererReturns` | Pass if the returned receipt is exactly what the receipt formatter returns. |

## R4 — What the Legacy Tests Actually Checked

The original tests used mocks to verify how `ReceiptService` called its collaborators.

They checked:

- Whether the line item factory was called once.
- Whether the tax calculator was called once per line item.
- How many times the currency formatter was called.
- The order in which collaborators were called.
- Whether `ReceiptService` returned the result from the receipt formatter.

### Why Two Tests Failed

The original implementation formatted each line item's amount twice.

I changed `ReceiptService.Build` to format each amount once and reuse the formatted value.

The receipt text stayed the same, but the number of formatter calls changed.

The old test expected 8 calls for 3 line items. The new implementation made 5 calls.

The order test also failed because it expected the old sequence of calls.

These failures showed that the tests were checking implementation details, not only the final behavior.

### Would These Tests Catch Incorrect VAT?

No.

The tests did not verify the actual VAT amount or the final receipt total.

The tax calculator was mocked, so the tests could pass even if the real VAT calculation was incorrect.

A test using the real tax calculator and checking the final receipt would provide better coverage.

### Which Four Interfaces Were Mocked?

The original tests mocked these interfaces:

- `ILineItemFactory`
- `ITaxCalculator`
- `ICurrencyFormatter`
- `IReceiptFormatter`

## R5 — Replacement Test

The legacy mock-based tests were replaced with a test using real receipt collaborators.

The test uses a completed rental with a late fee and damage charge.

Verify checks the complete receipt text against a snapshot.

This verifies the receipt's visible output, including the calculated VAT and total.

## R6 — Snapshot Difference

I temporarily changed the receipt label from `VAT 14%` to `Tax`.

Verify detected the difference between the actual receipt and the saved snapshot.

I captured the snapshot difference and then reverted the label to `VAT 14%`.

The test passed again after reverting the change.

## R7 — Removing Unnecessary Interfaces

I removed these interfaces:

- `ILineItemFactory`
- `ITaxCalculator`
- `ICurrencyFormatter`
- `IReceiptFormatter`

I also updated `ReceiptService` to use the concrete classes directly.

### Reason

Each interface had only one implementation in the current project.

The interfaces were used only to provide abstraction for `ReceiptService`.

The project does not currently need multiple implementations of these collaborators, and it does not use a dependency injection container to select implementations.

Removing the interfaces makes the code simpler and removes unnecessary abstraction.

The receipt tests now use real collaborators instead of mocking these interfaces.

I kept mocks for other dependencies where they are still useful for testing service behavior.
