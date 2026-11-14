using FoodSmart.Api.Contracts;
using FoodSmart.Api.Data;
using FoodSmart.Api.Models;
using FoodSmart.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSmart.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly FoodSmartDbContext _db;
    private readonly IAuthTokenService _auth;

    private static readonly IReadOnlyDictionary<string, string[]> AllowedTransitions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Recebido"] = ["Preparando", "Cancelado"],
            ["Preparando"] = ["Saiu para entrega", "Cancelado"],
            ["Saiu para entrega"] = ["Concluído", "Cancelado"],
            ["Concluído"] = [],
            ["Cancelado"] = []
        };

    public OrdersController(FoodSmartDbContext db, IAuthTokenService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user?.Role != "Admin")
            return Unauthorized(new { message = "Acesso restrito ao administrador." });

        var orders = await _db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user is null)
            return Unauthorized(new { message = "Faça login para visualizar seus pedidos." });

        var orders = await _db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .Where(order => order.UserId == user.Id)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
            return BadRequest(new { message = "O pedido precisa conter pelo menos um item." });

        if (request.Items.Count > 50)
            return BadRequest(new { message = "O pedido não pode conter mais de 50 itens." });

        if (request.Items.Any(item => item is null || item.ProductId <= 0 || item.Quantity is < 1 or > 50))
            return BadRequest(new { message = "Todos os itens devem possuir produto válido e quantidade entre 1 e 50." });

        if (string.IsNullOrWhiteSpace(request.CustomerName) ||
            request.CustomerName.Trim().Length > 120)
            return BadRequest(new { message = "Informe um nome válido com até 120 caracteres." });

        if (string.IsNullOrWhiteSpace(request.CustomerPhone) ||
            request.CustomerPhone.Trim().Length is < 8 or > 30)
            return BadRequest(new { message = "Informe um telefone válido com até 30 caracteres." });

        if (request.CustomerAddress?.Length > 250)
            return BadRequest(new { message = "O endereço deve ter até 250 caracteres." });

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var customTokenHeader = Request.Headers["X-FoodSmart-Token"].ToString();
        var hasToken = !string.IsNullOrWhiteSpace(authorizationHeader) ||
                       !string.IsNullOrWhiteSpace(customTokenHeader);
        var authenticatedUser = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);

        if (hasToken && authenticatedUser is null)
            return Unauthorized(new { message = "Token inválido ou conta desativada." });

        var productIds = request.Items.Select(item => item.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(product => productIds.Contains(product.Id) && product.IsActive)
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        if (products.Count != productIds.Count)
            return BadRequest(new { message = "Um ou mais produtos estão indisponíveis." });

        if (products.Values.Any(product => product.Price <= 0))
            return BadRequest(new { message = "Um ou mais produtos possuem preço inválido. Contate o administrador." });

        var order = new Order
        {
            UserId = authenticatedUser?.Id,
            CustomerName = request.CustomerName.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            CustomerAddress = request.CustomerAddress?.Trim() ?? string.Empty,
            Status = "Recebido",
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var product = products[item.ProductId];
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);

        if (order.Total <= 0 || order.Total > 99_999_999.99m)
            return BadRequest(new { message = "O valor total do pedido ultrapassa o limite permitido." });

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var createdOrder = await _db.Orders
            .AsNoTracking()
            .Include(savedOrder => savedOrder.Items)
            .ThenInclude(item => item.Product)
            .FirstAsync(savedOrder => savedOrder.Id == order.Id, cancellationToken);

        return Created($"/api/orders/{order.Id}", createdOrder);
    }

    [HttpGet("{id:int}/status")]
    public async Task<IActionResult> GetStatus(int id, CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user is null)
            return Unauthorized(new { message = "Faça login para acompanhar este pedido." });

        var order = await _db.Orders
            .AsNoTracking()
            .Include(current => current.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (order is null)
            return NotFound(new { message = "Pedido não encontrado." });

        if (user.Role != "Admin" && order.UserId != user.Id)
            return Unauthorized(new { message = "Você não tem permissão para acompanhar este pedido." });

        return Ok(order);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user?.Role != "Admin")
            return Unauthorized(new { message = "Acesso restrito ao administrador." });

        var requestedStatus = request.Status?.Trim();
        var canonicalStatus = AllowedTransitions.Keys
            .FirstOrDefault(status => string.Equals(status, requestedStatus, StringComparison.OrdinalIgnoreCase));

        if (canonicalStatus is null)
            return BadRequest(new { message = "Status inválido." });

        var order = await _db.Orders
            .Include(current => current.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (order is null)
            return NotFound(new { message = "Pedido não encontrado." });

        if (string.Equals(order.Status, canonicalStatus, StringComparison.OrdinalIgnoreCase))
            return Ok(order);

        if (!AllowedTransitions.TryGetValue(order.Status, out var nextStatuses) ||
            !nextStatuses.Contains(canonicalStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                message = $"Não é permitido alterar o status de '{order.Status}' para '{canonicalStatus}'."
            });
        }

        order.Status = canonicalStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(order);
    }
}
