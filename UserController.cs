private readonly Security.IAuthenticationService _authService;
private readonly IUserAccountsService _userAccountsService; // Inject your new service
private readonly ISignInManager _signInManager;
private readonly IHttpContextAccessor _httpContextAccessor;
private readonly IHostEnvironment _environment;
private readonly IConfiguration _configuration;

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
                // Fetch user and roles cleanly through the service layer instead of _context
                var (currentUser, userRolesNames) = await _userAccountsService.AuthenticateAndGetRolesAsync(model.Username);

                if (currentUser != null)
                {
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