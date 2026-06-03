namespace ssh.authservice.Entities
{
    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public string PermissionId { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Role? Role { get; set; }
        public virtual Permission? Permission { get; set; }
    }
}
