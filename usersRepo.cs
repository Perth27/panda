using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface IRolesRepository
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(string id);
        Task<Role> FindRoleByIdAsync(string id);
        Task AddRoleAsync(Role role);
        Task UpdateRoleAsync(Role role);
        Task RemoveRoleAsync(Role role);
        bool RoleExists(string id);
    }

    public class RolesRepository : IRolesRepository
    {
        private readonly PandoraDBContext _context;

        public RolesRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Role.ToListAsync();
        }

        public async Task<Role> GetRoleByIdAsync(string id)
        {
            return await _context.Role.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Role> FindRoleByIdAsync(string id)
        {
            return await _context.Role.FindAsync(id);
        }

        public async Task AddRoleAsync(Role role)
        {
            _context.Add(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(Role role)
        {
            _context.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRoleAsync(Role role)
        {
            _context.Role.Remove(role);
            await _context.SaveChangesAsync();
        }

        public bool RoleExists(string id)
        {
            return _context.Role.Any(e => e.Id == id);
        }
    }
}