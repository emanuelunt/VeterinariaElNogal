using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VeterinariaGestion.Infrastructure.Services;

namespace VeterinariaGestion.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _auth;

    public AccountController(IAuthService auth) => _auth = auth;

    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var usuario = await _auth.LoginAsync(username, password);
        if (usuario is null)
        {
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name,           usuario.NombreUsuario),
            new(ClaimTypes.Email,          usuario.Email)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccesoDenegado() => View();
}