using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.Post
{
    public class CreatePostDto
    {
        [StringLength(2000, ErrorMessage = "Title cannot exceed 2000 characters.")]
        public string Content { get; set; } = string.Empty;
        public List<IFormFile>? MediaUrls { get; set; } = [];

    }
}
