using SheSecure.DashboardService.DTOs;
using SheSecure.DashboardService.Interfaces;

namespace SheSecure.DashboardService.Services
{
    public class DashboardService : IDashboardService
    {
        public async Task<DashboardStatsDTO> GetStatsAsync()
        {
            return await Task.FromResult(
                new DashboardStatsDTO
                {
                    TotalComplaints = 10,
                    OpenComplaints = 3,
                    ResolvedComplaints = 7,
                    WellnessRequests = 5,
                    ActiveEmergencyAlerts = 1,
                    NotificationsSent = 20
                });
        }

        public async Task<List<ComplaintAnalyticsDTO>>
            GetComplaintAnalyticsAsync()
        {
            return await Task.FromResult(
                new List<ComplaintAnalyticsDTO>
                {
                    new()
                    {
                        Category = "Workplace Harassment",
                        Count = 5
                    },
                    new()
                    {
                        Category = "Bullying",
                        Count = 3
                    },
                    new()
                    {
                        Category = "Discrimination",
                        Count = 2
                    }
                });
        }

        public async Task<List<WellnessAnalyticsDTO>>
            GetWellnessAnalyticsAsync()
        {
            return await Task.FromResult(
                new List<WellnessAnalyticsDTO>
                {
                    new()
                    {
                        RequestType = "Counselling",
                        Count = 4
                    },
                    new()
                    {
                        RequestType = "WFH",
                        Count = 2
                    }
                });
        }

        public async Task<List<EmergencyAnalyticsDTO>>
            GetEmergencyAnalyticsAsync()
        {
            return await Task.FromResult(
                new List<EmergencyAnalyticsDTO>
                {
                    new()
                    {
                        Status = "Active",
                        Count = 1
                    },
                    new()
                    {
                        Status = "Resolved",
                        Count = 6
                    }
                });
        }
    }
}