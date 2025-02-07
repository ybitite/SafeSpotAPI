using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeSpotAPI.Data;
using SafeSpotAPI.Models;
using System.Collections.Generic;

namespace SafeSpotAPI.Controllers
{

    [Route("api/reports")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly ReportDbContext _db;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ReportDbContext contextDb, ILogger<ReportsController> logger)
        {
            _db = contextDb;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
            return await _db.Reports.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null)
                return NotFound();
            return report;
        }

        [HttpPost]
        public async Task<ActionResult<Report>> AddReport([FromBody] Report report)
        {
            if (report == null)
                return BadRequest("Le rapport ne peut pas être vide");

            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }

        public class ValidateRequest
        {
            public string Comment { get; set; }
        }

        [HttpPost("validate/{id}")]
        public async Task<IActionResult> ValidateReport(int id, [FromForm] ValidateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Comment))
                return BadRequest("Le commentaire est requis pour la validation.");

            // Recherche du rapport dans la table Reports
            var report = await _db.Reports.FindAsync(id);
            if (report == null)
                return NotFound();

            // Création du rapport validé
            var validatedReport = new ReportValidated
            {
                Longitude = report.Longitude,
                Latitude = report.Latitude,
                Date_Time = report.Date_Time,
                Description = report.Description,
                Image = report.Image,
                Video = report.Video,
                Comment = request.Comment,
                Date_Time_Validation = DateTime.UtcNow
            };

            // Ajouter le rapport validé dans la table ValidatedReports
            _db.ValidatedReports.Add(validatedReport);


            // Supprimer le rapport de la table Reports
            _db.Reports.Remove(report);

            // Sauvegarder les modifications dans la base de données
            await _db.SaveChangesAsync();

            // Redirection vers la page d'administration après validation
            return Redirect("https://localhost:7095/admin");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null)
                return NotFound();

            _db.Reports.Remove(report);
            await _db.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("DeleteValidated/{id}")]
        public async Task<IActionResult> DeleteReportValidated(int id)
        {
            var reportValidated = await _db.ValidatedReports.FindAsync(id);
            if(reportValidated == null)
                return NotFound();

            _db.ValidatedReports.Remove(reportValidated);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("admin")]
        public async Task<IActionResult> AdminPanel()
        {
            var viewModel = new AdminViewModel
            {
                PendingReports = await _db.Reports.ToListAsync(),
                ValidatedReports = await _db.ValidatedReports.ToListAsync()
            };

            return View(viewModel);
        }
    }
}
