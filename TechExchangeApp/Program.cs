using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// Increase form value limits for CKEditor content with embedded images
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = 50 * 1024 * 1024;   // 50 MB per value
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB total
});

// Increase Kestrel max request body size
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});
builder.Services.AddMemoryCache();
builder.Services.AddScoped<TechExchangeApp.Services.IWorkflowService, TechExchangeApp.Services.WorkflowService>();
builder.Services.AddScoped<TechExchangeApp.Services.INotificationQueueService, TechExchangeApp.Services.NotificationQueueService>();

// Cho phép lấy HttpContext trong Razor
builder.Services.AddHttpContextAccessor();

// --- Session cần đăng ký ---
builder.Services.AddDistributedMemoryCache(); // Lưu session trên memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.UseCompatibilityLevel(120)
    )
);


// --- Configuration ---
builder.Services.Configure<TechExchangeApp.Helpers.AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<TechExchangeApp.Configuration.SiteBrandingOptions>(builder.Configuration.GetSection(TechExchangeApp.Configuration.SiteBrandingOptions.SectionName));
builder.Services.Configure<TechExchangeApp.Configuration.FeatureFlags>(builder.Configuration.GetSection("FeatureFlags"));
builder.Services.Configure<TechExchangeApp.Configuration.DashboardJobOptions>(builder.Configuration.GetSection(TechExchangeApp.Configuration.DashboardJobOptions.SectionName));
builder.Services.Configure<TechExchangeApp.Configuration.OtpSettings>(builder.Configuration.GetSection(TechExchangeApp.Configuration.OtpSettings.SectionName));
builder.Services.Configure<TechExchangeApp.Configuration.AiChatOptions>(builder.Configuration.GetSection(TechExchangeApp.Configuration.AiChatOptions.SectionName));

// In-process memory cache (used by HomeAnalyticsService)
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// --- Identity Configuration ---
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Configure Identity options
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false; 
    options.SignIn.RequireConfirmedAccount = false;
})
.AddSignInManager<SignInManager<ApplicationUser>>() // Add SignInManager for cookie handling
.AddUserStore<TechExchangeApp.Services.ApplicationUserStore>() // Use Custom Store
.AddDefaultTokenProviders();

// Add Authentication Cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/dang-nhap";
    options.LogoutPath = "/dang-ky"; // Or logout logic
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "TechExchangeApp.Identity";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;

    // For AJAX/fetch requests: return 401 instead of redirecting to login page
    options.Events.OnRedirectToLogin = ctx =>
    {
        var isAjax = ctx.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                  || ctx.Request.Headers["Accept"].ToString().Contains("application/json");
        if (isAjax)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{\"success\":false,\"message\":\"Unauthorized\"}");
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// --- CMS Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CmsAccess", policy =>
        policy.Requirements.Add(new TechExchangeApp.Authorization.CmsAdminRequirement()));
});
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, TechExchangeApp.Authorization.CmsAdminHandler>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ICmsAccessService, TechExchangeApp.Services.CmsAccessService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ICntbMasterService, TechExchangeApp.Services.CntbMasterService>();

// --- Services ---
builder.Services.AddScoped<TechExchangeApp.Services.IExcelExportService, TechExchangeApp.Services.ExcelExportService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IProductService, TechExchangeApp.Services.ProductService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IDashboardService, TechExchangeApp.Services.DashboardService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IAdminDashboardService, TechExchangeApp.Services.AdminDashboardService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IHomeAnalyticsService, TechExchangeApp.Services.HomeAnalyticsService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IAccountService, TechExchangeApp.Services.AccountService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IProjectService, TechExchangeApp.Services.ProjectService>();

// Chat System
builder.Services.AddScoped<TechExchangeApp.Interfaces.IChatService, TechExchangeApp.Services.ChatService>();
builder.Services.AddScoped<TechExchangeApp.Services.IAiChatService, TechExchangeApp.Services.AiChatService>();
builder.Services.AddScoped<TechExchangeApp.Services.IAiKnowledgeService, TechExchangeApp.Services.AiKnowledgeService>();
builder.Services.AddScoped<TechExchangeApp.Services.IAiFeedbackService, TechExchangeApp.Services.AiFeedbackService>();
builder.Services.AddHttpClient<TechExchangeApp.Services.IOpenAiClientService, TechExchangeApp.Services.OpenAiClientService>();

// Generic Entity Action Engine
builder.Services.AddScoped<TechExchangeApp.Services.IEntityOwnershipResolver, TechExchangeApp.Services.EntityOwnershipResolver>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IEntityActionService, TechExchangeApp.Services.EntityActionService>();

// E-Sign Services
builder.Services.AddScoped<TechExchangeApp.Interfaces.IESignGateway, TechExchangeApp.Services.ESignGateway>();

