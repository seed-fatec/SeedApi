using SeedApi.Models;

namespace SeedApi.Services
{
  public class UserService(ApplicationDbContext context)
  {
    private readonly ApplicationDbContext _context = context;

    public async Task<User?> GetUserByIdAsync(int userId)
    {
      return await _context.Users.FindAsync(userId);
    }
  }
}