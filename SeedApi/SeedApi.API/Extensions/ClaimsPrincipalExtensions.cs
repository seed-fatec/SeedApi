using System.Security.Claims;

namespace SeedApi.API.Extensions;

public static class ClaimsPrincipalExtensions
{
  public static bool IsAdmin(this ClaimsPrincipal user)
  {
    return user.IsInRole("admin");
  }
}