// Permission System
builder.Services.AddScoped<TechExchangeApp.Interfaces.IPermissionService, TechExchangeApp.Services.PermissionService>();

// Seller Workflow Services
builder.Services.AddScoped<TechExchangeApp.Interfaces.IInvitationService, TechExchangeApp.Services.InvitationService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IProposalService, TechExchangeApp.Services.ProposalService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ISelectionService, TechExchangeApp.Services.SelectionService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IScoringService, TechExchangeApp.Services.ScoringService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IProjectMemberService, TechExchangeApp.Services.ProjectMemberService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IOtpEmailService, TechExchangeApp.Services.OtpEmailService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ISmsSender, TechExchangeApp.Services.StubSmsSender>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ILegalReviewService, TechExchangeApp.Services.LegalReviewService>();

// Step 6+7: Contract & Digital Signing Services
builder.Services.AddScoped<TechExchangeApp.Interfaces.IHashService,              TechExchangeApp.Services.HashService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IContractAuditService,     TechExchangeApp.Services.ContractAuditService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IContractApprovalService,  TechExchangeApp.Services.ContractApprovalService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IContractService,          TechExchangeApp.Services.ContractService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IContractSigningService,   TechExchangeApp.Services.ContractSigningService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IVerificationService,      TechExchangeApp.Services.VerificationService>();
builder.Services.AddScoped<TechExchangeApp.Services.PdfSigningService>();  // iText7 visible signature embedding
builder.Services.AddScoped<TechExchangeApp.Services.HtmlToPdfService>();   // iText7 HTML→PDF for contracts without uploaded file

// CA signing provider adapters (resolved by name via factory)
builder.Services.AddScoped<TechExchangeApp.Interfaces.ISigningProvider, TechExchangeApp.Services.Signing.VnptSigningProvider>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ISigningProvider, TechExchangeApp.Services.Signing.FptSigningProvider>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ISigningProvider, TechExchangeApp.Services.Signing.ViettelSigningProvider>();
builder.Services.AddScoped<TechExchangeApp.Services.ISigningProviderFactory, TechExchangeApp.Services.SigningProviderFactory>();
builder.Services.AddHttpClient(); // IHttpClientFactory for CA provider adapters (stub auto-callback, real API calls)


// --- AI Semantic Matching Services ---
builder.Services.AddMemoryCache();

// Infrastructure layer
builder.Services.AddHttpClient<TechExchangeApp.Infrastructure.AI.IEmbeddingService, TechExchangeApp.Infrastructure.AI.OpenAIEmbeddingService>();
builder.Services.AddScoped<TechExchangeApp.Infrastructure.Repositories.IEmbeddingRepository, TechExchangeApp.Infrastructure.Repositories.EmbeddingRepository>();
builder.Services.AddScoped<TechExchangeApp.Infrastructure.Repositories.ISearchLogRepository, TechExchangeApp.Infrastructure.Repositories.SearchLogRepository>();

// Application layer
builder.Services.AddScoped<TechExchangeApp.Application.Services.IAISupplierMatchingService, TechExchangeApp.Application.Services.AISupplierMatchingService>();
builder.Services.AddScoped<TechExchangeApp.Application.Services.IProductEmbeddingService, TechExchangeApp.Application.Services.ProductEmbeddingService>();
builder.Services.AddScoped<TechExchangeApp.Application.Services.ISearchService, TechExchangeApp.Application.Services.SearchService>();

// Background service
builder.Services.AddHostedService<TechExchangeApp.BackgroundServices.ProductEmbeddingUpdaterService>();

// --- Notification System ---
builder.Services.AddScoped<TechExchangeApp.Interfaces.ISystemParameterService, TechExchangeApp.Services.SystemParameterService>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.IEmailSender, TechExchangeApp.Services.GmailEmailSender>();
builder.Services.AddScoped<TechExchangeApp.Interfaces.ISmsSender, TechExchangeApp.Services.StubSmsSender>(); // Twilio replaced with stub — add Twilio back when SMS is needed
builder.Services.AddScoped<TechExchangeApp.Interfaces.INotificationProcessor, TechExchangeApp.Services.NotificationProcessor>();
builder.Services.AddHostedService<TechExchangeApp.BackgroundServices.NotificationWorker>();
builder.Services.AddHostedService<TechExchangeApp.BackgroundServices.DashboardBackgroundService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
// ⚠️ TẠM THỜI BẬT DEBUG — nhớ revert lại sau khi tìm xong lỗi
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseDeveloperExceptionPage(); // 👈 DEBUG tạm thời
    // app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int cacheSeconds = 60 * 60 * 24 * 30;
        ctx.Context.Response.Headers.CacheControl = $"public,max-age={cacheSeconds}";
    }
});

