namespace Ecommerce.Application.Cart;

public class CartCalculatorService : ICartCalculatorService
{
    private const decimal DefaultTaxRate = 0.1m;

    public CartSummaryResponse CalculateSummary(CartSummaryRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        decimal subtotal = 0m;
        int totalItems = 0;

        foreach (CartItemInput item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Item quantity must be greater than zero.");
            }

            if (item.UnitPrice < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Item price cannot be negative.");
            }

            totalItems += item.Quantity;
            subtotal += item.UnitPrice * item.Quantity;
        }

        decimal tax = decimal.Round(subtotal * DefaultTaxRate, 2, MidpointRounding.AwayFromZero);
        decimal total = subtotal + tax;

        return new CartSummaryResponse
        {
            TotalItems = totalItems,
            Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            Tax = tax,
            Total = decimal.Round(total, 2, MidpointRounding.AwayFromZero)
        };
    }
}
