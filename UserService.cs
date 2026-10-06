using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pandora.Models.ViewModels;
using Pandora.Repositories;
using Pandora.Security;

namespace Pandora.Services
{
    public interface IUserAccountsService
    {
        Task<(bool IsSuccess, string ErrorMessage, Exception ExceptionDetails)> ProcessLoginAsync(LoginViewModel model);
        Task SignOutAsync();
    }

    public class UserAccountsService : IUserAccountsService
    {
        private readonly IUserAccountsRepository _repository;
        private readonly Security.IAuthenticationService _authService;
        private readonly ISignInManager _signInManager;

        public UserAccountsService(
            IUserAccountsRepository repository, 
            Security.IAuthenticationService authService, 
            ISignInManager signInManager)
        {
            _repository = repository;
            _authService = authService;
            _signInManager = signInManager;
        }

        public async Task<(bool IsSuccess, string ErrorMessage, Exception ExceptionDetails)> ProcessLoginAsync(LoginViewModel model)
        {
            try
            {
                var user = await _authService.Login(model.Username.ToUpper(), model.Password, model.Domain);
                if (user != null)
                {
                    User currentUser = _repository.GetUserByLogon(model.Username);
                    
                    if (currentUser != null)
                    {
                        var userRolesNames = _repository.GetUserRolesNames(currentUser.Id);
                        await _signInManager.SignInAsync(model.Username.ToUpper(), userRolesNames);
                        return (true, null, null);
                    }

                    List<string> rolesList = new List<string>();
                    await _signInManager.SignInAsync(model.Username.ToUpper(), rolesList);
                    return (true, null, null);
                }
                
                return (false, "Incorrect Username or Password. Please try again.", null);
            }
            catch (Exception ex)
            {
                return (false, null, ex);
            }
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}