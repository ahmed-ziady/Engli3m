using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Engli3m.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProtectedController : ControllerBase
    {
        [HttpGet("open")]
        [AllowAnonymous]
        public IActionResult OpenEndpoint() => Ok("Public endpoint");

        [HttpGet("authenticated")]
        [Authorize] // Requires any authenticated user
        public IActionResult ForAuthenticated()
        {
            // Get user ID from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok($"Hello authenticated user! Your ID is: {userId}");
        }

        [HttpGet("student-only")]
        [Authorize(Roles = "Student")]
        public IActionResult StudentOnly() => Ok("Students only");

        [HttpGet("teacher-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult TeacherOnly() => Ok("Teachers only");

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly() => Ok("Admins only");
    }
}