namespace SheSecure.ComplaintService.DTOs.Responses
{
    public class ComplaintResponseDTO
    {
        public int Id { get; set; }

        public string ComplaintNumber { get; set; }

        // Null when IsAnonymous = true
        public string? UserId { get; set; }

        public string Category { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public bool IsAnonymous { get; set; }

        public string? AssignedTo { get; set; }

        public string? ResolutionNotes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}