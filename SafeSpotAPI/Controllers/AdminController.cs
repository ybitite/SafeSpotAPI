using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SafeSpotAPI.Models;  
using SafeSpotAPI.Data;    // Pour accéder au DbContext

namespace SafeSpotAPI.Controllers
{
    public class AdminController : Controller
    {
        private readonly ReportDbContext _context;

        public AdminController(ReportDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminViewModel
            {
                PendingReports = await _context.Reports.ToListAsync(),
                ValidatedReports = await _context.ValidatedReports.ToListAsync()
            };

            return View("AdminPanel", viewModel); 
        }
    }
}
