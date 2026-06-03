using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;

namespace SheSecure.Safety_WellnessService.Services
{
    public class SafeReachService : ISafeReachService
    {
        private readonly ISafeReachRepository _repository;

        public SafeReachService(
            ISafeReachRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync(
            CreateSafeReachDTO dto)
        {
            var check = new SafeReachCheck
            {
                EmployeeId = dto.EmployeeId,
                ExpectedArrivalTime =
                    dto.ExpectedArrivalTime,
                IsAcknowledged = false,
                Status = "Pending"
            };

            await _repository.CreateAsync(check);
        }

        public async Task AcknowledgeAsync(
            AcknowledgeSafeReachDTO dto)
        {
            var check =
                await _repository.GetByIdAsync(
                    dto.SafeReachId);

            if (check == null)
            {
                throw new Exception(
                    "Safe Reach record not found");
            }

            check.IsAcknowledged = true;

            check.AcknowledgedAt =
                DateTime.UtcNow;

            check.Status = "Reached Safely";

            await _repository.UpdateAsync(check);
        }
        public async Task EscalateAsync(int id)
        {
            var check =
                await _repository.GetByIdAsync(id);

            if (check == null)
            {
                throw new Exception(
                    "Safe Reach record not found");
            }

            if (check.IsAcknowledged)
            {
                throw new Exception(
                    "Employee already acknowledged");
            }

            check.Status = "Escalated";

            await _repository.UpdateAsync(check);
        }
        public async Task<object> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<object> GetByIdAsync(
            int id)
        {
            var check =
                await _repository.GetByIdAsync(id);

            if (check == null)
            {
                throw new Exception(
                    "Safe Reach record not found");
            }

            return check;
        }
    }
}