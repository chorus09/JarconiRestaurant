using BCrypt.Net;
using JarconiRestaurant.Auth.Jwt;
using JarconiRestaurant.Data;
using JarconiRestaurant.Domain.Enums;
using JarconiRestaurant.Domain.Users;
using JarconiRestaurant.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarconiRestaurant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(AppDbContext db, JwtTokenService jwt) {
        _db = db;
        _jwt = jwt;
    }

    // POST /api/auth/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDto dto) {
        dto.Email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("EMAIL_ALREADY_EXISTS");

        var user = new User {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Client
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, exp) = _jwt.CreateToken(user);
        return Ok(new AuthResponse {
            Token = token,
            ExpiresAtUtc = exp,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id
        });
    }

    // POST /api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto dto) {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLowerInvariant());
        if (user is null) return Unauthorized("INVALID_CREDENTIALS");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("INVALID_CREDENTIALS");

        if (user.IsBlocked) return Forbid();

        var (token, exp) = _jwt.CreateToken(user);
        return Ok(new AuthResponse {
            Token = token,
            ExpiresAtUtc = exp,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id
        });
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me() {
        var email = User.Identity?.Name ?? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var idClaim = User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
        var id = int.Parse(idClaim);
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();
        return Ok(new { user.Id, user.Email, Role = user.Role.ToString(), user.CreatedAtUtc });
    }
}