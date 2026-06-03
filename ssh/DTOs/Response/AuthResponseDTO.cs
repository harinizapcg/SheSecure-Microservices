namespace ssh.authservice.DTOs.Response
{
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TokenResponseDTO? Token { get; set; }
        public UserResponseDTO? User { get; set; }
    }
}
