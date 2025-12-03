using Engli3m.Application.DTOs;
using Engli3m.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Engli3m.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController(INotificationService _notificationService) : ControllerBase
    {




        [HttpPost("save-fcm-token")]
        public async Task<IActionResult> SaveFcmToken([FromBody] SaveFcmTokenDto dto)
        {
            await _notificationService.SaveFcmTokenAsync(dto);
            return Ok(new { message = "FCM token saved successfully" });
        }

        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser(string token)
        {

            var result = await _notificationService.SendToUserAsync(
                token,
                " Test Notification",
                "This is a test notification from C# backend"
            );

            return Ok(new { MessageId = result });
        }

    }

}

