using Microsoft.EntityFrameworkCore;
using SafeSpotAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    try
    {
        dbContext.Database.CanConnect();
        Console.WriteLine("Connexion à la base de données réussie !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur de connexion à la base : {ex.Message}");
    }
}
app.UseHttpsRedirection();

app.UseStaticFiles();
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

app.Run();
