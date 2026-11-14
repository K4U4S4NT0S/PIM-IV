using System.Text.Json.Serialization;

namespace FoodSmart.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Recebido";
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
