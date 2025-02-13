
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeSpotAPI.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// Add ILogger
builder.Services.AddLogging();

// Add IWebHostEnvironment
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

var app = builder.Build();

// Get ILogger
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Enable static file serving
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "admin",
    defaults: new { controller = "Reports", action = "AdminPanel" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reports}/{action=Index}/{id?}");

app.MapControllers();

// Test de connexion � la base de donn�es (apr�s la configuration du pipeline)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    try
    {
        dbContext.Database.CanConnect();
        logger.LogInformation("Connexion � la base de donn�es r�ussie !");
    }
    catch (Exception ex)
    {
        logger.LogError($"Erreur de connexion � la base : {ex.Message}");
    }
}

app.Run();