using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface IUserAccountsRepository
    {
        User GetUserByLogon(string logon);
        List<string> GetUserRolesNames(string userId);
    }

    public class UserAccountsRepository : IUserAccountsRepository
    {
        private readonly PandoraDBContext _context;

        public UserAccountsRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public User GetUserByLogon(string logon)
        {
            // Replaced .ToUpper() with EF.Functions.ILike to fix PostgreSQL case-sensitivity
            return _context.Users
                .Where(u => EF.Functions.ILike(u.Logon, logon))
                .ToList()
                .FirstOrDefault();
        }

        public List<string> GetUserRolesNames(string userId)
        {
            var usersRoles = _context.UserRoles.Where(p => p.UserId == userId);
            
            return _context.Roles
                .Join(usersRoles, roles => roles.Id, userRoles => userRoles.RoleId, (roles, userRoles) => roles)
                .Select(roles => roles.Name)
                .ToList();
        }
    }
}