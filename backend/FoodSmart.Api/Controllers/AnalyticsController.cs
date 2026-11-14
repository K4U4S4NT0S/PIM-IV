using FoodSmart.Api.Data;
using FoodSmart.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSmart.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly FoodSmartDbContext _db;
    private readonly IAuthTokenService _auth;

    public AnalyticsController(FoodSmartDbContext db, IAuthTokenService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var user = await _auth.GetAuthenticatedUserAsync(Request, cancellationToken);
        if (user?.Role != "Admin")
            return Unauthorized(new { message = "Acesso restrito ao administrador." });

        var orders = await _db.Orders
            .AsNoTracking()
            .Select(order => new
            {
                order.Total,
                order.Status,
                order.CreatedAt,
                Items = order.Items.Select(item => new
                {
                    item.ProductId,
                    ProductName = item.Product != null ? item.Product.Name : null,
                    item.Quantity,
                    item.UnitPrice
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        var completedOrders = orders
            .Where(order => !string.Equals(order.Status, "Cancelado", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var revenue = completedOrders.Sum(order => order.Total);
        var orderCount = orders.Count;
        var averageTicket = completedOrders.Count == 0 ? 0m : revenue / completedOrders.Count;

        var mostSoldProducts = completedOrders
            .SelectMany(order => order.Items)
            .GroupBy(item => item.ProductName ?? $"Produto #{item.ProductId}")
            .Select(group => new
            {
                name = group.Key,
                quantity = group.Sum(item => item.Quantity),
                total = group.Sum(item => item.Quantity * item.UnitPrice)
            })
            .OrderByDescending(item => item.quantity)
            .Take(5)
            .ToList();

        var revenueByDay = completedOrders
            .GroupBy(order => order.CreatedAt.Date)
            .OrderBy(group => group.Key)
            .TakeLast(7)
            .Select(group => new
            {
                date = group.Key.ToString("dd/MM"),
                total = group.Sum(order => order.Total)
            })
            .ToList();

        var ordersByStatus = orders
            .GroupBy(order => order.Status)
            .Select(group => new { status = group.Key, count = group.Count() })
            .OrderByDescending(item => item.count)
            .ToList();

        var peakHour = completedOrders
            .GroupBy(order => order.CreatedAt.Hour)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => $"{group.Key:00}:00")
            .FirstOrDefault() ?? "Sem dados";

        return Ok(new { revenue, orderCount, averageTicket, mostSoldProducts, revenueByDay, ordersByStatus, peakHour });
    }
}
