using Hangfire;
using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Jobs;
using System.Text;
using System.Text.Json;

namespace SheSecure.Safety_WellnessService.Services
{
    public class SafeReachService : ISafeReachService
    {
        private readonly ISafeReachRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public SafeReachService(
            ISafeReachRepository repository,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task CreateAsync(CreateSafeReachDTO dto)
        {
            var check = new SafeReachCheck
            {
                EmployeeId = dto.EmployeeId,
                ExpectedArrivalTime = dto.ExpectedArrivalTime,
                IsAcknowledged = false,
                Status = "Pending"
            };

            await _repository.CreateAsync(check);

            // Normalize to UTC
            var expectedUtc =
                dto.ExpectedArrivalTime.Kind == DateTimeKind.Utc
                    ? dto.ExpectedArrivalTime
                    : dto.ExpectedArrivalTime.ToUniversalTime();

            var now = DateTime.UtcNow;

            // Job 1 — reminder at expected arrival time
            var reminderDelay = expectedUtc - now;
            if (reminderDelay > TimeSpan.Zero)
            {
                BackgroundJob.Schedule<SafeReachReminderJob>(
                    job => job.SendReminderAsync(check.Id),
                    reminderDelay);
            }
            else
            {
                BackgroundJob.Enqueue<SafeReachReminderJob>(
                    job => job.SendReminderAsync(check.Id));
            }

            // Job 2 — escalation 30 min after expected arrival
            var escalationDelay =
                expectedUtc - now + TimeSpan.FromMinutes(30);
            if (escalationDelay > TimeSpan.Zero)
            {
                BackgroundJob.Schedule<SafeReachReminderJob>(
                    job => job.CheckAndEscalateAsync(check.Id),
                    escalationDelay);
            }
            else
            {
                BackgroundJob.Schedule<SafeReachReminderJob>(
                    job => job.CheckAndEscalateAsync(check.Id),
                    TimeSpan.FromMinutes(30));
            }
        }

        public async Task AcknowledgeAsync(AcknowledgeSafeReachDTO dto)
        {
            var check = await _repository.GetByIdAsync(dto.SafeReachId);

            if (check == null)
                throw new Exception("Safe Reach record not found");

            check.IsAcknowledged = true;
            check.AcknowledgedAt = DateTime.UtcNow;
            check.Status = "Acknowledged";

            await _repository.UpdateAsync(check);
        }

        public async Task EscalateAsync(int id)
        {
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
                throw new Exception("Safe Reach record not found");

            if (check.IsAcknowledged)
                throw new Exception("Employee already acknowledged");

            check.Status = "Escalated";
            await _repository.UpdateAsync(check);

            var message =
                $"Employee ID {check.EmployeeId} failed to confirm safe arrival. " +
                $"Manual escalation triggered. Please review immediately.";

            // Notify HR
            await SendNotificationAsync(
                employeeId: "2",
                title: "SafeReach Escalation - HR Action Required",
                message: message,
                type: "Emergency"
            );

            // Notify Security
            await SendNotificationAsync(
                employeeId: "3",
                title: "SafeReach Escalation - Security Alert",
                message: message,
                type: "Emergency"
            );

            // Notify Admin
            await SendNotificationAsync(
                employeeId: "1",
                title: "SafeReach Escalation - Admin Notice",
                message: message,
                type: "Emergency"
            );
        }

        public async Task<object> GetAllAsync()
            => await _repository.GetAllAsync();

        public async Task<object> GetByIdAsync(int id)
        {
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
                throw new Exception("Safe Reach record not found");

            return check;
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
                    $"[SafeReachService] Notification failed: {ex.Message}");
            }
        }
    }
}