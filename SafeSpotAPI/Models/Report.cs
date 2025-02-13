using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SafeSpotAPI.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public DateTime Date_Time { get; set; }
        public string? Description { get; set; } // Make Description nullable
        public string? Image { get; set; } // Make Image nullable
        public string? Video { get; set; } // Make Video nullable
    }
}