app.UseRouting();

// --- Bắt buộc: UseSession phải nằm ở đây ---
app.UseSession();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub
app.MapHub<TechExchangeApp.Hubs.NotificationHub>("/notificationHub");


// --- CMS Area Route (MUST be before all other routes) ---
app.MapAreaControllerRoute(
    name: "cms",
    areaName: "Cms",
    pattern: "cms/{controller=Dashboard}/{action=Index}/{id?}"
);

// --- Custom Routes (Moved from Controllers) ---

// 1. Product Routes — each URL maps to its own dedicated action
app.MapControllerRoute(
    name: "product_cong_nghe",
    pattern: "cong-nghe",
    defaults: new { controller = "Product", action = "CongNghe" }
);

app.MapControllerRoute(
    name: "product_thiet_bi",
    pattern: "thiet-bi",
    defaults: new { controller = "Product", action = "ThietBi" }
);

app.MapControllerRoute(
    name: "product_sp_tri_tue",
    pattern: "tai-san-tri-tue",
    defaults: new { controller = "Product", action = "TaiSanTriTue" }
);


app.MapControllerRoute(
    name: "product_detail",
    pattern: "{menu:int}-cong-nghe-thiet-bi/{typeId:int}/{slug}-{id:int}",
    defaults: new { controller = "Product", action = "Detail" }
);

app.MapControllerRoute(
    name: "product_category",
    pattern: "2-ds-cong-nghe-thiet-bi/{slug}-{cateId:int}",
    defaults: new { controller = "Product", action = "ProductByCate" }
);

app.MapControllerRoute(
    name: "product_add_cart",
    pattern: "cart/add/{id:int}",
    defaults: new { controller = "Product", action = "AddToCart" }
);

// 2. TimKiemDoiTac Routes
app.MapControllerRoute(
    name: "tkdt_index_clean",
    pattern: "tim-kiem-doi-tac",
    defaults: new { controller = "TimKiemDoiTac", action = "Index" }
);

app.MapControllerRoute(
    name: "tkdt_index",
    pattern: "tim-kiem-doi-tac-11",
    defaults: new { controller = "TimKiemDoiTac", action = "Index" }
);

app.MapControllerRoute(
    name: "tkdt_detail",
    pattern: "11-tim-kiem-doi-tac/{slug}-{id}",
    defaults: new { controller = "TimKiemDoiTac", action = "Detail" }
);

app.MapControllerRoute(
    name: "tkdt_category",
    pattern: "11-ds-tim-kiem-doi-tac/{slug}-{cateId}",
    defaults: new { controller = "TimKiemDoiTac", action = "List" }
);

// 3. TiemLucKHCN Routes
app.MapControllerRoute(
    name: "tiemluc_index",
    pattern: "tiem-luc-KHCN",
    defaults: new { controller = "TiemLucKHCN", action = "Index" }
);

// 4. NhuCauCongNghe Routes
app.MapControllerRoute(
    name: "nhucau_dang",
    pattern: "yeu-cau-cong-nghe-67",
    defaults: new { controller = "Nhucaucongnghe", action = "CateTechNeeds", menuId = 67 }
);

app.MapControllerRoute(
    name: "nhucau_detail",
    pattern: "{menuId:int}/yeu-cau/{slug}-{id:int}",
    defaults: new { controller = "Nhucaucongnghe", action = "Detail" }
);

// 5. News Routes
app.MapControllerRoute(
    name: "news_menu",
    pattern: "{queryString:regex(^(tin-su-kien|hoi-thao-trinh-dien-cong-nghe|bao-cao-phan-tich-xu-huong-cong-nghe|giai-phap-cong-nghe|mo-hinh-cong-nghe)$)}-{menuId:int}",
    defaults: new { controller = "News", action = "Category" }
);

app.MapControllerRoute(
    name: "news_detail",
    pattern: "{menuId:int}/{queryString}-{id:long}",
    defaults: new { controller = "News", action = "Detail" }
);

// 6. Menus Routes (Gioi thieu, quy dinh)
app.MapControllerRoute(
    name: "menu_detail",
    pattern: "{queryString:regex(^gioi-thieu-chung|quy-dinh-chung$)}-{menuId:int}",
    defaults: new { controller = "Menu", action = "Detail" }
);

// 7. Forum Routes
app.MapControllerRoute(
    name: "forum_index",
    pattern: "thao-luan",
    defaults: new { controller = "Forum", action = "Index" }
);

app.MapControllerRoute(
    name: "forum_detail",
    pattern: "chi-tiet-thao-luan-{id}",
    defaults: new { controller = "Forum", action = "Detail" }
);

