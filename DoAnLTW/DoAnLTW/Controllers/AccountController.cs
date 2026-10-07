using System.Security.Claims;
using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Controllers;

public class AccountController(DbContext_TuiXach db) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(request);

        var account = await db.TaiKhoans.SingleOrDefaultAsync(x => x.tenDangNhap == request.TenDangNhap, ct);
        var verified = false;
        if (account is not null)
        {
            // Hỗ trợ cả mật khẩu thường (nhập tay từ SQL) lẫn mật khẩu đã mã hóa (PasswordHasher)
            if (account.matKhau == request.MatKhau)
            {
                verified = true;
            }
            else
            {
                try
                {
                    verified = new PasswordHasher<TaiKhoan>().VerifyHashedPassword(account, account.matKhau, request.MatKhau) != PasswordVerificationResult.Failed;
                }
                catch
                {
                    verified = false;
                }
            }
        }
        if (!verified)
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(request);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account!.maTK.ToString()),
            new(ClaimTypes.Name, account.tenDangNhap),
            new(ClaimTypes.Role, account.vaiTro)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return !string.IsNullOrWhiteSpace(request.ReturnUrl) && Url.IsLocalUrl(request.ReturnUrl)
            ? LocalRedirect(request.ReturnUrl)
            : RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
