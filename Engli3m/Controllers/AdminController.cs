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

        [HttpGet("locked-accounts")]
        public async Task<IActionResult> GetAllLockedAccounts()
        {
            var lockedAccounts = await _adminService.GetAllLockedAccountAsync();
            if (lockedAccounts == null || !lockedAccounts.Any())
                return NotFound("No locked accounts found.");
            return Ok(lockedAccounts);
        }
        [HttpGet("unlocked-accounts")]
        public async Task<IActionResult> GetAllUnLockedAccounts()
        {
            var lockedAccounts = await _adminService.GetAllUnLockedAccountAsync();
            if (lockedAccounts == null || !lockedAccounts.Any())
                return NotFound("No Unlocked accounts found.");
            return Ok(lockedAccounts);
        }

        [HttpPost("unlock/{userId:int}")]
        public async Task<IActionResult> UnlockUser(int userId)
        {
            var success = await _adminService.UnlockUserAsync(userId);
            if (!success)
                return StatusCode(400, "Failed to unlock user.");
            return Ok(new { Message = "User unlocked successfully." });
        }

        [HttpPost("lock/{userId:int}")]
        public async Task<IActionResult> LockUser(int userId)
        {
            var success = await _adminService.LockUserAsync(userId);
            if (!success)
                return StatusCode(400, "Failed to lock user.");
            return Ok(new { Message = "User locked successfully." });
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
