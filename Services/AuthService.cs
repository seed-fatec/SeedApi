using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SeedApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SeedApi.Services
{
  public class AuthService(IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
  {
    private readonly ApplicationDbContext _context = context;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<bool> RegisterAsync(string name, string email, string password, UserRole role)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Email == email);

      if (userExists)
        return false;

      var newUser = new User
      {
        Name = name,
        Email = email,
        PasswordHash = HashPassword(password),
        Role = role
      };

      _context.Users.Add(newUser);
      await _context.SaveChangesAsync();
      return true;
    }

    public async Task<(string? accessToken, string? refreshToken)> AuthenticateAsync(string email, string password, UserRole role)
    {
      var user = await _context.Users.Include(u => u.RefreshToken).FirstOrDefaultAsync(u => u.Email == email && u.Role == role);
      if (user == null || !VerifyPassword(password, user.PasswordHash))
        return (null, null);

      var accessToken = GenerateJwtToken(user);
      var refreshToken = GenerateJwtRefreshToken(user);

      user.RefreshToken = new RefreshToken
      {
        Token = refreshToken,
        ExpiryTime = DateTime.UtcNow.AddDays(7)
      };

      _context.Users.Update(user);
      await _context.SaveChangesAsync();

      return (accessToken, refreshToken);
    }

    private string GenerateJwtRefreshToken(User user)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

      var claims = new List<Claim>
        {
          new ("UserId", user.Id.ToString()),
          new ("TokenType", "RefreshToken")
        };

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddDays(7),
        Issuer = _jwtSettings.Issuer,
        Audience = _jwtSettings.Audience,
        SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }

    private string GenerateJwtToken(User user)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

      var claims = new List<Claim>
            {
              new ("UserId", user.Id.ToString()),
              new ("TokenType", "AccessToken"),
              new (ClaimTypes.Role, user.Role.ToString())
            };

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(1),
        Issuer = _jwtSettings.Issuer,
        Audience = _jwtSettings.Audience,
        SigningCredentials = new SigningCredentials(
          new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }

    private static string HashPassword(string password)
    {
      var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
      return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
      return HashPassword(password) == storedHash;
    }

    public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
    {
      var user = await _context.Users
        .Include(u => u.RefreshToken)
        .FirstOrDefaultAsync(u => u.RefreshToken != null && u.RefreshToken.Token == refreshToken);

      if (user == null || user.RefreshToken == null || user.RefreshToken.ExpiryTime <= DateTime.UtcNow)
        return null;

      return GenerateJwtToken(user);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
      var user = await _context.Users
        .Include(u => u.RefreshToken)
        .FirstOrDefaultAsync(u => u.RefreshToken != null && u.RefreshToken.Token == refreshToken);

      if (user == null)
        return false;

      user.RefreshToken = null;
      _context.Users.Update(user);
      await _context.SaveChangesAsync();
      return true;
    }
  }
}