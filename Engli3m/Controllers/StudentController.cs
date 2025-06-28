using Engli3m.Application.DTOs;
using Engli3m.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Engli3m.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentController(IStudentService studentService) : ControllerBase
    {

        [HttpGet("lectures-and-quizzes")]
        public async Task<IActionResult> GetAllLecturesAndQuizzes()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            try
            {
                var result = await studentService.GetAllLecturesAndQuizzes(userId);
                return Ok(result);
            }
            catch (Exception)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> SubmitAnswer([FromForm] SubmmitQuizDto dto)
        {
            // Get student ID from token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var studentId))
                return Unauthorized();

            // Call service to save submission
            var success = await studentService.SubmmitQuizAnswerAsync(dto, studentId);
            if (!success)
                return StatusCode(500, "Failed to submit quiz answer.");

            return Ok(new { Message = "Quiz answer submitted successfully." });
        }
    }
}
