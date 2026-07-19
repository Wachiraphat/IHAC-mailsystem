using FinalProject.Areas.Identity.Data;
using FinalProject.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var isVercelDemo = builder.Environment.IsEnvironment("Docker");

// Vercel containers have an ephemeral writable /tmp directory. This mode is
// intentionally suitable for a portfolio demo only; data can reset at any time.
if (isVercelDemo && !string.IsNullOrEmpty(connectionString) &&
    connectionString.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    connectionString = "Data Source=/tmp/FinalProjectDemo.db";
}

builder.Services.AddDbContext<FinalProjectContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString) && connectionString.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("IHAC-MailSystem");

if (isVercelDemo || builder.Configuration["DEMO_KEY_GENERATION"] == "true")
{
    var keyPath = builder.Configuration["DEMO_KEY_PATH"] ?? "/app/demo-keys";
    dataProtection
        .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
        .SetDefaultKeyLifetime(TimeSpan.FromDays(3650));
}
else
{
    dataProtection.PersistKeysToDbContext<FinalProjectContext>();
}
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();

builder.Services.AddDefaultIdentity<FinalProjectUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<FinalProjectContext>();
builder.Services.AddRazorPages();

var app = builder.Build();

// The container build calls the application once to create a shared key ring.
// Every Vercel instance receives the same read-only keys in the final image.
if (builder.Configuration["DEMO_KEY_GENERATION"] == "true")
{
    var provider = app.Services.GetRequiredService<IDataProtectionProvider>();
    provider.CreateProtector("IHAC portfolio demo key initialization")
        .Protect("initialize");
    return;
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FinalProjectContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
