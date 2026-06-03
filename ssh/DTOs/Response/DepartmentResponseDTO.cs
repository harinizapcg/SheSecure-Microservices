namespace ssh.authservice.DTOs.Response
{
    public class DepartmentResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ManagerId { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
