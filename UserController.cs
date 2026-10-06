using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Pandora.Services;
using Pandora.Models;

namespace Pandora.Controllers
{
    public class UserAccountsController : Controller
    {
        private readonly Security.IAuthenticationService _authService;
        private readonly IUserAccountsService _userAccountsService;
        private readonly ISignInManager _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        // Base path or standard variables if used in your app
        private readonly string basePath = ""; 

        public UserAccountsController(
            Security.IAuthenticationService authService,
            IUserAccountsService userAccountsService,
            ISignInManager signInManager,
            IHttpContextAccessor httpContextAccessor,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            _authService = authService;
            _userAccountsService = userAccountsService;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _configuration = configuration;
        }

        // GET: UserAccounts/Login
        public IActionResult Login()
        {
            if (_environment.IsDevelopment() || _environment.IsStaging())
            {
                var productionUrl = _configuration["productionUrl"];
                ViewBag.EnvironmentMessage = $"This is a Non-Production environment. Please click on <a href=\"{productionUrl}\">{productionUrl}</a> to be directed to the Production environment.";
            }
            return View();
        }

        // POST: UserAccounts/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var returnUrl = basePath + "/ConsolidatedBackups/Index";
            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                if (model.ReturnUrl.Length > 1)
                {
                    returnUrl = basePath + model.ReturnUrl;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _authService.Login(model.Username.ToUpper(), model.Password, model.Domain);
                    if (user != null)
                    {
                        var currentUser = (User)await _userAccountsService.GetUserByLogonAsync(model.Username.ToUpper());
                        if (currentUser != null)
                        {
                            var userRolesNames = await _userAccountsService.GetUserRoleNamesAsync(currentUser.Id);

                            await _signInManager.SignInAsync(model.Username.ToUpper(), userRolesNames);
                            return Redirect(returnUrl);
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Incorrect Username or Password. Please try again.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model);
        }

        // POST: UserAccounts/SignOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();

            var basePathDynamic = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            var returnUrl = basePathDynamic + "/ConsolidatedBackups/Index";

            return Redirect(returnUrl);
        }
    }
}