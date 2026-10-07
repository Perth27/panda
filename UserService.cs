using System.Collections.Generic;
using System.Threading.Tasks;
using Pandora.Models;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface IRolesService
    {
        Task<List<Role>> GetRolesAsync();
        Task<Role> GetRoleDetailsAsync(string id);
        Task CreateRoleAsync(Role role);
        Task<Role> GetRoleForEditAsync(string id);
        Task UpdateRoleAsync(Role role);
        Task<Role> GetRoleForDeleteAsync(string id);
        Task DeleteRoleAsync(string id);
        bool RoleExists(string id);
    }

    public class RolesService : IRolesService
    {
        private readonly IRolesRepository _repository;

        public RolesService(IRolesRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _repository.GetAllRolesAsync();
        }

        public async Task<Role> GetRoleDetailsAsync(string id)
        {
            return await _repository.GetRoleByIdAsync(id);
        }

        public async Task CreateRoleAsync(Role role)
        {
            await _repository.AddRoleAsync(role);
        }

        public async Task<Role> GetRoleForEditAsync(string id)
        {
            return await _repository.FindRoleByIdAsync(id);
        }

        public async Task UpdateRoleAsync(Role role)
        {
            await _repository.UpdateRoleAsync(role);
        }

        public async Task<Role> GetRoleForDeleteAsync(string id)
        {
            return await _repository.GetRoleByIdAsync(id);
        }

        public async Task DeleteRoleAsync(string id)
        {
            var role = await _repository.FindRoleByIdAsync(id);
            if (role != null)
            {
                await _repository.RemoveRoleAsync(role);
            }
        }

        public bool RoleExists(string id)
        {
            return _repository.RoleExists(id);
        }
    }
}