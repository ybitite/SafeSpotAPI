using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportsController(ReportDbContext contextDb, ILogger<ReportsController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _db = contextDb;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportValidated>>> GetReports()
        {
            return await _db.ValidatedReports.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportValidated>> GetReport(int id)
        {
            var reportValidated = await _db.ValidatedReports.FindAsync(id);
            if (reportValidated == null)
                return NotFound();
            return reportValidated;
        }

        [HttpPost]
        public async Task<ActionResult<Report>> AddReport([FromForm] Report report, IFormFile? image, IFormFile? audio)
        {
            _logger.LogInformation("AddReport action called.");

            if (report == null)
            {
                _logger.LogError("AddReport: report is null.");
                return BadRequest("Le rapport ne peut pas être vide");
            }

            _logger.LogInformation($"AddReport: Report data - Description: {report.Description}, Longitude: {report.Longitude}, Latitude: {report.Latitude}, Date_Time: {report.Date_Time}");

            // Gestion de l'image
            if (image != null && image.Length > 0)
            {
                _logger.LogInformation($"AddReport: Image received - FileName: {image.FileName}, Length: {image.Length}");

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving image to disk.");
                    return StatusCode(500, "Error saving image.");
                }

                report.Image = fileName;
            }
            else
            {
                _logger.LogInformation("AddReport: No image received.");
            }

            // Gestion de l'audio
            if (audio != null && audio.Length > 0)
            {
                _logger.LogInformation($"AddReport: Audio received - FileName: {audio.FileName}, Length: {audio.Length}");

                var audioFileName = Guid.NewGuid().ToString() + Path.GetExtension(audio.FileName);
                var audioFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "uploads", audioFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(audioFilePath));

                try
                {
                    using (var fileStream = new FileStream(audioFilePath, FileMode.Create))
                    {
                        await audio.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving audio to disk.");
                    return StatusCode(500, "Error saving audio.");
                }

                report.Audio = audioFileName;
            }
            else
            {
                _logger.LogInformation("AddReport: No audio received.");
            }

            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"AddReport: Report added successfully - ID: {report.Id}");

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
                Audio = report.Audio,
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
            return Redirect("https://safespotapi20250207214631.azurewebsites.net/admin");
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
            if (reportValidated == null)
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