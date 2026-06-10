using Hangfire;
using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Jobs;
using SheSecure.Safety_WellnessService.Interfaces;

namespace SheSecure.Safety_WellnessService.Services
{
    public class SafeReachService : ISafeReachService
    {
        private readonly ISafeReachRepository _repository;

        public SafeReachService(ISafeReachRepository repository)
        {
            _repository = repository;
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
            var expectedUtc = dto.ExpectedArrivalTime.Kind == DateTimeKind.Utc
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
                // Already past — fire immediately
                BackgroundJob.Enqueue<SafeReachReminderJob>(
                    job => job.SendReminderAsync(check.Id));
            }

            // Job 2 — escalation after arrival time + 30 min grace
            var escalationDelay = expectedUtc - now + TimeSpan.FromMinutes(30);
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
                throw new Exception("Safe reach check not found");

            check.IsAcknowledged = true;
            check.AcknowledgedAt = DateTime.UtcNow;
            check.Status = "Acknowledged";

            await _repository.UpdateAsync(check);
        }

        public async Task<object> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return list;
        }

        public async Task<object> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task EscalateAsync(int id)
        {
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
                throw new Exception("Safe reach check not found");

            check.Status = "Escalated";

            await _repository.UpdateAsync(check);
        }
    }
}
