using Engli3m.Application.DTOs.EQuiz;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Engli3m.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EQuizController(IEQuizServices quizServices) : ControllerBase
    {
        [HttpPost("create-quiz")]
        public async Task<IActionResult> CreateEQuizAsync(CreateEQuizDto createEQuizDto)
        {
            if (createEQuizDto == null)
            {
                return BadRequest("لا يمكن أن يكون الاختبار فارغاً.");
            }
            try
            {
                await quizServices.CreateEQuizAsync(createEQuizDto, 1);
                return Ok("تم إنشاء الاختبار بنجاح.");
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"خطأ داخلي في السيرفر: {ex.Message}");
            }
        }

        [HttpGet("allQuizzes")]
        public async Task<IActionResult> GetAllEQuizzesAsync()
        {
            var result = await quizServices.GetAllEQuizzesAsync();
            if (result == null)
                return NotFound("لا يوجد اختبارات حالياً، انتظر قليلاً.");
            return Ok(result);
        }

        [HttpPost("active-quiz/{id}")]
        public async Task<IActionResult> ActiveEQuizByIdAsync(int id)
        {
            if (id <= 0)
                return BadRequest("معرّف الاختبار غير صالح.");
            try
            {
                var isActive = await quizServices.ActiveEQuizByIdAsync(id);
                if (isActive)
                    return Ok("تم تفعيل الاختبار بنجاح.");
                else
                    return NotFound("الاختبار غير موجود أو مُفعل بالفعل.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"خطأ داخلي في السيرفر: {ex.Message}");
            }
        }

        [HttpGet("quiz-by-grade")]
        public async Task<IActionResult> GetEQuizByGradeAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var gradeClaim = User.FindFirst("GradeLevel")?.Value; // 👈 نفس الاسم اللي في التوكن

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var studentId))
                return Unauthorized("تعذر الحصول على معرّف الطالب من التوكن.");

            if (string.IsNullOrEmpty(gradeClaim) || !int.TryParse(gradeClaim, out var gradeInt))
                return Unauthorized("تعذر الحصول على الصف الدراسي من التوكن.");

            if (!Enum.IsDefined(typeof(GradeLevel), gradeInt))
                return BadRequest("الصف الدراسي غير صالح.");

            var gradeEnum = (GradeLevel)gradeInt; // ✅ تحويل int → Enum

            try
            {
                var quiz = await quizServices.GetEQuizByGradeAsync(gradeEnum, studentId);
                if (quiz == null)
                    return NotFound($"لا يوجد اختبار للصف {gradeEnum}.");
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"خطأ داخلي: {ex.Message}");
            }
        }

        [HttpDelete("delete-quiz/{eQuizId}")]
        public async Task<IActionResult> DeleteEQuizAsync(int eQuizId)
        {
            if (eQuizId <= 0)
                return BadRequest("معرّف الاختبار غير صالح.");
            try
            {
                var isDeleted = await quizServices.DeleteEQuizAsync(eQuizId);
                if (isDeleted)
                    return Ok("تم حذف الاختبار بنجاح.");
                else
                    return NotFound("لم يتم العثور على الاختبار.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"خطأ داخلي في السيرفر: {ex.Message}");
            }
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitEQuizAsync([FromBody] EQuizSubmissionDto submissionDto)
        {
            if (submissionDto == null)
                return BadRequest("مطلوب إرسال بيانات الحل.");

            if (submissionDto.Questions == null || submissionDto.Questions.Count == 0)
                return BadRequest("يجب إرسال إجابة واحدة على الأقل.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var studentId))
            {
                return Unauthorized("تعذر الحصول على معرّف الطالب من التوكن.");
            }

            try
            {
                var result = await quizServices.SubmitQuizAsync(submissionDto, studentId);

                if (result)
                    return Ok(new { success = true, message = "تم حفظ الحل بنجاح." });

                return BadRequest(new { success = false, message = "فشل حفظ الحل." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "حدث خطأ غير متوقع.", detail = ex.Message });
            }
        }

        [HttpGet("result/{quizId}")]
        [Authorize]
        public async Task<IActionResult> GetQuizResultAsync(int quizId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var studentId))
                return Unauthorized("تعذر الحصول على معرّف الطالب من التوكن.");

            try
            {
                var result = await quizServices.GetQuizResultAsync(quizId, studentId);
                if (result == null)
                    return NotFound("لا يوجد نتيجة محفوظة لهذا الاختبار.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "حدث خطأ في السيرفر.", detail = ex.Message });
            }
        }
    }
}
