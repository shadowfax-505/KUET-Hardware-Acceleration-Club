using KUETHardwareAccelerationClub.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var appDataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataPath);
var identityDbPath = Path.Combine(appDataPath, "identity.db");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite($"Data Source={identityDbPath}");
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Auth";
    options.AccessDeniedPath = "/Account/Auth";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var roles = new[] { "Admin", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@hack.kuet.ac.bd";
    var adminPassword = app.Configuration["Seed:AdminPassword"]
        ?? (app.Environment.IsDevelopment() ? "Admin@123" : null);
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null && adminPassword is not null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var adminCreate = await userManager.CreateAsync(adminUser, adminPassword);
        if (adminCreate.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    var demoMembers = app.Environment.IsDevelopment()
        ? new[]
        {
            (Email: "member1@kuet.ac.bd", Password: "member123"),
            (Email: "member2@kuet.ac.bd", Password: "member123"),
            (Email: "member3@kuet.ac.bd", Password: "member123")
        }
        : Array.Empty<(string Email, string Password)>();

    foreach (var demoMember in demoMembers)
    {
        var memberUser = await userManager.FindByEmailAsync(demoMember.Email);
        if (memberUser is null)
        {
            memberUser = new IdentityUser
            {
                UserName = demoMember.Email,
                Email = demoMember.Email,
                EmailConfirmed = true
            };

            var memberCreate = await userManager.CreateAsync(memberUser, demoMember.Password);
            if (!memberCreate.Succeeded)
            {
                continue;
            }
        }

        if (!await userManager.IsInRoleAsync(memberUser, "Member"))
        {
            await userManager.AddToRoleAsync(memberUser, "Member");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var httpsPortConfigured =
    !string.IsNullOrWhiteSpace(builder.Configuration["HTTPS_PORT"]) ||
    !string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_HTTPS_PORT"]);

if (app.Environment.IsDevelopment() || httpsPortConfigured)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();