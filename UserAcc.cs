using System.Collections.Generic;
using System.Threading.Tasks;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface IUserAccountsService
    {
        Task<object> GetUserByLogonAsync(string logon);
        Task<List<string>> GetUserRoleNamesAsync(string userId);
    }

    public class UserAccountsService : IUserAccountsService
    {
        private readonly IUserAccountsRepository _userAccountsRepository;

        public UserAccountsService(IUserAccountsRepository userAccountsRepository)
        {
            _userAccountsRepository = userAccountsRepository;
        }

        public async Task<object> GetUserByLogonAsync(string logon)
        {
            return await _userAccountsRepository.GetUserByLogonAsync(logon);
        }

        public async Task<List<string>> GetUserRoleNamesAsync(string userId)
        {
            return await _userAccountsRepository.GetUserRoleNamesAsync(userId);
        }
    }
}