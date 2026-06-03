using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Models;
using SheSecure.WellnessSafetyService.Entities;


public class WellnessDbContext : DbContext
{
    public WellnessDbContext(DbContextOptions<WellnessDbContext> options)
        : base(options)
    {
    }

    public DbSet<WellnessRequest> WellnessRequests { get; set; }


    public DbSet<EmergencyAlert> EmergencyAlerts { get; set; }

    public DbSet<MoodLog> MoodLogs { get; set; }
}           