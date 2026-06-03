namespace ssh.authservice.DTOs.Response
{
    public class RoleResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
