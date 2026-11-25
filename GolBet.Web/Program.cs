using GolBet.Entities;
using GolBet.Web.Identity;
using Microsoft.AspNetCore.Identity;
using GolBet.Repositories.Data;
using GolBet.Repositories.Implementations;
using GolBet.Repositories.Interfaces;
using GolBet.Services.Implementations;
using GolBet.Services.Interfaces;
using GolBet.Services.Mapping;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;

var culture = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

// Serilog replaces the default logger and reads its settings from the "Serilog" section of appsettings.json
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new GolBet.Web.ModelBinding.FlexibleDecimalModelBinderProvider());
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(name => $"{name} debe ser un número.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, name) => $"El valor de {name} no es válido.");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddErrorDescriber<SpanishIdentityErrorDescriber>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
});

// Open generic registration: one line, a repository for every entity
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Specific repositories
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IBetRepository, BetRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();

// AutoMapper: scans the assembly containing MappingProfile for all profiles
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Business services
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IBetService, BetService>();

var app = builder.Build();

// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    await DbSeeder.SeedAsync(context, roleManager, userManager, app.Environment.IsDevelopment());
}

// Configure the HTTP request pipeline.
// Unhandled exceptions (500): friendly page outside development,
// full stack trace while developing
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/General");
    app.UseHsts();
}

// Status codes without a body (404 and friends): re-execute
// the pipeline against ErrorController
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// The HTTP profile is loopback-only and works without a development certificate.
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseStaticFiles();

// One clean line per request: method, path, status and duration
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
