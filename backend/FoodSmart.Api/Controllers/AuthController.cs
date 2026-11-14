using FoodSmart.Api.Contracts;
using FoodSmart.Api.Data;
using FoodSmart.Api.Models;
using FoodSmart.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSmart.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly FoodSmartDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenService _tokenService;

    public AuthController(
        FoodSmartDbContext db,
        IPasswordHasher passwordHasher,
        IAuthTokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Informe e-mail e senha." });

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(
            current => current.Email == email && current.IsActive,
            cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        return Ok(new LoginResponse(
            _tokenService.CreateToken(user),
            new { user.Id, user.Name, user.Email, user.Role }));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 120)
            return BadRequest(new { message = "Informe um nome entre 2 e 120 caracteres." });

        if (string.IsNullOrWhiteSpace(email) || email.Length > 180 ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            return BadRequest(new { message = "Informe um e-mail válido com até 180 caracteres." });

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 256)
            return BadRequest(new { message = "A senha deve possuir entre 8 e 256 caracteres." });

        if (await _db.Users.AnyAsync(current => current.Email == email, cancellationToken))
            return Conflict(new { message = "Este e-mail já está cadastrado." });

        var user = new User
        {
            Name = name,
            Email = email,
            Role = "Cliente",
            PasswordHash = _passwordHasher.Hash(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // O índice único do banco também protege contra dois cadastros simultâneos.
            return Conflict(new { message = "Não foi possível cadastrar: este e-mail pode já estar em uso." });
        }

        return Created(
            $"/api/auth/users/{user.Id}",
            new LoginResponse(
                _tokenService.CreateToken(user),
                new { user.Id, user.Name, user.Email, user.Role }));
    }
}
