using FoodSmart.Api.Contracts;
using FoodSmart.Api.Data;
using FoodSmart.Api.Models;
using FoodSmart.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSmart.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly FoodSmartDbContext _db;
    private readonly IAuthTokenService _auth;

    public ProductsController(FoodSmartDbContext db, IAuthTokenService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        if (includeInactive)
        {
            var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
            if (user?.Role != "Admin")
                return Unauthorized(new { message = "Acesso restrito ao administrador." });
        }

        var products = await _db.Products
            .AsNoTracking()
            .Where(product => includeInactive || product.IsActive)
            .OrderBy(product => product.Category)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id && product.IsActive, cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductRequest request, CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user?.Role != "Admin")
            return Unauthorized(new { message = "Acesso restrito ao administrador." });

        if (!IsValidProduct(request, out var validationMessage))
            return BadRequest(new { message = validationMessage });

        var product = new Product
        {
            Category = request.Category.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Emoji = string.IsNullOrWhiteSpace(request.Emoji) ? "🍔" : request.Emoji.Trim(),
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            IsFeatured = request.IsFeatured,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user?.Role != "Admin")
            return Unauthorized(new { message = "Acesso restrito ao administrador." });

        if (!IsValidProduct(request, out var validationMessage))
            return BadRequest(new { message = validationMessage });

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null) return NotFound();

        product.Category = request.Category.Trim();
        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.Emoji = string.IsNullOrWhiteSpace(request.Emoji) ? "🍔" : request.Emoji.Trim();
        product.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        product.IsFeatured = request.IsFeatured;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user?.Role != "Admin")
            return Unauthorized(new { message = "Acesso restrito ao administrador." });

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null) return NotFound();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static bool IsValidProduct(ProductRequest request, out string message)
    {
        if (string.IsNullOrWhiteSpace(request.Category) || request.Category.Trim().Length > 40)
            message = "Categoria é obrigatória e deve ter até 40 caracteres.";
        else if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120)
            message = "Nome é obrigatório e deve ter até 120 caracteres.";
        else if (request.Description is null || request.Description.Trim().Length > 600)
            message = "Descrição deve ter até 600 caracteres.";
        else if (request.Price <= 0 || request.Price > 99_999_999.99m)
            message = "Preço deve ser maior que zero e compatível com o banco de dados.";
        else if (request.Emoji?.Length > 20)
            message = "Emoji deve ter até 20 caracteres.";
        else if (request.ImageUrl?.Length > 1000)
            message = "Endereço da imagem deve ter até 1000 caracteres.";
        else
        {
            message = string.Empty;
            return true;
        }

        return false;
    }
}