app.MapControllerRoute(
    name: "forum_category",
    pattern: "thao-luan-{linhvuc:int}-{parentid:int}",
    defaults: new { controller = "Forum", action = "Index" }
);

app.MapControllerRoute(
    name: "feedback_index",
    pattern: "lien-he-74",
    defaults: new { controller = "Feedback", action = "Index" }
);

// 8. Auth Routes
app.MapControllerRoute(
    name: "login_page",
    pattern: "dang-nhap",
    defaults: new { controller = "Account", action = "Login" }
);

app.MapControllerRoute(
    name: "register_page",
    pattern: "dang-ky",
    defaults: new { controller = "Account", action = "Register" }
);


// 9. ChuyenGia Routes
app.MapControllerRoute(
    name: "chuyen_gia_index",
    pattern: "chuyen-gia",
    defaults: new { controller = "ChuyenGia", action = "Index" }
);

app.MapControllerRoute(
    name: "nha_cung_ung_index",
    pattern: "nha-cung-ung",
    defaults: new { controller = "NhaCungUng", action = "Index" }
);

// 301 Redirect — old DichVuTuVan list pages
app.MapGet("8-dich-vu-tu-van", ctx =>
{
    ctx.Response.Redirect("/chuyen-gia", permanent: true);
    return Task.CompletedTask;
});
app.MapGet("8-dich-vu-cung-ung", ctx =>
{
    ctx.Response.Redirect("/nha-cung-ung", permanent: true);
    return Task.CompletedTask;
});

// Video routes — /video và /video.html cùng trỏ vào VideoController.Index
app.MapControllerRoute(
    name: "video_clean",
    pattern: "video",
    defaults: new { controller = "Video", action = "Index" }
);

// 10. Dashboard Routes (public)
app.MapControllerRoute(
    name: "contract_dashboard",
    pattern: "hop-dong-ky-ket",
    defaults: new { controller = "Home", action = "ContractDashboard" }
);

app.MapControllerRoute(
    name: "connection_dashboard",
    pattern: "ket-noi-cung-cau",
    defaults: new { controller = "Home", action = "ConnectionDashboard" }
);

app.MapControllerRoute(
    name: "dang_ky_nha_cung_ung",
    pattern: "dang-ky-nha-cung-ung",
    defaults: new { controller = "DangKyNhaCungUng", action = "DangKy" }
);

app.MapControllerRoute(
    name: "dang_ky_tu_van",
    pattern: "dang-ky-tu-van",
    defaults: new { controller = "DangKyTuVan", action = "DangKy" }
);

app.MapControllerRoute(
    name: "quan_ly_san_pham",
    pattern: "quan-ly-san-pham",
    defaults: new { controller = "QuanLySanPham", action = "Index" }
);

app.MapControllerRoute(
    name: "quan_ly_san_pham_cong_nghe_tao_moi",
    pattern: "quan-ly-san-pham/cong-nghe/tao-moi",
    defaults: new { controller = "QuanLySanPham", action = "TaoMoiCongNghe" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_thiet_bi_tao_moi",
    pattern: "quan-ly-san-pham/thiet-bi/tao-moi",
    defaults: new { controller = "QuanLySanPham", action = "TaoMoiThietBi" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_sohuu_tao_moi",
    pattern: "quan-ly-san-pham/so-huu-tri-tue/tao-moi",
    defaults: new { controller = "QuanLySanPham", action = "TaoMoiSoHuuTriTue" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_tao_moi_post",
    pattern: "quan-ly-san-pham/tao-moi",
    defaults: new { controller = "QuanLySanPham", action = "TaoMoi" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_cong_nghe_chinh_sua",
    pattern: "quan-ly-san-pham/cong-nghe/chinh-sua/{id:int}",
    defaults: new { controller = "QuanLySanPham", action = "ChinhSua" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_thiet_bi_chinh_sua",
    pattern: "quan-ly-san-pham/thiet-bi/chinh-sua/{id:int}",
    defaults: new { controller = "QuanLySanPham", action = "ChinhSua" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_sohuu_chinh_sua",
    pattern: "quan-ly-san-pham/so-huu-tri-tue/chinh-sua/{id:int}",
    defaults: new { controller = "QuanLySanPham", action = "ChinhSua" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_chinh_sua_post",
    pattern: "quan-ly-san-pham/chinh-sua/{id:int}",
    defaults: new { controller = "QuanLySanPham", action = "ChinhSua" }
);
app.MapControllerRoute(
    name: "quan_ly_san_pham_xoa",
    pattern: "quan-ly-san-pham/xoa",
    defaults: new { controller = "QuanLySanPham", action = "Xoa" }
);

// ROUTE MẶC ĐỊNH

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
