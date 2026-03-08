using Ecommerce_Datos.Datos;
using Ecommerce_Datos.Datos.Repositorio;
using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce.Application;
using Ecommerce_Utilidades;
using Ecommerce_Utilidades.BrainTree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

Console.WriteLine("[Startup] Process started.");
var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("[Startup] Builder created.");

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

Console.WriteLine("[Startup] Registering services...");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaulConnection")));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddApplication();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<ITipoAplicacionRepositorio, TipoAplicacionRepositorio>();
builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<IUsuarioAplicacionRepositorio, UsuarioAplicacionRepositorio>();
builder.Services.AddScoped<IOrdenRepositorio, OrdenRepositorio>();
builder.Services.AddScoped<IOrdenDetalleRepositorio, OrdenDetalleRepositorio>();
builder.Services.AddScoped<IVentaRepositorio, VentaRepositorio>();
builder.Services.AddScoped<IVentaDetalleRepositorio, VentaDetalleRepositorio>();

builder.Services.Configure<BrainTreeSettings>(builder.Configuration.GetSection("BrainTree"));
builder.Services.AddSingleton<IBrainTreeGate, BrainTreeGate>();

var facebookSection = builder.Configuration.GetSection("Authentication:Facebook");
string? facebookAppId = facebookSection["AppId"];
string? facebookAppSecret = facebookSection["AppSecret"];
if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
{
    builder.Services.AddAuthentication().AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = facebookAppSecret;
    });
}
Console.WriteLine("[Startup] Service registration completed.");

var app = builder.Build();
app.Logger.LogInformation("App built successfully.");
Console.WriteLine("[Startup] App built.");

if (app.Environment.IsDevelopment() &&
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")) &&
    string.IsNullOrWhiteSpace(app.Configuration["urls"]) &&
    app.Urls.Count == 0)
{
    const string defaultHttpUrl = "http://localhost:5005";
    app.Urls.Add(defaultHttpUrl);
    app.Logger.LogInformation("ASPNETCORE_URLS is not set. Defaulting Development URL to {Url}.", defaultHttpUrl);
}

app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

bool useHttpsRedirection = !app.Environment.IsDevelopment() ||
                           app.Configuration.GetValue<bool>("UseHttpsRedirectionInDevelopment");
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
    app.Logger.LogInformation("HTTPS redirection enabled.");
}
else
{
    app.Logger.LogInformation("HTTPS redirection disabled for Development. HTTP is the default.");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.Logger.LogInformation("Starting development seed initialization.");
try
{
    using CancellationTokenSource seedTimeoutCts = new(TimeSpan.FromSeconds(30));
    await Ecommerce.SeedData.InitializeAsync(app.Services, app.Environment, seedTimeoutCts.Token);
    app.Logger.LogInformation("Seed initialization finished.");
}
catch (OperationCanceledException)
{
    app.Logger.LogError("Seed initialization timed out after 30 seconds. Continuing startup.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Seed initialization failed. Continuing startup.");
}

app.MapRazorPages();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Lifetime.ApplicationStarted.Register(() =>
{
    string urls = app.Urls.Count > 0 ? string.Join(", ", app.Urls) : "no urls reported";
    app.Logger.LogInformation("Application started. Listening on: {Urls}", urls);
    Console.WriteLine($"[Startup] Application started. Listening on: {urls}");
});

app.Logger.LogInformation("Entering app.Run().");
Console.WriteLine("[Startup] Entering app.Run().");
app.Run();
