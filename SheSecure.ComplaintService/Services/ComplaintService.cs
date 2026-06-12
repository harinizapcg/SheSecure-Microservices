using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _repository;

        // Roles that can see all complaints
        private static readonly HashSet<string> PrivilegedRoles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "HR", "Security", "Manager", "Admin"
            };

        public ComplaintService(IComplaintRepository repository)
        {
            _repository = repository;
        }

        // ===================== CREATE =====================
        public async Task<ComplaintResponseDTO> CreateComplaintAsync(
            CreateComplaintDTO dto,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId cannot be null.");

            var complaint = new Complaint
            {
                UserId = dto.IsAnonymous ? "0" : userId,
                Category = dto.Category,
                Subject = dto.Subject,
                Description = dto.Description,
                Priority = dto.Priority,
                IsAnonymous = dto.IsAnonymous,
                Status = "Submitted",
                ComplaintNumber = $"CMP-{DateTime.UtcNow.Ticks}"
            };

            var created = await _repository.CreateComplaintAsync(complaint);
            return Map(created);
        }

        // ===================== GET ALL (ROLE BASED) =====================
        // Employee      → only their own complaints
        // HR/Security/Manager/Admin → all complaints
        public async Task<List<ComplaintResponseDTO>> GetAllComplaintsAsync(
            string role,
            string userId)
        {
            var complaints = await _repository.GetAllComplaintsAsync();

            if (!PrivilegedRoles.Contains(role))
            {
                // Employee or unknown role: filter to own complaints only
                complaints = complaints
                    .Where(c => c.UserId == userId)
                    .ToList();
            }

            return complaints.Select(Map).ToList();
        }

        // ===================== GET MY COMPLAINTS =====================
        public async Task<List<ComplaintResponseDTO>> GetMyComplaintsAsync(string userId)
        {
            var complaints = await _repository.GetComplaintsByUserIdAsync(userId);
            return complaints.Select(Map).ToList();
        }

        // ===================== GET BY ID (ROLE BASED) =====================
        public async Task<ComplaintResponseDTO?> GetComplaintByIdAsync(
            int id,
            string role,
            string userId)
        {
            var complaint = await _repository.GetComplaintByIdAsync(id);

            if (complaint == null)
                return null;

            // Employee can only see their own complaint
            if (!PrivilegedRoles.Contains(role) && complaint.UserId != userId)
                return null;

            return Map(complaint);
        }

        // ===================== UPDATE STATUS =====================
        // HR, Security, Manager, Admin only (enforced at controller too)
        public async Task UpdateComplaintStatusAsync(
            UpdateComplaintStatusDTO dto,
            string role)
        {
            if (!PrivilegedRoles.Contains(role))
                throw new UnauthorizedAccessException("Not allowed.");

            var complaint = await _repository.GetComplaintByIdAsync(dto.ComplaintId);

            if (complaint == null)
                throw new Exception("Complaint not found.");

            complaint.Status = dto.Status;
            complaint.ResolutionNotes = dto.ResolutionNotes;

            await _repository.UpdateComplaintAsync(complaint);
        }

        // ===================== ASSIGN =====================
        // HR, Security, Manager, Admin only (enforced at controller too)
        public async Task AssignComplaintAsync(
            AssignComplaintDTO dto,
            string role)
        {
            if (!PrivilegedRoles.Contains(role))
                throw new UnauthorizedAccessException("Not allowed.");

            var complaint = await _repository.GetComplaintByIdAsync(dto.ComplaintId);

            if (complaint == null)
                throw new Exception("Complaint not found.");

            complaint.AssignedTo = dto.AssignedTo.ToString();

            await _repository.UpdateComplaintAsync(complaint);
        }

        // ===================== EDIT (ADMIN ONLY) =====================
        public async Task<ComplaintResponseDTO?> EditComplaintAsync(
            int id,
            EditComplaintDTO dto)
        {
            var complaint = await _repository.GetComplaintByIdAsync(id);

            if (complaint == null)
                return null;

            // Only update fields that are provided
            if (!string.IsNullOrWhiteSpace(dto.Category))
                complaint.Category = dto.Category;

            if (!string.IsNullOrWhiteSpace(dto.Subject))
                complaint.Subject = dto.Subject;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                complaint.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Priority))
                complaint.Priority = dto.Priority;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                complaint.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes))
                complaint.ResolutionNotes = dto.ResolutionNotes;

            await _repository.UpdateComplaintAsync(complaint);

            return Map(complaint);
        }

        // ===================== DELETE (ADMIN ONLY) =====================
        public async Task<bool> DeleteComplaintAsync(int id)
        {
            return await _repository.DeleteComplaintAsync(id);
        }

        // ===================== MAPPER =====================
        private ComplaintResponseDTO Map(Complaint x)
        {
            return new ComplaintResponseDTO
            {
                Id = x.Id,
                ComplaintNumber = x.ComplaintNumber,
                UserId = x.IsAnonymous ? null : x.UserId,
                Category = x.Category,
                Subject = x.Subject,
                Description = x.Description,
                Priority = x.Priority,
                Status = x.Status,
                IsAnonymous = x.IsAnonymous,
                AssignedTo = x.AssignedTo,
                ResolutionNotes = x.ResolutionNotes,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            };
        }
    }
}