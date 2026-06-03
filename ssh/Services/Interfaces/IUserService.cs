using ssh.authservice.DTOs.Request;
using ssh.authservice.DTOs.Response;
using ssh.authservice.DTOs.Common;

namespace ssh.authservice.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResultDTO<UserResponseDTO>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<UserResponseDTO?> GetUserByIdAsync(string userId);
        Task<UserResponseDTO> UpdateUserAsync(string userId, UpdateUserDTO request);
        Task<bool> DeactivateUserAsync(string userId);
        Task<PagedResultDTO<UserResponseDTO>> GetUsersByDepartmentAsync(string departmentId, int pageNumber, int pageSize);
    }
}
