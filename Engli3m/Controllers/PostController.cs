using Engli3m.Application.DTOs.Post;
using Engli3m.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Engli3m.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController(IPostServices postServices) : ControllerBase
    {

        [HttpPost("create-post")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto createPostDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { رسالة = "غير مصرح لك بالدخول." });

            var success = await postServices.CreatePostAsync(userId, createPostDto);
            if (!success)
                return BadRequest(new { رسالة = "فشل في إنشاء المنشور." });

            return Ok(new { رسالة = "تم إنشاء المنشور بنجاح." });
        }

        [HttpDelete("delete-post/{postId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var success = await postServices.DeletePostAsync(postId);
            if (!success)
                return NotFound(new { رسالة = "المنشور غير موجود أو لم يتم حذفه." });

            return Ok(new { رسالة = "تم حذف المنشور بنجاح." });
        }

        [HttpGet("get-all-posts")]
        public async Task<IActionResult> GetAllPosts(int pageNumber = 1, int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized("تعذر الحصول على رقم الطالب من التوكن.");

            var posts = await postServices.GetAllPostAsync(pageNumber, pageSize, currentUserId);
            if (posts == null || posts.Count == 0)
                return NotFound(new { رسالة = "لا يوجد منشورات." });

            return Ok(posts);
        }
        [HttpPost("mark-fav-post")]
        [Authorize]
        public async Task<IActionResult> MarkPostAsAFav(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var studentId))
                return Unauthorized("تعذر الحصول على رقم الطالب من التوكن.");

            var result = await postServices.MarkPostFavAsync(postId, studentId);
            if (!result)
                return BadRequest("لم يتم اضافة المنشور الي الصفحة الشحصية ربما يكون موجود بالفعل!");
            return Ok("تم الاضافة الي الصفحة الشخصية");
        }

    }
}
