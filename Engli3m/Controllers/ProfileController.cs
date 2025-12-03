using Engli3m.Application.DTOs.Profile;
using Engli3m.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Engli3m.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfile profileServices;
        private readonly IEQuizServices quizServices;
        private readonly IStudentService studentService;

        public ProfileController(IProfile profileServices, IEQuizServices quizServices, IStudentService studentService)
        {
            this.profileServices = profileServices;
            this.quizServices = quizServices;
            this.studentService = studentService;
        }

        private bool TryGetStudentId(out int studentId)
        {
            studentId = 0;
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(claimValue) && int.TryParse(claimValue, out studentId);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            try
            {
                var result = await profileServices.GetProfileAsync(userId);
                return Ok(result);
            }
            catch
            {
                return StatusCode(StatusCodes.Status404NotFound, "المستخدم غير موجود!");
            }
        }

        [HttpPut("UpdatePhoneNumber")]
        public async Task<IActionResult> UpdatePhoneNumber(string phoneNumber)
        {
            if (!TryGetStudentId(out var userId))
                return Unauthorized("غير مصرح لك بالدخول!");

            var user = await profileServices.UpdatePhoneNumber(userId, phoneNumber);
            return user
                ? Ok("تم تحديث رقم الهاتف بنجاح")
                : StatusCode(StatusCodes.Status404NotFound, "المستخدم غير موجود!");
        }

        [HttpPut("UpdateProfilePicture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] ProfileImageDto imageDto)
        {
            if (!TryGetStudentId(out var userId))
                return Unauthorized("غير مصرح لك بالدخول!");

            try
            {
                var result = await profileServices.UpdateProfilePicture(userId, imageDto);
                return result
                    ? Ok("تم تحديث صورة الملف الشخصي بنجاح")
                    : StatusCode(StatusCodes.Status404NotFound, "المستخدم غير موجود!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status417ExpectationFailed, $"حدث خطأ: {ex.Message}");
            }
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("بيانات غير صالحة");

            var result = await profileServices.ResetPasswordAsync(dto.UserId, dto.NewPassword);

            if (result is null)
                return NotFound("المستخدم غير موجود!");

            return Ok(new { Success = true, Message = result });
        }

        [HttpGet("submitted-quizzes")]
        public async Task<IActionResult> GetAllQuizResultAsync(int studentId)
        {

            var result = await quizServices.GetAllQuizResultAsync(studentId);
            return result == null
                ? NotFound("لا يوجد اختبارات مجابة!")
                : Ok(result);
        }

        [HttpGet("favPost")]
        public async Task<IActionResult> GetAllFavPosts(int studentId)
        {
            var result = await profileServices.GetFavPostAsync(studentId);
            return result == null
                ? NotFound("لا يوجد اي منشورات مفضلة!")
                : Ok(result);
        }

        [HttpGet("submitted-progress")]
        public async Task<IActionResult> GetSubmittedProgress(int studentId)
        {

            try
            {
                var progressList = await profileServices.GetSubmittedProgressAsync(studentId);

                return !progressList.Any()
                    ? NotFound("لا يوجد تقدم مسجل لهذا الطالب.")
                    : Ok(progressList);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "حدث خطأ أثناء معالجة طلبك، برجاء المحاولة لاحقاً.");
            }
        }
    }
}
