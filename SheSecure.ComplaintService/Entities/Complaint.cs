namespace SheSecure.ComplaintService.Entities
{
    public class Complaint
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string ComplaintNumber { get; set; }

        public string Category { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public bool IsAnonymous { get; set; }

        public string? AssignedTo { get; set; }       // nullable

        public string? ResolutionNotes { get; set; }  // nullable

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}