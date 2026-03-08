using Ecommerce.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductQueryService _productQueryService;

    public ProductsController(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var products = await _productQueryService.GetCatalogAsync(cancellationToken);
        return Ok(products);
    }
}
