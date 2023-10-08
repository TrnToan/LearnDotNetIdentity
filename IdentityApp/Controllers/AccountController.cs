using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Register(string returnUrl = null!)
    {
        var registerVM = new RegisterViewModel();
        registerVM.ReturnUrl = returnUrl;
        return View(registerVM);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl)
    {
        returnUrl ??= Url.Content("~/");
        model.ReturnUrl = returnUrl;
        if (ModelState.IsValid)
        {
            var user = new AppUser(model.FullName, model.Address)
            {
                UserName = model.UserName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false); // once the web page closed down, user have to relog in
                return LocalRedirect(returnUrl);
            }
            ModelState.AddModelError("Email", "User could not be created. Password not unique enough");
        }
        var errors = ModelState.Select(x => x.Value.Errors)
            .Where(y => y.Count > 0)
            .ToList();
        return View(model);
    }
}
