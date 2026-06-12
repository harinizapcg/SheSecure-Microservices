using SheSecure.WellnessSafetyService.DTOs.Requests;
using SheSecure.WellnessSafetyService.DTOs.Responses;
using SheSecure.WellnessSafetyService.Entities;
using SheSecure.WellnessSafetyService.Interfaces;
using System.Text;
using System.Text.Json;

namespace SheSecure.WellnessSafetyService.Services
{
    public class WellnessRequestService
        : IWellnessRequestService
    {
        private readonly IWellnessRequestRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public WellnessRequestService(
            IWellnessRequestRepository repository,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<WellnessRequestResponseDTO>
            CreateRequestAsync(CreateWellnessRequestDTO dto)
        {
            var request = new WellnessRequest
            {
                EmployeeId = dto.EmployeeId,
                RequestType = dto.RequestType,
                Description = dto.Description,
                Priority = dto.Priority
            };

            var saved = await _repository.CreateRequestAsync(request);

            // Notify employee that request was received
            await SendNotificationAsync(
                employeeId: dto.EmployeeId.ToString(),
                title: "WFH Request Submitted",
                message: $"Your {dto.RequestType} request has been submitted and is pending approval.",
                type: "Wellness"
            );

            return MapToResponse(saved);
        }

        public async Task<List<WellnessRequestResponseDTO>>
            GetAllRequestsAsync()
        {
            var requests = await _repository.GetAllRequestsAsync();

            return requests.Select(MapToResponse).ToList();
        }

        public async Task<WellnessRequestResponseDTO?>
            GetByIdAsync(int id)
        {
            var request = await _repository.GetByIdAsync(id);

            if (request == null)
                return null;

            return MapToResponse(request);
        }

        public async Task UpdateStatusAsync(
            UpdateWellnessRequestStatusDTO dto)
        {
            var request = await _repository.GetByIdAsync(dto.RequestId);

            if (request == null)
                throw new Exception("Wellness request not found");

            request.Status = dto.Status;
            request.AssignedTo = dto.AssignedTo;

            await _repository.UpdateRequestAsync(request);
        }

        public async Task ApproveRequestAsync(
            int requestId, int managerId)
        {
            var request = await _repository.GetByIdAsync(requestId);

            if (request == null)
                throw new Exception("Wellness request not found");

            if (request.Status != "Pending")
                throw new Exception(
                    $"Request cannot be approved as it is already '{request.Status}'");

            request.Status = "Approved";
            request.AssignedTo = managerId;

            await _repository.UpdateRequestAsync(request);

            // Notify employee
            await SendNotificationAsync(
                employeeId: request.EmployeeId.ToString(),
                title: "WFH Request Approved",
                message: $"Your {request.RequestType} request has been approved by management.",
                type: "Wellness"
            );
        }

        public async Task RejectRequestAsync(
            int requestId, int managerId, string reason)
        {
            var request = await _repository.GetByIdAsync(requestId);

            if (request == null)
                throw new Exception("Wellness request not found");

            if (request.Status != "Pending")
                throw new Exception(
                    $"Request cannot be rejected as it is already '{request.Status}'");

            request.Status = "Rejected";
            request.AssignedTo = managerId;

            await _repository.UpdateRequestAsync(request);

            // Notify employee
            await SendNotificationAsync(
                employeeId: request.EmployeeId.ToString(),
                title: "WFH Request Rejected",
                message: $"Your {request.RequestType} request has been rejected. Reason: {reason}",
                type: "Wellness"
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
                    $"[WellnessRequestService] Notification failed: {ex.Message}");
            }
        }

        private WellnessRequestResponseDTO MapToResponse(
            WellnessRequest x) => new()
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                RequestType = x.RequestType,
                Description = x.Description,
                Priority = x.Priority,
                Status = x.Status,
                AssignedTo = x.AssignedTo,
                CreatedAt = x.CreatedAt
            };
    }
}