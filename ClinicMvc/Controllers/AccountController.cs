using System.Security.Claims;
using ClinicMvc.Models;
using ClinicMvc.Repositories;
using ClinicMvc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

/// <summary>
/// Контролер за најава и одјава на корисници.
/// Користи ASP.NET Core Cookie Authentication (не JWT, бидејќи ова е класична MVC веб апликација).
/// </summary>
public class AccountController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AccountController(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>GET: /Account/Login - ја прикажува формата за најава.</summary>
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    /// <summary>
    /// POST: /Account/Login
    /// Ги проверува внесените податоци, и ако се точни, креира автентикациско cookie
    /// со Claims (Username, Role, DoctorId) кои понатаму се читаат низ целата апликација.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userRepository.GetByUsernameAsync(model.Username);

        // Намерно иста порака за "нема корисник" и "погрешна лозинка" -
        // не откриваме дали корисничкото ime постои (безбедносна практика)
        if (user == null || !_passwordHasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Погрешно корисничко ime или лозинка.");
            return View(model);
        }

        // Градиме ги Claims - податоци кои ќе бидат достапни низ целата апликација
        // преку User.Identity и User.Claims во контролерите/View-ovите
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        // DoctorId се додава само ако корисникот е поврзан со доктор
        if (user.DoctorId.HasValue)
        {
            claims.Add(new Claim("DoctorId", user.DoctorId.Value.ToString()));
        }

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Го креираме автентикациското cookie - IsPersistent = false значи
        // сесијата истекува кога прелистувачот ќе се затвори
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = false });

        // Пренасочи го корисникот кон страницата од каде дошол, или кон почетна
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    /// <summary>POST: /Account/Logout - ја брише автентикациската сесија.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    /// <summary>GET: /Account/AccessDenied - страница за "немате пристап".</summary>
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
