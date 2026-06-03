using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Models;

namespace SheSecure.Safety_WellnessService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodController : ControllerBase
    {
        private readonly WellnessDbContext _context;

        public MoodController(WellnessDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateMoodLogDTO dto)
        {
            var mood = new MoodLog
            {
                EmployeeId = dto.EmployeeId,
                MoodLevel = dto.MoodLevel,
                StressLevel = dto.StressLevel,
                Remarks = dto.Remarks
            };

            _context.MoodLogs.Add(mood);

            await _context.SaveChangesAsync();

            return Ok(mood);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.MoodLogs.ToListAsync());
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeLogs(string employeeId)
        {
            var logs = await _context.MoodLogs
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }
    }
}