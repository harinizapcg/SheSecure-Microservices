namespace ssh.authservice.DTOs.Request
{
    public class CreateDepartmentDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ManagerId { get; set; } = string.Empty;
    }
}
