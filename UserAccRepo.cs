using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface IUserAccountsRepository
    {
        Task<object> GetUserByLogonAsync(string logon);
        Task<List<string>> GetUserRoleNamesAsync(string userId);
    }

    public class UserAccountsRepository : IUserAccountsRepository
    {
        private readonly PandoraDBContext _context;

        public UserAccountsRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public async Task<object> GetUserByLogonAsync(string logon)
        {
            return await _context.Users
                .Where(u => u.Logon == logon)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetUserRoleNamesAsync(string userId)
        {
            var usersRoles = _context.UserRoles.Where(p => p.UserId == userId);
            return await _context.Roles
                .Join(usersRoles, role => role.Id, userRole => userRole.RoleId, (role, userRole) => role.Name)
                .ToListAsync();
        }
    }
}