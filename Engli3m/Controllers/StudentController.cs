using Engli3m.Application.DTOs.Lecture;
using Engli3m.Application.DTOs.Quiz;
using Engli3m.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Student")]
public class StudentController(IStudentService studentService) : ControllerBase
{
    private bool TryGetStudentId(out int studentId)
    {
        studentId = 0;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out studentId);
    }

    [HttpGet("lectures-and-quizzes")]
    public async Task<IActionResult> GetAllLecturesAndQuizzes()
    {
        if (!TryGetStudentId(out var studentId))
            return Unauthorized("غير مصرح لك بالدخول، لم يتم التعرف على المستخدم.");

        try
        {
            var result = await studentService.GetAllLecturesAndQuizzes(studentId);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "حدث خطأ أثناء معالجة طلبك، برجاء المحاولة لاحقاً.");
        }
    }

    [HttpPost("submit-answer")]
    public async Task<IActionResult> SubmitAnswer([FromForm] SubmmitQuizDto dto)
    {
        if (!TryGetStudentId(out var studentId))
            return Unauthorized("غير مصرح لك، لم يتم التعرف على هوية الطالب.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await studentService.SubmmitQuizAnswerAsync(dto, studentId);
        if (!success)
            return BadRequest("فشل في حفظ إجابة الاختبار.");

        return Ok(new { Message = "تم إرسال إجابة الاختبار بنجاح, ربما تكون ارسلت حل الاختبار من قبل" });
    }
    [HttpPost("submit-progress")]
    public async Task<IActionResult> SubmitVideoProgress([FromBody] LectureProgressDto lectureProgressDto)
    {
        if (!TryGetStudentId(out var studentId))
            return Unauthorized("غير مصرح لك، لم يتم التعرف على هوية الطالب.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await studentService.SetVideoProgress(studentId, lectureProgressDto);
            return Ok(new { Message = "تم حفظ تقدم الفيديو بنجاح." });
        }
        catch (ApplicationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

}
