using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Models;

namespace SheSecure.Safety_WellnessService.DTOs
{
    public class CreateMoodLogDTO
    {
        public string EmployeeId { get; set; }

        public int MoodLevel { get; set; }

        public int StressLevel { get; set; }

        public string? Remarks { get; set; }

        public DbSet<MoodLog> MoodLogs { get; set; }
    }
}