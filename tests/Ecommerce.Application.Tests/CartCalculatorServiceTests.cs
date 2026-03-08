using Ecommerce.Application.Cart;

namespace Ecommerce.Application.Tests;

public class CartCalculatorServiceTests
{
    private readonly CartCalculatorService _service = new();

    [Fact]
    public void CalculateSummary_WithItems_ReturnsExpectedTotals()
    {
        CartSummaryRequest request = new()
        {
            Items =
            [
                new CartItemInput { ProductId = 1, ProductName = "A", UnitPrice = 100m, Quantity = 2 },
                new CartItemInput { ProductId = 2, ProductName = "B", UnitPrice = 50m, Quantity = 1 }
            ]
        };

        CartSummaryResponse result = _service.CalculateSummary(request);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal(250m, result.Subtotal);
        Assert.Equal(25m, result.Tax);
        Assert.Equal(275m, result.Total);
    }

    [Fact]
    public void CalculateSummary_WithEmptyItems_ReturnsZeroTotals()
    {
        CartSummaryResponse result = _service.CalculateSummary(new CartSummaryRequest());

        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0m, result.Subtotal);
        Assert.Equal(0m, result.Tax);
        Assert.Equal(0m, result.Total);
    }

    [Fact]
    public void CalculateSummary_WithNegativePrice_Throws()
    {
        CartSummaryRequest request = new()
        {
            Items = [new CartItemInput { ProductId = 1, UnitPrice = -1m, Quantity = 1 }]
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => _service.CalculateSummary(request));
    }
}
