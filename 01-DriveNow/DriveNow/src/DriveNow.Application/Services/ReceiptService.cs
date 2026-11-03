using DriveNow.Application.Receipts;
using DriveNow.Domain.Entities;
using DriveNow.Domain.ValueObjects;

namespace DriveNow.Application.Services;

public class ReceiptService
{
    readonly LineItemFactory _lineItems;
    readonly TaxCalculator _tax;
    readonly CurrencyFormatter _currency;
    readonly ReceiptFormatter _receipts;

    public ReceiptService(
        LineItemFactory lineItems,
        TaxCalculator tax,
        CurrencyFormatter currency,
        ReceiptFormatter receipts)
    {
        _lineItems = lineItems;
        _tax = tax;
        _currency = currency;
        _receipts = receipts;
    }

    public string Build(Booking booking, ReturnReport? report)
    {
        var items = _lineItems.Create(booking, report);
        decimal vat = 0m;
        decimal net = 0m;

        var formattedItems = new List<(LineItem Item, string Amount)>();
        var width = 0;

        foreach (var item in items)
        {
            vat += _tax.CalculateVat(item.Amount);
            net += item.Amount;

            string amount = _currency.Format(item.Amount);
            formattedItems.Add((item, amount));

            width = Math.Max(width, amount.Length);
        }

        var total = net + vat;

        var lines = new List<string>();

        foreach (var formattedItem in formattedItems)
        {
            lines.Add(
                formattedItem.Item.Description.PadRight(30, '.') +
                formattedItem.Amount.PadLeft(width));
        }

        lines.Add("VAT 14%".PadRight(30, '.') + _currency.Format(vat).PadLeft(width));
        lines.Add("Total".PadRight(30, '.') + _currency.Format(total).PadLeft(width));

        return _receipts.Render(booking.Reference, lines);
    }
}