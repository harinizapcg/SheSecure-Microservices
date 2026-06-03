using ssh.authservice.DTOs.Request;
using ssh.authservice.DTOs.Response;
using ssh.authservice.DTOs.Common;

namespace ssh.authservice.Services.Interfaces
{
    public interface IRoleService
    {
        Task<PagedResultDTO<RoleResponseDTO>> GetAllRolesAsync(int pageNumber, int pageSize);
        Task<RoleResponseDTO?> GetRoleByIdAsync(string roleId);
        Task<RoleResponseDTO> CreateRoleAsync(RoleResponseDTO request);
        Task<RoleResponseDTO> UpdateRoleAsync(string roleId, RoleResponseDTO request);
        Task<bool> DeleteRoleAsync(string roleId);
        Task<bool> AssignRoleToUserAsync(AssignRoleDTO request);
    }
}
