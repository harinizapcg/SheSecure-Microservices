using Microsoft.EntityFrameworkCore;
using SheSecure.ComplaintService.Data;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly ComplaintDbContext _context;

        public ComplaintRepository(ComplaintDbContext context)
        {
            _context = context;
        }

        public async Task<Complaint> CreateComplaintAsync(Complaint complaint)
        {
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task<List<Complaint>> GetAllComplaintsAsync()
        {
            return await _context.Complaints.ToListAsync();
        }

        public async Task<Complaint?> GetComplaintByIdAsync(int id)
        {
            return await _context.Complaints.FindAsync(id);
        }

        // Fixed: was using x.EmployeeId which didn't exist — now uses x.UserId
        public async Task<List<Complaint>> GetComplaintsByUserIdAsync(string userId)
        {
            return await _context.Complaints
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task UpdateComplaintAsync(Complaint complaint)
        {
            complaint.UpdatedAt = DateTime.UtcNow;
            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteComplaintAsync(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);

            if (complaint == null)
                return false;

            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}