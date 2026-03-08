using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Cart;

public class CartItemInput
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(1, 10_000)]
    public int Quantity { get; set; }
}

public class CartSummaryRequest
{
    public IList<CartItemInput> Items { get; set; } = new List<CartItemInput>();
}

public class CartSummaryResponse
{
    public int TotalItems { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

public interface ICartCalculatorService
{
    CartSummaryResponse CalculateSummary(CartSummaryRequest request);
}
