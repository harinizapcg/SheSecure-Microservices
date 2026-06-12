namespace SheSecure.ComplaintService.DTOs.Requests
{
    // Used by Admin to edit any complaint field
    public class EditComplaintDTO
    {
        public string? Category { get; set; }

        public string? Subject { get; set; }

        public string? Description { get; set; }

        public string? Priority { get; set; }

        public string? Status { get; set; }

        public string? ResolutionNotes { get; set; }
    }
}