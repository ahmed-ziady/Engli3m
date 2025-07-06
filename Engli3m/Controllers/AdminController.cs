using Engli3m.Application.DTOs;
using Engli3m.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Engli3m.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController(IAdminService _adminService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("lecture")]
        public async Task<IActionResult> UploadLecture([FromForm] LectureUploadDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var lectureId = await _adminService.UploadLectureAsync(dto, userId);
            if (lectureId ==-1)
                return StatusCode(500, "Lecture upload failed.");

            return Ok(new { Message = "Lecture uploaded successfully.", lectureId });
        }

        [HttpPost("quiz")]
        public async Task<IActionResult> UploadQuiz([FromForm] QuizUploadDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var success = await _adminService.UploadQuizAsync(dto, userId);
            if (!success)
                return StatusCode(500, "Quiz upload failed.");

            return Ok(new { Message = "Quiz uploaded successfully." });
        }

        [HttpGet("student-accounts")]
        public async Task<IActionResult> GetAllLockedAccounts()
        {
            var lockedAccounts = await _adminService.GetAllStudentAccountAsync();
            if (lockedAccounts == null || !lockedAccounts.Any())
                return NotFound("No  accounts found.");
            return Ok(lockedAccounts);
        }

        [HttpPost("toggle-lock/{userId:int}")]
        public async Task<IActionResult> ToggleUserLockStatusAsync(int userId)
        {
            try
            {
                var isNowLocked = await _adminService.ToggleUserLockStatusAsync(userId);

                var statusMessage = isNowLocked ? "User has been locked." : "User has been unlocked.";

                return Ok(new { Message = statusMessage, IsLocked = isNowLocked });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { ex.Message });
            }

            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while toggling user status." });
            }

        }

        [HttpGet("quizzes-answers")]
        public async Task<IActionResult> QuizzesAnswers()
        {
            var answers = await _adminService.QuizzesAnswersAsync();
            if (answers == null || !answers.Any())
                return NotFound("No quiz answers found.");
            return Ok(answers);

        }
    }
}
