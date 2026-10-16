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
    public IActionResult Register(string? returnUrl) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(request);

        var trimmedUsername = request.TenDangNhap.Trim();
        var trimmedEmail = request.Email.Trim();

        // 1. Kiểm tra trùng tên đăng nhập
        var usernameExists = await db.TaiKhoans.AnyAsync(x => x.tenDangNhap.ToLower() == trimmedUsername.ToLower(), ct);
        if (usernameExists)
        {
            ModelState.AddModelError(nameof(request.TenDangNhap), "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác.");
            return View(request);
        }

        // 2. Kiểm tra trùng email
        var emailExists = await db.KhachHangs.AnyAsync(x => x.email != null && x.email.ToLower() == trimmedEmail.ToLower(), ct);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(request.Email), "Địa chỉ email này đã được đăng ký. Vui lòng chọn email khác hoặc đăng nhập.");
            return View(request);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var role = request.VaiTro == Roles.Admin ? Roles.Admin : Roles.Customer;

            // 3. Tạo tài khoản đăng nhập (taikhoan)
            var account = new TaiKhoan
            {
                tenDangNhap = trimmedUsername,
                matKhau = request.MatKhau,
                vaiTro = role
            };
            db.TaiKhoans.Add(account);
            await db.SaveChangesAsync(ct);

            // 4. Tạo hồ sơ khách hàng (khachhang) liên kết với tài khoản
            var customer = new KhachHang
            {
                matk = account.maTK,
                hoTen = request.HoTen.Trim(),
                email = trimmedEmail,
                sdt = request.SoDienThoai.Trim(),
                diaChi = !string.IsNullOrWhiteSpace(request.DiaChi) ? request.DiaChi.Trim() : "TP. Hồ Chí Minh"
            };
            db.KhachHangs.Add(customer);
            await db.SaveChangesAsync(ct);

            // 5. Khởi tạo giỏ hàng (giohang) cho khách hàng
            var cart = new GioHang
            {
                maKH = customer.maKH,
                ngayTao = DateTime.Now
            };
            db.GioHangs.Add(cart);
            await db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            // 6. Đăng nhập tự động ngay sau khi đăng ký
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.maTK.ToString()),
                new(ClaimTypes.Name, account.tenDangNhap),
                new(ClaimTypes.Role, account.vaiTro)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["AuthSuccess"] = $"Chúc mừng {customer.hoTen}! Bạn đã đăng ký tài khoản thành công.";

            if (!string.IsNullOrWhiteSpace(request.ReturnUrl) && Url.IsLocalUrl(request.ReturnUrl))
            {
                return LocalRedirect(request.ReturnUrl);
            }

            return role == Roles.Admin
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            ModelState.AddModelError(string.Empty, "Lỗi tạo tài khoản: " + ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
