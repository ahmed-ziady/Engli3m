using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.Profile
{
    public class ProfileImageDto
    {
        [Required(ErrorMessage = "Profile image is required.")]
        public IFormFile ProfileImage { get; set; } = null!;
    }
}
