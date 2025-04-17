using System.IdentityModel.Tokens.Jwt;

namespace SeedApi.Middlewares;

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

    var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last();

    if (string.IsNullOrEmpty(token))
    {
      await _next(context);
      return;
    }

    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);
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
