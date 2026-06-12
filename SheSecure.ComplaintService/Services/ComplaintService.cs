using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;
using System.Text;
using System.Text.Json;

namespace SheSecure.ComplaintService.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public ComplaintService(
            IComplaintRepository repository,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ComplaintResponseDTO> CreateComplaintAsync(
            CreateComplaintDTO dto,
            string employeeId)
        {
            var complaint = new Complaint
            {
                // If anonymous, store 0 so real ID is never saved
                EmployeeId = dto.IsAnonymous ? "0" : employeeId,
                Category = dto.Category,
                Subject = dto.Subject,
                Description = dto.Description,
                Priority = dto.Priority,
                IsAnonymous = dto.IsAnonymous,
                Status = "Submitted",
                ComplaintNumber = GenerateComplaintNumber()
            };

            var created = await _repository.CreateComplaintAsync(complaint);

            // Notify employee that complaint was received (skip if anonymous)
            if (!dto.IsAnonymous)
            {
                await SendNotificationAsync(
                    employeeId: employeeId.ToString(),
                    title: "Complaint Submitted",
                    message: $"Your complaint {created.ComplaintNumber} has been successfully submitted.",
                    type: "Complaint"
                );
            }

            return MapToResponse(created);
        }

        public async Task<List<ComplaintResponseDTO>> GetAllComplaintsAsync()
        {
            var complaints = await _repository.GetAllComplaintsAsync();

            return complaints.Select(MapToResponse).ToList();
        }
        public async Task<List<ComplaintResponseDTO>> GetMyComplaintsAsync(string employeeId)
        {
            var complaints =
                await _repository.GetComplaintsByEmployeeIdAsync(employeeId);

            return complaints
                .Select(MapToResponse)
                .ToList();
        }
        public async Task<ComplaintResponseDTO> GetComplaintByIdAsync(int id)
        {
            var complaint = await _repository.GetComplaintByIdAsync(id);

            if (complaint == null)
                throw new Exception("Complaint not found");

            return MapToResponse(complaint);
        }

        public async Task UpdateComplaintStatusAsync(
            UpdateComplaintStatusDTO dto)
        {
            var complaint =
                await _repository.GetComplaintByIdAsync(dto.ComplaintId);

            if (complaint == null)
                throw new Exception("Complaint not found");

            complaint.Status = dto.Status;
            complaint.ResolutionNotes = dto.ResolutionNotes;
            complaint.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateComplaintAsync(complaint);

            // Notify employee about status change (skip if anonymous)
            if (!complaint.IsAnonymous && complaint.EmployeeId != "0")
            {
                await SendNotificationAsync(
                    employeeId: complaint.EmployeeId.ToString(),
                    title: "Complaint Status Updated",
                    message: $"Your complaint {complaint.ComplaintNumber} status has been updated to '{dto.Status}'.",
                    type: "Complaint"
                );
            }
        }

        public async Task AssignComplaintAsync(AssignComplaintDTO dto)
        {
            var complaint =
                await _repository.GetComplaintByIdAsync(dto.ComplaintId);

            if (complaint == null)
                throw new Exception("Complaint not found");

            complaint.AssignedTo = dto.AssignedTo;

            await _repository.UpdateComplaintAsync(complaint);

            // Notify assigned person
            await SendNotificationAsync(
                employeeId: dto.AssignedTo.ToString(),
                title: "Complaint Assigned to You",
                message: $"Complaint {complaint.ComplaintNumber} has been assigned to you for investigation.",
                type: "Complaint"
            );
        }

        // ── Helpers ───────────────────────────────────────────────────────

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
                    $"[ComplaintService] Notification failed: {ex.Message}");
            }
        }

        private ComplaintResponseDTO MapToResponse(Complaint x)
        {
            return new ComplaintResponseDTO
            {
                Id = x.Id,
                ComplaintNumber = x.ComplaintNumber,
                // Mask employee ID if anonymous
                EmployeeId = x.IsAnonymous ? null : x.EmployeeId,
                Category = x.Category,
                Subject = x.Subject,
                Priority = x.Priority,
                Status = x.Status,
                IsAnonymous = x.IsAnonymous,
                AssignedTo = x.AssignedTo,
                ResolutionNotes = x.ResolutionNotes,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            };
        }

        private string GenerateComplaintNumber()
        {
            return $"CMP-{DateTime.UtcNow.Ticks}";
        }
    }
}