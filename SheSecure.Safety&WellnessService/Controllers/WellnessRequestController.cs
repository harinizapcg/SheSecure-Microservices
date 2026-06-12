using Microsoft.AspNetCore.Mvc;
using SheSecure.WellnessSafetyService.DTOs.Requests;
using SheSecure.WellnessSafetyService.Interfaces;

namespace SheSecure.WellnessSafetyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WellnessRequestController : ControllerBase
    {
        private readonly IWellnessRequestService _service;

        public WellnessRequestController(
            IWellnessRequestService service)
        {
            _service = service;
        }

        // POST /api/WellnessRequest/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateRequest(
            [FromBody] CreateWellnessRequestDTO dto)
        {
            var result = await _service.CreateRequestAsync(dto);

            return Ok(result);
        }

        // GET /api/WellnessRequest/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRequests()
        {
            var result = await _service.GetAllRequestsAsync();

            return Ok(result);
        }

        // GET /api/WellnessRequest/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound("Wellness request not found");

            return Ok(result);
        }

        // PUT /api/WellnessRequest/status
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] UpdateWellnessRequestStatusDTO dto)
        {
            await _service.UpdateStatusAsync(dto);

            return Ok("Wellness request updated successfully");
        }

        // PUT /api/WellnessRequest/approve/{requestId}
        // Manager approves a WFH request
        [HttpPut("approve/{requestId}")]
        public async Task<IActionResult> ApproveRequest(
            int requestId, [FromQuery] int managerId)
        {
            await _service.ApproveRequestAsync(requestId, managerId);

            return Ok("Wellness request approved successfully");
        }

        // PUT /api/WellnessRequest/reject/{requestId}
        // Manager rejects a WFH request with a reason
        [HttpPut("reject/{requestId}")]
        public async Task<IActionResult> RejectRequest(
            int requestId,
            [FromQuery] int managerId,
            [FromQuery] string reason)
        {
            await _service.RejectRequestAsync(requestId, managerId, reason);

            return Ok("Wellness request rejected successfully");
        }
    }
}