using SheSecure.WellnessSafetyService.DTOs.Requests;
using SheSecure.WellnessSafetyService.DTOs.Responses;

namespace SheSecure.WellnessSafetyService.Interfaces
{
    public interface IWellnessRequestService
    {
        Task<WellnessRequestResponseDTO>
            CreateRequestAsync(
                CreateWellnessRequestDTO dto);

        Task<List<WellnessRequestResponseDTO>>
            GetAllRequestsAsync();

        Task<WellnessRequestResponseDTO?>
            GetByIdAsync(int id);

        Task UpdateStatusAsync(
            UpdateWellnessRequestStatusDTO dto);

        Task ApproveRequestAsync(int requestId, int managerId);

        Task RejectRequestAsync(int requestId, int managerId, string reason);
    }
}