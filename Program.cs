using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Website_Idea.Data;
using Website_Idea.Filters;
using Website_Idea.Services;

var builder = WebApplication.CreateBuilder(args);

const long uploadLimit = 40 * 1024 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = uploadLimit);
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = uploadLimit);
builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = uploadLimit);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IImageStorage, ImageStorage>();
builder.Services.AddScoped<LoadCompanyInfoFilter>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/manage/login";
        options.AccessDeniedPath = "/manage/login";
        options.Cookie.Name = "Atelier.Manage";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = async context =>
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var current = ManagePinStamp.For(config["ManagePin"]);
            if (context.Principal?.FindFirst(ManagePinStamp.ClaimType)?.Value != current)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews(options => options.Filters.AddService<LoadCompanyInfoFilter>());

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "uploads", "products"));
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "uploads", "gallery"));

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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
