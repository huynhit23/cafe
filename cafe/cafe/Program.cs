using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ KẾT NỐI DATABASE
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Sem3Connection")
    ));

// ✅ IDENTITY
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ✅ Cookie --- admin redirect riêng, user redirect riêng
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/UserAccount/Login";
    options.AccessDeniedPath = "/UserAccount/AccessDenied";
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/Admin"))
            ctx.Response.Redirect("/Admin/Account/Login?returnUrl=" + Uri.EscapeDataString(ctx.Request.Path));
        else
            ctx.Response.Redirect("/UserAccount/Login?returnUrl=" + Uri.EscapeDataString(ctx.Request.Path));
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/Admin"))
            ctx.Response.Redirect("/Admin/Account/AccessDenied");
        else
            ctx.Response.Redirect("/UserAccount/AccessDenied");
        return Task.CompletedTask;
    };
});

// ✅ Session cho giỏ hàng
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ VNPay Service
builder.Services.AddScoped<cafe.Services.IVnPayService, cafe.Services.VnPayService>();

// ✅ Email Service
builder.Services.AddScoped<cafe.Services.IEmailService, cafe.Services.EmailService>();

// MVC & SignalR
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews(options =>
{
    options.MaxModelBindingCollectionSize = int.MaxValue;
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();

// ✅ SEED admin + roles
using (var scope = app.Services.CreateScope())
{
    await SeedAdminAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<cafe.Hubs.OrderHub>("/orderHub");
app.Run();

static async Task SeedAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { "Admin", "User" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    const string adminEmail = "admin@cafe.com";
    const string adminPassword = "Admin@123";

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Administrator", EmailConfirmed = true };
        var r = await userManager.CreateAsync(admin, adminPassword);
        if (r.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
    }
    else if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}