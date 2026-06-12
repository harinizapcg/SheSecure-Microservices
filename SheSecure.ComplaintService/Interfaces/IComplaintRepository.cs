using SheSecure.ComplaintService.Entities;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateComplaintAsync(Complaint complaint);

        Task<List<Complaint>> GetAllComplaintsAsync();

        Task<Complaint?> GetComplaintByIdAsync(int id);

        Task<List<Complaint>> GetComplaintsByUserIdAsync(string userId);

        Task UpdateComplaintAsync(Complaint complaint);

        Task<bool> DeleteComplaintAsync(int id);
    }
}