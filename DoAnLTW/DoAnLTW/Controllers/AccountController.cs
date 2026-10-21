using System.Security.Claims;
using DoAnLTW.Data;
using DoAnLTW.Models;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

        // kiểm tra trùng tên đăng nhập
        var usernameExists = await db.TaiKhoans.AnyAsync(x => x.tenDangNhap.ToLower() == trimmedUsername.ToLower(), ct);
        if (usernameExists)
        {
            ModelState.AddModelError(nameof(request.TenDangNhap), "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác.");
            return View(request);
        }

        // kiểm tra trùng email
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

            // tạo tài khoản đăng nhập (taikhoan) - identity tự tăng matk
            var account = new TaiKhoan
            {
                tenDangNhap = trimmedUsername,
                matKhau = request.MatKhau,
                vaiTro = role
            };
            db.TaiKhoans.Add(account);
            await db.SaveChangesAsync(ct);

            // tạo hồ sơ khách hàng (khachhang) - identity tự tăng makh
            var customer = new KhachHang
            {
                maTK = account.maTK,
                hoTen = request.HoTen.Trim(),
                email = trimmedEmail,
                sdt = request.SoDienThoai.Trim(),
                diaChi = !string.IsNullOrWhiteSpace(request.DiaChi) ? request.DiaChi.Trim() : "TP. Hồ Chí Minh"
            };
            db.KhachHangs.Add(customer);
            await db.SaveChangesAsync(ct);

            // khởi tạo giỏ hàng (giohang) cho khách hàng - identity tự tăng magiohang
            var cart = new GioHang
            {
                maKH = customer.maKH,
                ngayTao = DateTime.Now
            };
            db.GioHangs.Add(cart);
            await db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            // đăng nhập tự động ngay sau khi đăng ký
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

    // hồ sơ khách hàng (customer profile)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var accountValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(accountValue, out var maTk))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var account = await db.TaiKhoans
            .Include(x => x.KhachHang)
            .ThenInclude(kh => kh!.DonHangs)
            .FirstOrDefaultAsync(x => x.maTK == maTk, ct);

        if (account == null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var customer = account.KhachHang;
        if (customer == null)
        {
            customer = new KhachHang
            {
                maTK = account.maTK,
                hoTen = account.tenDangNhap,
                email = $"{account.tenDangNhap}@gmail.com",
                sdt = "0900000000",
                diaChi = "TP. Hồ Chí Minh"
            };
            db.KhachHangs.Add(customer);
            await db.SaveChangesAsync(ct);
        }

        var vm = new ProfileViewModel
        {
            MaKH = customer.maKH,
            TenDangNhap = account.tenDangNhap,
            VaiTro = account.vaiTro,
            HoTen = customer.hoTen,
            Email = customer.email ?? string.Empty,
            SoDienThoai = customer.sdt ?? string.Empty,
            DiaChi = customer.diaChi ?? string.Empty,
            SoDonHang = customer.DonHangs?.Count ?? 0,
            TongChiTieu = customer.DonHangs?.Where(d => d.trangThai != "Đã hủy").Sum(d => d.tongTien) ?? 0
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel request, CancellationToken ct)
    {
        var accountValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(accountValue, out var maTk))
        {
            return RedirectToAction(nameof(Login));
        }

        var account = await db.TaiKhoans
            .Include(x => x.KhachHang)
            .ThenInclude(kh => kh!.DonHangs)
            .FirstOrDefaultAsync(x => x.maTK == maTk, ct);

        if (account == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var customer = account.KhachHang;
        if (customer == null)
        {
            customer = new KhachHang { maTK = account.maTK };
            db.KhachHangs.Add(customer);
        }

        var trimmedEmail = request.Email.Trim();
        var emailExists = await db.KhachHangs.AnyAsync(
            x => x.maTK != maTk && x.email != null && x.email.ToLower() == trimmedEmail.ToLower(), ct);

        if (emailExists)
        {
            ModelState.AddModelError(nameof(request.Email), "Địa chỉ email này đã được sử dụng bởi một tài khoản khác.");
        }

        if (!ModelState.IsValid)
        {
            request.MaKH = customer.maKH;
            request.TenDangNhap = account.tenDangNhap;
            request.VaiTro = account.vaiTro;
            request.SoDonHang = customer.DonHangs?.Count ?? 0;
            request.TongChiTieu = customer.DonHangs?.Where(d => d.trangThai != "Đã hủy").Sum(d => d.tongTien) ?? 0;
            return View(request);
        }

        customer.hoTen = request.HoTen.Trim();
        customer.email = trimmedEmail;
        customer.sdt = request.SoDienThoai.Trim();
        customer.diaChi = !string.IsNullOrWhiteSpace(request.DiaChi) ? request.DiaChi.Trim() : null;

        await db.SaveChangesAsync(ct);

        TempData["ProfileSuccess"] = "Cập nhật thông tin cá nhân thành công!";
        return RedirectToAction(nameof(Profile));
    }

    // đổi mật khẩu từ trang hồ sơ
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu đổi mật khẩu không hợp lệ.";
            TempData["PasswordError"] = error;
            return RedirectToAction(nameof(Profile));
        }

        var accountValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(accountValue, out var maTk))
        {
            return RedirectToAction(nameof(Login));
        }

        var account = await db.TaiKhoans.FirstOrDefaultAsync(x => x.maTK == maTk, ct);
        if (account == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var verified = false;
        if (account.matKhau == request.MatKhauHienTai)
        {
            verified = true;
        }
        else
        {
            try
            {
                verified = new PasswordHasher<TaiKhoan>().VerifyHashedPassword(account, account.matKhau, request.MatKhauHienTai) != PasswordVerificationResult.Failed;
            }
            catch
            {
                verified = false;
            }
        }

        if (!verified)
        {
            TempData["PasswordError"] = "Mật khẩu hiện tại không chính xác.";
            return RedirectToAction(nameof(Profile));
        }

        account.matKhau = request.MatKhauMoi;
        await db.SaveChangesAsync(ct);

        TempData["ProfileSuccess"] = "Đổi mật khẩu thành công! Mật khẩu mới đã được áp dụng.";
        return RedirectToAction(nameof(Profile));
    }

    // quên mật khẩu & đặt lại mật khẩu
    [HttpGet]
    public IActionResult ForgotPassword(string? username) => View(new ForgotPasswordViewModel { TenDangNhap = username ?? string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(request);

        var trimmedUsername = request.TenDangNhap.Trim();
        var trimmedEmail = request.Email.Trim();

        var account = await db.TaiKhoans
            .Include(t => t.KhachHang)
            .FirstOrDefaultAsync(x => x.tenDangNhap.ToLower() == trimmedUsername.ToLower(), ct);

        if (account == null)
        {
            ModelState.AddModelError(nameof(request.TenDangNhap), "Tên đăng nhập không tồn tại trong hệ thống.");
            return View(request);
        }

        var customer = account.KhachHang ?? await db.KhachHangs.FirstOrDefaultAsync(k => k.maTK == account.maTK, ct);
        if (customer == null || string.IsNullOrWhiteSpace(customer.email))
        {
            ModelState.AddModelError(nameof(request.Email), "Tài khoản này chưa có email xác minh trong hệ thống.");
            return View(request);
        }

        if (!string.Equals(customer.email.Trim(), trimmedEmail, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(request.Email), "Địa chỉ email không khớp với email đã đăng ký của tài khoản này.");
            return View(request);
        }

        account.matKhau = request.MatKhauMoi;
        await db.SaveChangesAsync(ct);

        TempData["AuthSuccess"] = $"Đặt lại mật khẩu thành công cho tài khoản '{account.tenDangNhap}'. Vui lòng đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string? username) => RedirectToAction(nameof(ForgotPassword), new { username });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ResetPassword(ForgotPasswordViewModel request, CancellationToken ct) => ForgotPassword(request, ct);

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
