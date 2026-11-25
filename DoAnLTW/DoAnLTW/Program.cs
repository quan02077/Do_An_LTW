using Microsoft.EntityFrameworkCore;
using DoAnLTW.Data;
using DoAnLTW.Services;
using DoAnLTW.Areas.Admin.Services;
using DoAnLTW.Hubs;
using DoAnLTW.Services.Chatbot;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// tiện cho môi trường phát triển: đọc khóa API (và cấu hình cục bộ khác) từ file .env cạnh Program.cs.
// .env nằm trong .gitignore nên không bị commit; xem .env.example để biết cách dùng.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddDotEnvFile(Path.Combine(builder.Environment.ContentRootPath, ".env"));
}

// add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DbContext_TuiXach>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IHomeRender, HomeRender>();
builder.Services.AddSingleton<IProductPricingService, ProductPricingService>();
builder.Services.AddScoped<ICurrentCustomerService, CurrentCustomerService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IFlashSaleService, CatalogService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
// email xác nhận đơn: EmailService gửi SMTP; OrderEmailQueue chạy nền để bước đặt hàng không phải chờ SMTP
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<OrderEmailQueue>();
builder.Services.AddSingleton<IOrderEmailQueue>(sp => sp.GetRequiredService<OrderEmailQueue>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<OrderEmailQueue>());
// thông báo đơn hàng thời gian thực (SignalR): NotificationService bọc IHubContext, lỗi gửi không làm hỏng nghiệp vụ.
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();

// chat thời gian thực giữa khách hàng và nhân viên (SupportChatHub): lưu lịch sử trong CSDL, giới hạn tốc độ gửi tin theo tài khoản.
builder.Services.AddScoped<ISupportChatService, SupportChatService>();
builder.Services.AddSingleton<SupportChatRateGate>();

// chatbot tư vấn túi: truy xuất sản phẩm từ CSDL rồi nhờ Groq (openai/gpt-oss-20b) viết câu tư vấn.
// khóa API đặt bằng user-secrets (Chatbot:Groq:ApiKey); thiếu khóa hoặc AI lỗi thì tự dùng gợi ý từ CSDL.
// token chống giả mạo (CSRF) cho các lời gọi fetch/JSON: gửi trong header thay vì trường form.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
builder.Services.Configure<ChatbotOptions>(builder.Configuration.GetSection(ChatbotOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ChatLlmGate>();
// Groq là mô hình chính, Gemma (OpenRouter) dự phòng; ChatModelChain thử lần lượt các mô hình đã có khóa.
builder.Services.AddHttpClient<GroqChatClient>();
builder.Services.AddHttpClient<OpenRouterChatClient>();
builder.Services.AddScoped<IChatModelClient>(sp => new ChatModelChain(
    [sp.GetRequiredService<GroqChatClient>(), sp.GetRequiredService<OpenRouterChatClient>()],
    sp.GetRequiredService<ILogger<ChatModelChain>>()));
builder.Services.AddScoped<IChatProductSearch, ChatProductSearch>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

// giới hạn tốc độ riêng cho /api/chat (khách chưa đăng nhập cũng gọi được): theo tên người dùng, không có thì theo IP.
var chatbotOptions = builder.Configuration.GetSection(ChatbotOptions.SectionName).Get<ChatbotOptions>() ?? new ChatbotOptions();
builder.Services.AddRateLimiter(limiter =>
{
    limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    limiter.AddPolicy("chat", httpContext => RateLimitPartition.GetSlidingWindowLimiter(
        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        factory: _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = chatbotOptions.PerSessionPerMinute,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0
        }));
    limiter.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "10";
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Bạn gửi hơi nhanh, vui lòng thử lại sau ít giây." }, token);
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Events.OnRedirectToLogin = context =>
        {
            // API và hub SignalR là kênh gọi bằng script: trả 401 thay vì chuyển hướng sang trang đăng nhập HTML.
            if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            // đã đăng nhập nhưng không đủ quyền (ví dụ khách gọi API của nhân viên): script cần mã 403, không phải trang HTML.
            if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
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
// mô hình 3D (.glb/.gltf) dùng cho khung xem túi xoay 360° ở trang chủ: mặc định ASP.NET không biết kiểu file này nên trả 404.
var staticFileTypes = new FileExtensionContentTypeProvider();
staticFileTypes.Mappings[".glb"] = "model/gltf-binary";
staticFileTypes.Mappings[".gltf"] = "model/gltf+json";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = staticFileTypes });

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// hub thông báo thời gian thực (yêu cầu đăng nhập bằng cookie, xem [Authorize] trong NotificationHub)
app.MapHub<NotificationHub>("/hubs/notifications");
// khung chat khách hàng - nhân viên (đăng nhập bắt buộc, xem [Authorize] trong SupportChatHub)
app.MapHub<SupportChatHub>("/hubs/support-chat");

// 1. route cho area admin: /admin => Areas/Admin/Controllers/HomeController.cs
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 2. route mac dinh cho user: / => Controllers/HomeController.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
