using FoodSmart.Api.Services;

namespace FoodSmart.Api.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_GeneratesHashThatVerifies()
    {
        const string password = "SenhaForte@2026";

        var hash = _hasher.Hash(password);

        Assert.NotEqual(password, hash);
        Assert.StartsWith("pbkdf2$", hash);
        Assert.True(_hasher.Verify(password, hash));
    }

    [Fact]
    public void Verify_ReturnsFalseForDifferentPassword()
    {
        var hash = _hasher.Hash("SenhaForte@2026");

        Assert.False(_hasher.Verify("OutraSenha@2026", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("pbkdf2$abc$salt$hash")]
    [InlineData("pbkdf2$100000$%%%$%%%")]
    [InlineData("pbkdf2$999999999$YWJj$YWJj")]
    public void Verify_ReturnsFalseForMalformedHash(string malformedHash)
    {
        Assert.False(_hasher.Verify("SenhaForte@2026", malformedHash));
    }
}
