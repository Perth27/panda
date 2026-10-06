using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Pandora.Models.ViewModels;
using Pandora.Services;

namespace Pandora.Controllers
{
    public class UserAccountsController : Controller
    {
        private readonly IUserAccountsService _userAccountsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public UserAccountsController(
            IUserAccountsService userAccountsService, 
            IHttpContextAccessor httpContextAccessor, 
            IHostEnvironment environment, 
            IConfiguration configuration)
        {
            _userAccountsService = userAccountsService;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            if (_environment.IsDevelopment() || _environment.IsStaging())
            {
                var productionUrl = _configuration["productionUrl"];
                ViewBag.EnvironmentMessage = $"This is a Non-Production environment. Please click on <a href=\"{productionUrl}\">{productionUrl}</a> to be directed to the Production environment.";
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.Domain = "CORP";
            var basePath = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
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
                var result = await _userAccountsService.ProcessLoginAsync(model);
                
                if (result.IsSuccess)
                {
                    return Redirect(returnUrl);
                }

                if (result.ExceptionDetails != null)
                {
                    ModelState.AddModelError(string.Empty, result.ExceptionDetails.Message);
                }
            }

            ViewBag.MessageError = "Incorrect Username or Password. Please try again.";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
            await _userAccountsService.SignOutAsync();
            
            var basePath = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            var returnUrl = basePath + "/ConsolidatedBackups/Index";
            
            return Redirect(returnUrl);
        }
    }
}