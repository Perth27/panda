using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pandora.Models;
using Pandora.ViewModels;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface IUsersService
    {
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserDetailsAsync(string id);
        UserVM GetUserCreateVM();
        UserVM GetRepopulatedUserCreateVM(UserVM user);
        Task<(bool IsSuccess, User NewUser, string ErrorMessage)> CreateUserAsync(UserVM user);
        Task<User> GetUserForEditAsync(string id);
        Task UpdateUserAsync(UserVM user);
        Task<User> GetUserForDeleteAsync(string id);
        Task DeleteUserAsync(string id);
        bool UserExists(string id);
    }

    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly IUserExtension _userExtension;

        public UsersService(IUsersRepository repository, UserManager<User> userManager, IUserExtension userExtension)
        {
            _repository = repository;
            _userManager = userManager;
            _userExtension = userExtension;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _repository.GetAllUsersAsync();
        }

        public async Task<User> GetUserDetailsAsync(string id)
        {
            return await _repository.GetUserByIdAsync(id);
        }

        public UserVM GetUserCreateVM()
        {
            var model = new UserVM();
            var roles = _repository.GetAllRoles();
            foreach (var role in roles)
            {
                model.RoleList.Add(new SelectListItem() { Text = role.Name, Value = role.Name });
            }
            return model;
        }

        public UserVM GetRepopulatedUserCreateVM(UserVM user)
        {
            var roles = _repository.GetAllRoles();
            foreach (var UserRole in roles)
            {
                user.RoleList.Add(new SelectListItem() { Text = UserRole.Name, Value = UserRole.Name });
            }
            return user;
        }

        public async Task<(bool IsSuccess, User NewUser, string ErrorMessage)> CreateUserAsync(UserVM user)
        {
            User newUser = await _userExtension.LookupAdUser(user.Logon);
            if (newUser == null)
            {
                return (false, null, null);
            }

            User existingUser = _repository.GetExistingUserByLogon(newUser.Logon);
            if (existingUser != null)
            {
                return (false, null, "User already exists!");
            }

            string userPWD = "Vision1!";
            var createNewUser = await _userManager.CreateAsync(newUser, userPWD);
            if (createNewUser.Succeeded)
            {
                //here we tie the new user to the role : Question 3
                await _userManager.AddToRoleAsync(newUser, user.RoleName);
                return (true, newUser, null);
            }

            return (false, newUser, null);
        }

        public async Task<User> GetUserForEditAsync(string id)
        {
            return await _repository.FindUserByIdAsync(id);
        }

        public async Task UpdateUserAsync(UserVM user)
        {
            _repository.UpdateUser(user);
            await _repository.SaveChangesAsync();
        }

        public async Task<User> GetUserForDeleteAsync(string id)
        {
            return await _repository.GetUserByIdAsync(id);
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _repository.FindUserByIdAsync(id);
            if (user != null)
            {
                _repository.RemoveUser(user);
                await _repository.SaveChangesAsync();
            }
        }

        public bool UserExists(string id)
        {
            return _repository.UserExists(id);
        }
    }
}