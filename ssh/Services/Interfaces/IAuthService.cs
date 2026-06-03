using ssh.authservice.DTOs.Request;
using ssh.authservice.DTOs.Response;
using ssh.authservice.DTOs.Common;

namespace ssh.authservice.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request);
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO request);
    }
}
