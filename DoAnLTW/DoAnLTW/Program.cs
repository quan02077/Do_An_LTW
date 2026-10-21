using Microsoft.EntityFrameworkCore;
using DoAnLTW.Data;
using DoAnLTW.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DbContext_TuiXach>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IHomeRender, HomeRender>();
builder.Services.AddSingleton<IProductPricingService, ProductPricingService>();
builder.Services.AddScoped<ICurrentCustomerService, CurrentCustomerService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

// chỉ môi trường phát triển tự áp dụng migration và nạp dữ liệu demo.
if (app.Environment.IsDevelopment())
{
    await DbInitializer.InitializeAsync(app.Services);
}

// configure the http request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // the default hsts value is 30 days. you may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "catalog_html",
    pattern: "catalog.html",
    defaults: new { controller = "Catalog", action = "Index" });

app.MapControllerRoute(
    name: "productDetail_html",
    pattern: "productDetail.html",
    defaults: new { controller = "Catalog", action = "Detail" });

app.MapControllerRoute(
    name: "flashSale_html",
    pattern: "flashSale.html",
    defaults: new { controller = "FlashSale", action = "Index" });

app.MapControllerRoute(
    name: "admin_html",
    pattern: "admin.html",
    defaults: new { controller = "Admin", action = "Index" });

app.MapControllerRoute(
    name: "home_html",
    pattern: "homePage.html",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "login_html",
    pattern: "login.html",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "register_html",
    pattern: "register.html",
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "profile_html",
    pattern: "profile.html",
    defaults: new { controller = "Account", action = "Profile" });

app.MapControllerRoute(
    name: "forgot_password_html",
    pattern: "forgot-password.html",
    defaults: new { controller = "Account", action = "ForgotPassword" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
