using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintResponseDTO> CreateComplaintAsync(CreateComplaintDTO dto, string userId);

        Task<List<ComplaintResponseDTO>> GetAllComplaintsAsync(string role, string userId);

        Task<List<ComplaintResponseDTO>> GetMyComplaintsAsync(string userId);

        Task<ComplaintResponseDTO?> GetComplaintByIdAsync(int id, string role, string userId);

        Task UpdateComplaintStatusAsync(UpdateComplaintStatusDTO dto, string role);

        Task AssignComplaintAsync(AssignComplaintDTO dto, string role);

        // Admin only
        Task<ComplaintResponseDTO?> EditComplaintAsync(int id, EditComplaintDTO dto);

        Task<bool> DeleteComplaintAsync(int id);
    }
}