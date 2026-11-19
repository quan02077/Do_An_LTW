using System.Security.Claims;
using DoAnLTW.Models;
using DoAnLTW.Services;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

// controller chỉ điều phối: kiểm tra ModelState, gọi service, đặt TempData và redirect.
// toàn bộ truy cập CSDL và quy tắc nghiệp vụ tài khoản nằm trong AccountService.
public class AccountController(IAccountService accountService, ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(request);

        var account = await accountService.ValidateCredentialsAsync(request.TenDangNhap, request.MatKhau, ct);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(request);
        }

        await SignInAsync(account);

        // đăng nhập thành công: tất cả người dùng (kể cả admin) đều về trang home.
        // riêng tài khoản admin sẽ có nút chuyển sang trang quản trị trên header và trang chủ.
        return RedirectToLocal(request.ReturnUrl);
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

        try
        {
            var result = await accountService.RegisterAsync(request, ct);
            if (!result.Success)
            {
                ModelState.AddModelError(result.Field ?? string.Empty, result.Error!);
                return View(request);
            }

            // đăng nhập tự động ngay sau khi đăng ký
            await SignInAsync(result.Value!);
            TempData["AuthSuccess"] = $"Chúc mừng {request.HoTen.Trim()}! Bạn đã đăng ký tài khoản thành công.";
            return RedirectToLocal(request.ReturnUrl);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // chi tiết lỗi chỉ ghi vào log cho lập trình viên; người dùng chỉ thấy câu thông báo chung
            // (ex.Message có thể lộ tên bảng, cột, cấu trúc CSDL).
            logger.LogError(ex, "Lỗi khi đăng ký tài khoản {TenDangNhap}", request.TenDangNhap);
            ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản, vui lòng thử lại sau.");
            return View(request);
        }
    }

    // hồ sơ khách hàng (customer profile)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        if (!TryGetAccountId(out var maTk)) return await ForceLoginAsync();

        var profile = await accountService.GetProfileAsync(maTk, ct);
        if (profile is null) return await ForceLoginAsync();

        return View(profile);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel request, CancellationToken ct)
    {
        if (!TryGetAccountId(out var maTk)) return await ForceLoginAsync();

        if (ModelState.IsValid)
        {
            var result = await accountService.UpdateProfileAsync(maTk, request, ct);
            if (result.Success)
            {
                TempData["ProfileSuccess"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError(result.Field ?? string.Empty, result.Error!);
        }

        // hiển thị lại form: các trường chỉ đọc lấy từ CSDL, không tin giá trị client gửi lên.
        var current = await accountService.GetProfileAsync(maTk, ct);
        if (current is null) return await ForceLoginAsync();

        request.MaKH = current.MaKH;
        request.TenDangNhap = current.TenDangNhap;
        request.VaiTro = current.VaiTro;
        request.SoDonHang = current.SoDonHang;
        request.TongChiTieu = current.TongChiTieu;
        return View(request);
    }

    // đổi mật khẩu từ trang hồ sơ
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["PasswordError"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                ?? "Dữ liệu đổi mật khẩu không hợp lệ.";
            return RedirectToAction(nameof(Profile));
        }

        if (!TryGetAccountId(out var maTk)) return await ForceLoginAsync();

        var result = await accountService.ChangePasswordAsync(maTk, request.MatKhauHienTai, request.MatKhauMoi, ct);
        if (!result.Success)
        {
            TempData["PasswordError"] = result.Error;
            return RedirectToAction(nameof(Profile));
        }

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

        var result = await accountService.ResetPasswordAsync(request, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(result.Field ?? string.Empty, result.Error!);
            return View(request);
        }

        TempData["AuthSuccess"] = $"Đặt lại mật khẩu thành công cho tài khoản '{result.Value}'. Vui lòng đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string? username) => RedirectToAction(nameof(ForgotPassword), new { username });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ResetPassword(ForgotPasswordViewModel request, CancellationToken ct) => ForgotPassword(request, ct);

    [HttpGet]
    public IActionResult AccessDenied() => View();

    // dùng chung cho Login và Register: chỉ một role duy nhất lấy từ cột vaiTro.
    private async Task SignInAsync(TaiKhoan account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.maTK.ToString()),
            new(ClaimTypes.Name, account.tenDangNhap),
            new(ClaimTypes.Role, account.vaiTro)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    private bool TryGetAccountId(out int maTk) =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out maTk);

    private async Task<IActionResult> ForceLoginAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // chỉ chuyển hướng tới URL nội bộ để chống open redirect.
    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
}
