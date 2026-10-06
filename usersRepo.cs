using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;
using Pandora.ViewModels;

namespace Pandora.Repositories
{
    public interface IUsersRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string id);
        Task<User> FindUserByIdAsync(string id);
        IEnumerable<dynamic> GetAllRoles();
        User GetExistingUserByLogon(string logon);
        void UpdateUser(UserVM user);
        Task SaveChangesAsync();
        void RemoveUser(User user);
        bool UserExists(string id);
    }

    public class UsersRepository : IUsersRepository
    {
        private readonly PandoraDBContext _context;

        public UsersRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.User.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _context.User.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<User> FindUserByIdAsync(string id)
        {
            return await _context.User.FindAsync(id);
        }

        public IEnumerable<dynamic> GetAllRoles()
        {
            return _context.Roles.ToList();
        }

        public User GetExistingUserByLogon(string logon)
        {
            return _context.Users.Where(m => m.Logon == logon).SingleOrDefault();
        }

        public void UpdateUser(UserVM user)
        {
            _context.Update(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void RemoveUser(User user)
        {
            _context.User.Remove(user);
        }

        public bool UserExists(string id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}