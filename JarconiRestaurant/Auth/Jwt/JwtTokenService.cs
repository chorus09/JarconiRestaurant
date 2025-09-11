using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JarconiRestaurant.Auth.Jwt;
using JarconiRestaurant.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JarconiRestaurant.Auth.Jwt;

public class JwtTokenService {
    private readonly JwtOptions _opt;

    public JwtTokenService(IOptions<JwtOptions> opt) => _opt = opt.Value;

    public (string token, DateTime expires) CreateToken(User user) {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_opt.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}