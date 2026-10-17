using GolBet.Entities;
using GolBet.Repositories.Data;
using GolBet.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GolBet.Web.Controllers;

public class AccountController(UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager, ILogger<AccountController> logger) : Controller
{
    private const decimal WelcomeBalance = 100_000m;

    [HttpGet, AllowAnonymous]
    public IActionResult Register()
        => User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Matches") : View(new RegisterViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Matches");
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim(),
            // Neither the starting balance nor the role comes from the form.
            Balance = WelcomeBalance
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(user, DbSeeder.BettorRole);
        if (!roleResult.Succeeded)
        {
            // Do not sign in an incomplete account or leave it blocking another attempt.
            var cleanup = await userManager.DeleteAsync(user);
            logger.LogError("Bettor role assignment failed. Account cleanup succeeded: {Succeeded}", cleanup.Succeeded);
            ModelState.AddModelError("", "No pudimos completar el registro. Inténtalo de nuevo.");
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = $"¡Bienvenido, {user.FullName}! Recibiste 100.000 FutCoins para empezar.";
        return RedirectToAction("Index", "Matches");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Matches");
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        return View(new LoginViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Matches");
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(model.Email.Trim(), model.Password,
            model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Matches");

        ModelState.AddModelError("", "Correo o contraseña incorrectos.");
        return View(model);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
    }
}
