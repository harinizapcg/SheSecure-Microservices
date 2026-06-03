namespace ssh.authservice.DTOs.Common
{
    public class ErrorResponseDTO
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
    }
}
