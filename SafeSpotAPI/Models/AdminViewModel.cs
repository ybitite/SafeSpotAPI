using System.Collections.Generic;

namespace SafeSpotAPI.Models
{
    public class AdminViewModel
    {
        public List<Report> PendingReports { get; set; }
        public List<ReportValidated> ValidatedReports { get; set; }
    }
}
