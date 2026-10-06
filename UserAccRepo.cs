using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface IUserAccountsRepository
    {
        Task<(object User, List<string> Roles)> GetUserWithRolesAsync(string username);
    }

    public class UserAccountsRepository : IUserAccountsRepository
    {
        private readonly PandoraDBContext _context;

        public UserAccountsRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public async Task<(object User, List<string> Roles)> GetUserWithRolesAsync(string username)
        {
            var upperUsername = username.ToUpper();

            // Find the user by logon name
            var currentUser = await _context.Users
                .Where(u => u.Logon == upperUsername)
                .FirstOrDefaultAsync();

            if (currentUser == null)
            {
                return (null, new List<string>());
            }

            // Fetch the user's role names by joining UserRoles and Role tables
            var roles = await _context.UserRoles
                .Where(p => p.UserId == currentUser.Id)
                .Join(_context.Role,
                      userRole => userRole.RoleId,
                      role => role.Id,
                      (userRole, role) => role.Name)
                .ToListAsync();

            return (currentUser, roles);
        }
    }
}