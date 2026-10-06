using System.Collections.Generic;
using System.Threading.Tasks;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface IUserAccountsService
    {
        Task<(object User, List<string> Roles)> AuthenticateAndGetRolesAsync(string username);
    }

    public class UserAccountsService : IUserAccountsService
    {
        private readonly IUserAccountsRepository _userAccountsRepository;

        public UserAccountsService(IUserAccountsRepository userAccountsRepository)
        {
            _userAccountsRepository = userAccountsRepository;
        }

        public async Task<(object User, List<string> Roles)> AuthenticateAndGetRolesAsync(string username)
        {
            return await _userAccountsRepository.GetUserWithRolesAsync(username);
        }
    }
}