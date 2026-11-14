using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FoodSmart.Api.Data;
using FoodSmart.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodSmart.Api.Services;

public interface IAuthTokenService
{
    string CreateToken(User user);
    Task<User?> GetAuthenticatedUserAsync(
        HttpRequest request,
        CancellationToken cancellationToken = default);
}

public class AuthTokenService : IAuthTokenService
{
    private readonly IConfiguration _configuration;
    private readonly FoodSmartDbContext _db;

    public AuthTokenService(IConfiguration configuration, FoodSmartDbContext db)
    {
        _configuration = configuration;
        _db = db;
    }

    public string CreateToken(User user)
    {
        var payload = JsonSerializer.Serialize(new
        {
            user.Id,
            Exp = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds()
        });

        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        return $"{payloadBase64}.{Sign(payloadBase64)}";
    }

    public async Task<User?> GetAuthenticatedUserAsync(
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = ReadValidUserId(request);
        if (userId is null)
            return null;

        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Id == userId.Value && user.IsActive,
                cancellationToken);
    }

    private int? ReadValidUserId(HttpRequest request)
    {
        try
        {
            var authorization = request.Headers.Authorization.ToString();
            var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization[7..].Trim()
                : request.Headers["X-FoodSmart-Token"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var parts = token.Split('.');
            if (parts.Length != 2 || parts.Any(part => string.IsNullOrWhiteSpace(part)))
                return null;

            var expected = Encoding.UTF8.GetBytes(Sign(parts[0]));
            var supplied = Encoding.UTF8.GetBytes(parts[1]);

            if (expected.Length != supplied.Length ||
                !CryptographicOperations.FixedTimeEquals(expected, supplied))
                return null;

            using var document = JsonDocument.Parse(
                Encoding.UTF8.GetString(Base64UrlDecode(parts[0])));

            var root = document.RootElement;
            if (!root.TryGetProperty("Exp", out var expiration) ||
                !expiration.TryGetInt64(out var exp) ||
                exp <= DateTimeOffset.UtcNow.ToUnixTimeSeconds() ||
                !root.TryGetProperty("Id", out var id) ||
                !id.TryGetInt32(out var userId) ||
                userId <= 0)
                return null;

            return userId;
        }
        catch (Exception exception) when (
            exception is FormatException ||
            exception is JsonException ||
            exception is InvalidOperationException ||
            exception is ArgumentException)
        {
            return null;
        }
    }

    private string Sign(string payload)
    {
        var key = _configuration["Auth:Secret"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException(
                "Configure Auth:Secret com pelo menos 32 bytes por variável de ambiente.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
