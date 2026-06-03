namespace SheSecure.ComplaintService.DTOs.Requests
{
    public class AssignComplaintDTO
    {
        public int ComplaintId { get; set; }

        public int AssignedTo { get; set; }
    }
}