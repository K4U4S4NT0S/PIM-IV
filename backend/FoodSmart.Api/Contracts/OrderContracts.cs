
using System.ComponentModel.DataAnnotations;

namespace FoodSmart.Api.Contracts;

public class CreateOrderRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 8)]
    public string CustomerPhone { get; set; } = string.Empty;

    [StringLength(250)]
    public string? CustomerAddress { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 50)]
    public int Quantity { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required]
    [StringLength(40)]
    public string Status { get; set; } = string.Empty;
}
