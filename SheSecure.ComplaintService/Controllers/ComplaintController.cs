using Microsoft.AspNetCore.Mvc;
using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _service;

        public ComplaintController(IComplaintService service)
        {
            _service = service;
        }

        // ===================== CREATE =====================
        // Any role can create a complaint
        [HttpPost("create")]
        public async Task<IActionResult> CreateComplaint(
            [FromBody] CreateComplaintDTO dto,
            [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("userId is required.");

            var result = await _service.CreateComplaintAsync(dto, userId);
            return Ok(result);
        }

        // ===================== GET ALL (ROLE BASED) =====================
        // Employee  → sees only their own complaints
        // HR, Security, Manager, Admin → sees all complaints
        [HttpGet("all")]
        public async Task<IActionResult> GetAllComplaints(
            [FromQuery] string role,
            [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(userId))
                return BadRequest("role and userId are required.");

            var result = await _service.GetAllComplaintsAsync(role, userId);
            return Ok(result);
        }

        // ===================== GET BY ID (ROLE BASED) =====================
        // Employee  → can only view their own complaint
        // HR, Security, Manager, Admin → can view any complaint
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComplaintById(
            int id,
            [FromQuery] string role,
            [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(userId))
                return BadRequest("role and userId are required.");

            var result = await _service.GetComplaintByIdAsync(id, role, userId);

            if (result == null)
                return NotFound("Complaint not found or access denied.");

            return Ok(result);
        }

        // ===================== MY COMPLAINTS =====================
        // Returns only complaints belonging to the given userId
        [HttpGet("my")]
        public async Task<IActionResult> GetMyComplaints(
            [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("userId is required.");

            var result = await _service.GetMyComplaintsAsync(userId);
            return Ok(result);
        }

        // ===================== UPDATE STATUS =====================
        // HR, Security, Manager, Admin only
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] UpdateComplaintStatusDTO dto,
            [FromQuery] string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest("role is required.");

            if (role == "Employee")
                return StatusCode(403, "Access denied.");

            await _service.UpdateComplaintStatusAsync(dto, role);
            return Ok("Status updated successfully.");
        }

        // ===================== ASSIGN =====================
        // HR, Security, Manager, Admin only
        [HttpPut("assign")]
        public async Task<IActionResult> AssignComplaint(
            [FromBody] AssignComplaintDTO dto,
            [FromQuery] string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest("role is required.");

            if (role == "Employee")
                return StatusCode(403, "Access denied.");

            await _service.AssignComplaintAsync(dto, role);
            return Ok("Complaint assigned successfully.");
        }

        // ===================== EDIT COMPLAINT (ADMIN ONLY) =====================
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditComplaint(
            int id,
            [FromBody] EditComplaintDTO dto,
            [FromQuery] string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest("role is required.");

            if (role != "Admin")
                return StatusCode(403, "Access denied.");

            var result = await _service.EditComplaintAsync(id, dto);

            if (result == null)
                return NotFound("Complaint not found.");

            return Ok(result);
        }

        // ===================== DELETE COMPLAINT (ADMIN ONLY) =====================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteComplaint(
            int id,
            [FromQuery] string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest("role is required.");

            if (role != "Admin")
                return StatusCode(403, "Access denied.");

            var deleted = await _service.DeleteComplaintAsync(id);

            if (!deleted)
                return NotFound("Complaint not found.");

            return Ok("Complaint deleted successfully.");
        }
    }
}