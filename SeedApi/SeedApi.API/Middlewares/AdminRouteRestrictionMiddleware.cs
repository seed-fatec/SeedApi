using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace SeedApi.API.Middlewares;

public class AdminRouteRestrictionMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

  public async Task Invoke(HttpContext context)
  {
    var endpoint = context.GetEndpoint();

    if (endpoint == null)
    {
      await _next(context);
      return;
    }

    var authorize = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();

    if (authorize == null)
    {
      await _next(context);
      return;
    }

    var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last();

    if (string.IsNullOrEmpty(token))
    {
      await _next(context);
      return;
    }

    var handler = new JwtSecurityTokenHandler();
    JwtSecurityToken jwtToken;
    try
    {
      jwtToken = handler.ReadJwtToken(token);
    }
    catch (Exception)
    {
      context.Response.StatusCode = 401;
      await context.Response.WriteAsJsonAsync(new { message = "Token JWT ausente ou mal formatado." });
      return;
    }
    var roles = jwtToken?.Claims
        .Where(c => c.Type == "role")
        .Select(c => c.Value)
        .ToList();

    if (roles == null || !roles.Contains("admin"))
    {
      await _next(context);
      return;
    }

    var hasOnlyAdmin = endpoint.Metadata.GetMetadata<OnlyAdminAttribute>() != null;
    var hasAllowAdmin = endpoint.Metadata.GetMetadata<AllowAdminAttribute>() != null;

    if (!hasOnlyAdmin && !hasAllowAdmin)
    {
      context.Response.StatusCode = 403;
      await context.Response.WriteAsync("Admins não podem acessar esta rota.");
      return;
    }

    await _next(context);
  }
}
