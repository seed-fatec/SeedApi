using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Configuration;
using SeedApi.Domain.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace SeedApi.Application.Services;

public class AuthService(IOptions<JwtSettings> jwtSettings, IPersistenceContext context)
{
  private readonly IPersistenceContext _context = context;
  private readonly JwtSettings _jwtSettings = jwtSettings.Value;

  public async Task<bool> RegisterAsync(string name, string email, string password, UserRole role)
  {
    var userExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email);

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

  public async Task<bool> RegisterAdminAsync(string email, string password)
  {
    var adminExists = await _context.Admins.IgnoreQueryFilters().AnyAsync(u => u.Email == email);

    if (adminExists)
      return false;

    var newAdmin = new Admin
    {
      Email = email,
      PasswordHash = HashPassword(password),
    };

    _context.Admins.Add(newAdmin);
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

  public async Task<(string? accessToken, string? refreshToken)> AuthenticateAdminAsync(string email, string password)
  {
    var admin = await _context.Admins.Include(u => u.RefreshToken).FirstOrDefaultAsync(u => u.Email == email);

    if (admin == null || !VerifyPassword(password, admin.PasswordHash))
      return (null, null);

    var accessToken = GenerateAdminJwtToken();
    var refreshToken = GenerateAdminJwtRefreshToken();

    admin.RefreshToken = new AdminRefreshToken
    {
      Token = refreshToken,
      ExpiryTime = DateTime.UtcNow.AddDays(1)
    };

    _context.Admins.Update(admin);

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

  private string GenerateAdminJwtRefreshToken()
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

    var claims = new List<Claim>
    {
      new ("TokenType", "RefreshToken")
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddDays(1),
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
  private string GenerateAdminJwtToken()
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

    var claims = new List<Claim>
    {
      new (ClaimTypes.Role, "admin"),
      new ("TokenType", "AccessToken"),
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

  public async Task<bool> RevokeAdminRefreshTokenAsync(string refreshToken)
  {
    var admin = await _context.Admins
      .Include(a => a.RefreshToken)
      .FirstOrDefaultAsync(a => a.RefreshToken != null && a.RefreshToken.Token == refreshToken);

    if (admin == null)
      return false;

    admin.RefreshToken = null;
    _context.Admins.Update(admin);
    await _context.SaveChangesAsync();
    return true;
  }
}
