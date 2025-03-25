using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SeedApi.Models.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace SeedApi.Middlewares;

public sealed class RequireRoleAttribute(UserRole requiredRole) : ActionFilterAttribute
{
  private readonly UserRole _requiredRole = requiredRole;

  public override void OnActionExecuting(ActionExecutingContext context)
  {
    var token = context.HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last();

    if (string.IsNullOrEmpty(token))
    {
      context.Result = new UnauthorizedResult();
      return;
    }

    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);

    if (jwtToken == null)
    {
      context.Result = new UnauthorizedResult();
      return;
    }

    var roles = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

    if (!roles.Contains(_requiredRole.ToString()))
    {
      context.Result = new ForbidResult();
      return;
    }

    base.OnActionExecuting(context);
  }
}