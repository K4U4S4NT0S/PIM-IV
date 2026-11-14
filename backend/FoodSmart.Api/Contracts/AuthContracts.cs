using System.ComponentModel.DataAnnotations;

namespace FoodSmart.Api.Contracts;

public record LoginRequest(
    [param: Required]
    [param: EmailAddress]
    [param: StringLength(180)]
    string Email,

    [param: Required]
    [param: StringLength(256, MinimumLength = 1)]
    string Password
);

public record RegisterRequest(
    [param: Required, StringLength(120, MinimumLength = 2)]
    string Name,

    [param: Required, EmailAddress, StringLength(180)]
    string Email,

    [param: Required, StringLength(256, MinimumLength = 8)]
    string Password,

    [param: StringLength(30)]
    string? Phone
);
public record LoginResponse(string Token, object User);
