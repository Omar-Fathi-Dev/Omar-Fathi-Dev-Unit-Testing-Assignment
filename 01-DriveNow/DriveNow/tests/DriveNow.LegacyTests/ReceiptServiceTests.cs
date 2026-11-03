using DriveNow.Application.Receipts;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.ValueObjects;

namespace DriveNow.LegacyTests;

public class ReceiptServiceTests
{
    [Fact]
    public async Task Build_CompletedRentalWithLateFeeAndDamage_MatchesReceiptSnapshot()
    {
        Booking booking = new()
        {
            Reference = "DN-TEST-0001",
            Status = BookingStatus.Completed,
            Price = new PriceBreakdown
            {
                BaseAmount = 1000m,
                InsuranceCost = 100m
            }
        };

        ReturnReport report = new()
        {
            LateFee = 50m,
            DamageCharge = 200m
        };

        ReceiptService sut = new(
            new LineItemFactory(),
            new TaxCalculator(),
            new CurrencyFormatter(),
            new ReceiptFormatter());

        string receipt = sut.Build(booking, report);

        await Verifier.Verify(receipt);
    }
}