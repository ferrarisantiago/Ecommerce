using Ecommerce.Application.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartCalculatorService _cartCalculator;

    public CartController(ICartCalculatorService cartCalculator)
    {
        _cartCalculator = cartCalculator;
    }

    [HttpPost("summary")]
    [ProducesResponseType(typeof(CartSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetSummary([FromBody] CartSummaryRequest request)
    {
        try
        {
            var summary = _cartCalculator.CalculateSummary(request);
            return Ok(summary);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Problem(statusCode: 400, title: "Invalid cart payload", detail: ex.Message);
        }
    }
}
