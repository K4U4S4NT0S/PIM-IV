namespace FoodSmart.Api.Contracts;

public record ProductRequest(
    string Category,
    string Name,
    string Description,
    decimal Price,
    string Emoji,
    string? ImageUrl,
    bool IsFeatured,
    bool IsActive
);
