using Engli3m.Application.DTOs;
using Engli3m.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Engli3m.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthServices _authServices) : ControllerBase
    {


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authServices.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var result = await _authServices.LoginAsync(dto);
                if (result == null)
                    return Unauthorized(new { message = "Invalid email or password." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Return 400 BadRequest with JSON body
                return BadRequest(new { message = ex.Message });
            }
        }




        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null || !int.TryParse(idClaim, out var userId))
                return Unauthorized(new { Error = "Invalid user" });

            var ok = await _authServices.Logout(userId);
            if (!ok)
                return BadRequest(new { Error = "Logout failed" });

            return NoContent();
        }
    }
}