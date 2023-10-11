using IdentityApp.Models;
using IdentityApp.Services;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly SendMailkitService _service;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, SendMailkitService service)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _service = service;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
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

    [HttpGet]
    public IActionResult SignIn(string? returnUrl)
    {
        SignInViewModel model = new SignInViewModel();
        returnUrl ??= Url.Content("~/");
        model.ReturnUrl = returnUrl;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError("error", "Invalid Sign In attempt.");
                return View(model);
            }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignOut()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
    {
        if(ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(viewModel.Email);
            if(user == null)
            {
                return View(viewModel);
            }
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new {userId = user.Id, token = resetToken }, HttpContext.Request.Scheme);

            var mailContent = new MailContent()
            {
                To = viewModel.Email,
                Subject = "Password Reset",
                Body = "Please reset email by going to this " + "<a href=\"" + callbackUrl + "\">link</a>"
            };
            await _service.SendMail(mailContent);
            return RedirectToAction("ForgotPasswordConfirmation");
        }
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        return token == null ? View("Error") : View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "User not found");
                return View();
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token, resetPasswordViewModel.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }
        }
        var errors = ModelState.Select(x => x.Value.Errors)
            .Where(y => y.Count > 0)
            .ToList();
        return View(resetPasswordViewModel);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}
