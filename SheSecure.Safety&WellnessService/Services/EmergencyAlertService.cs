using SheSecure.Safety_WellnessService.DTOs.Requests;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;
using System.Text;
using System.Text.Json;

namespace SheSecure.Safety_WellnessService.Services
{
    public class EmergencyAlertService : IEmergencyAlertService
    {
        private readonly IEmergencyAlertRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmergencyAlertService(
            IEmergencyAlertRepository repository,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<EmergencyAlert> CreateAlertAsync(
            CreateEmergencyAlertDTO dto)
        {
            var alert = new EmergencyAlert
            {
                EmployeeId = dto.EmployeeId,
                Location = dto.Location,
                Description = dto.Description,
                Severity = dto.Severity,
                Status = "Active",
                TriggeredAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAlertAsync(alert);

            var message =
                $"SOS Alert triggered by Employee ID {dto.EmployeeId}. " +
                $"Location: {dto.Location}. Severity: {dto.Severity}. " +
                $"Please respond immediately.";

            // Notify Security team
            await SendNotificationAsync(
                employeeId: "3",
                title: "🚨 SOS Alert - Security Action Required",
                message: message,
                type: "Emergency"
            );

            // Notify HR
            await SendNotificationAsync(
                employeeId: "2",
                title: "🚨 SOS Alert - HR Notification",
                message: message,
                type: "Emergency"
            );

            // Notify Admin
            await SendNotificationAsync(
                employeeId: "1",
                title: "🚨 SOS Alert - Admin Notice",
                message: message,
                type: "Emergency"
            );

            // Confirm to employee that SOS was received
            await SendNotificationAsync(
                employeeId: dto.EmployeeId.ToString(),
                title: "SOS Received",
                message: "Your SOS alert has been received. Help is on the way.",
                type: "Emergency"
            );

            return created;
        }

        public async Task<List<EmergencyAlert>> GetAllAlertsAsync()
        {
            return await _repository.GetAllAlertsAsync();
        }

        public async Task<EmergencyAlert?> GetAlertByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task ResolveAlertAsync(
            ResolveEmergencyAlertDTO dto)
        {
            var alert = await _repository.GetByIdAsync(dto.AlertId);

            if (alert == null)
                throw new Exception("Alert not found");

            alert.Status = "Resolved";
            alert.ResolvedAt = DateTime.UtcNow;

            await _repository.UpdateAlertAsync(alert);

            // Notify employee that alert was resolved
            await SendNotificationAsync(
                employeeId: alert.EmployeeId.ToString(),
                title: "SOS Alert Resolved",
                message: $"Your SOS alert has been resolved. Stay safe.",
                type: "Emergency"
            );
        }

        // ── Helper ────────────────────────────────────────────────────────

        private async Task SendNotificationAsync(
            string employeeId, string title,
            string message, string type)
        {
            try
            {
                var client = _httpClientFactory
                    .CreateClient("NotificationService");

                var payload = JsonSerializer.Serialize(new
                {
                    employeeId,
                    title,
                    message,
                    type
                });

                var content = new StringContent(
                    payload, Encoding.UTF8, "application/json");

                await client.PostAsync("api/Notification/create", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[EmergencyAlertService] Notification failed: {ex.Message}");
            }
        }
    }
}