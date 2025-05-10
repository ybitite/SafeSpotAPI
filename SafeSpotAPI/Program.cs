using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeSpotAPI.Data;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

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

// Configuration du type MIME pour les fichiers .3gp
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m4a"] = "audio/aac";

// Enable static file serving
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider // Utilisation du ContentTypeProvider
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads",
    ContentTypeProvider = provider // Utilisation du ContentTypeProvider
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

// Test de connexion à la base de données (après la configuration du pipeline)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    try
    {
        dbContext.Database.CanConnect();
        logger.LogInformation("Connexion à la base de données réussie !");
    }
    catch (Exception ex)
    {
        logger.LogError($"Erreur de connexion à la base : {ex.Message}");
    }
}

app.Run();