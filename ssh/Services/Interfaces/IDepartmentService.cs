using ssh.authservice.DTOs.Request;
using ssh.authservice.DTOs.Response;
using ssh.authservice.DTOs.Common;

namespace ssh.authservice.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<PagedResultDTO<DepartmentResponseDTO>> GetAllDepartmentsAsync(int pageNumber, int pageSize);
        Task<DepartmentResponseDTO?> GetDepartmentByIdAsync(string departmentId);
        Task<DepartmentResponseDTO> CreateDepartmentAsync(CreateDepartmentDTO request);
        Task<DepartmentResponseDTO> UpdateDepartmentAsync(string departmentId, CreateDepartmentDTO request);
        Task<bool> DeleteDepartmentAsync(string departmentId);
        Task<DepartmentResponseDTO?> GetDepartmentStatisticsAsync(string departmentId);
    }
}
