using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Quiz;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enums;
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
        [RequestSizeLimit(3221225472)]

        [AllowAnonymous]
        [HttpPost("lecture")]
        public async Task<IActionResult> UploadLecture([FromForm] LectureUploadDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var lectureId = await _adminService.UploadLectureAsync(dto, userId);
            if (lectureId == -1)
                return StatusCode(500, "فشل رفع المحاضرة.");

            return Ok(new { Message = "تم رفع المحاضرة بنجاح.", lectureId });
        }

        [HttpPost("quiz")]
        public async Task<IActionResult> UploadQuiz([FromForm] QuizUploadDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var success = await _adminService.UploadQuizAsync(dto, userId);
            if (!success)
                return BadRequest("فشل رفع الاختبار.");

            return Ok(new { Message = "تم رفع الاختبار بنجاح." });
        }

        [HttpGet("student-accounts")]
        public async Task<IActionResult> GetAllLockedAccounts(GradeLevel grade)
        {
            var lockedAccounts = await _adminService.GetAllStudentAccountAsync(grade);
            if (lockedAccounts == null || lockedAccounts.Count == 0)
                return NotFound("لا توجد حسابات.");
            return Ok(lockedAccounts);
        }

        [HttpPost("toggle-lock/{userId:int}")]
        public async Task<IActionResult> ToggleUserLockStatusAsync(int userId)
        {
            try
            {
                var isNowLocked = await _adminService.ToggleUserLockStatusAsync(userId);

                var statusMessage = isNowLocked ? "تم قفل الحساب." : "تم فتح الحساب.";

                return Ok(new { Message = statusMessage, IsLocked = isNowLocked });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "حدث خطأ غير متوقع أثناء تغيير حالة الحساب." });
            }
        }
        [HttpPost("toggle-Users-lock/{toggleLock:bool}")]
        public async Task<IActionResult> ToggleUsersLockAsync(bool toggleLock)
        {
          
                var isNowLocked = await _adminService.ToggleUsersLockStatusAsync(toggleLock);

                var statusMessage = isNowLocked ? "تم قفل جميع الحسابات." : "تم فتح جميع الحسابات .";

                return Ok(new { Message = statusMessage, IsLocked = isNowLocked });
            
        }


        [HttpPost("payment/{UserId:int}")]
        public async Task<IActionResult> PaymentStatusAsync(int UserId)
        {
            try
            {
                var isNowPaid = await _adminService.MarkTheUserAsPaidAsync(UserId);
                var statusMessage = "تم تحديث حالة الدفع للحساب إلى مدفوع.";
                return Ok(new { Message = statusMessage, IsPaid = isNowPaid });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "حدث خطأ غير متوقع أثناء تغيير حالة الدفع." });
            }
        }

        [HttpGet("quizzes-answers")]
        public async Task<IActionResult> QuizzesAnswers(int UserId)
        {
            var answers = await _adminService.QuizzesAnswersAsync();
            if (answers == null || answers.Count == 0)
                return NotFound("لا توجد إجابات للاختبارات.");
            return Ok(answers);
        }

        [HttpGet("getLecutures")]
        public async Task<IActionResult> GetLeacturesByGrade(GradeLevel grade)
        {
            var result = await _adminService.GetLecturesByGrade(grade);
            if (result == null || result.Count == 0) return NotFound("لا يوجد حصص بعد");
            return Ok(result);
        }
        [HttpDelete("deleteLecture")]
        public async Task<IActionResult> DeleteLectureAsync(int lectureId)
        {
            var result = await _adminService.DeleteLecureAsync(lectureId);
            if (!result)
                return BadRequest("لم يتم حذف الحصة");
            return Ok("تم حذف الحصة بنجاح ");
        }
        [HttpPost("activeLecture")]
        public async Task<IActionResult> ActiveLecture(int lectureId)
        {
            var result = await _adminService.ActiveLectureAsync(lectureId);
            if (!result)
                return BadRequest("تم ايقاف نشاط الحصة");
            return Ok("الطالب يقدر يشوف الحصة الان");

        }
        [HttpPut("editLectureName")]
        public async Task<IActionResult> EditLecture(int lectureId, string LectureName)
        {
            var result = await _adminService.EditLectureAsync(lectureId, LectureName);

            if (!result)
                return NotFound("Lecture not found");
            return Ok("Lecture Updated Successfully");

        }
            [HttpPost("submit-degree")]
        public async Task<IActionResult> SubmitDegree(int userId, double degree)
        {
            if (degree < 1 || degree > 10)
                return BadRequest("The degree must be between 1 and 10.");

            var result = await _adminService.SubmitDegreeAsync(userId, degree);

            if (!result)
                return NotFound("User not found or unable to submit degree.");

            return Ok("Degree submitted successfully.");
        }

        [HttpGet("top-students")]
        public async Task<IActionResult> GetTopTenStudent(GradeLevel gradeLevel)
        {
            var result = await _adminService.GetTopTenStudentsAsync(gradeLevel);
            if (result == null)
                return BadRequest("Something Bad Occured pease try again later");
            return Ok(result);
        }
    }
}
