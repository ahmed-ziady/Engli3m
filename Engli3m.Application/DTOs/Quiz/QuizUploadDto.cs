using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.Quiz
{
    public record QuizUploadDto
    {
        [Required]
        [Display(Name = "Post Title")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters long.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9\s]+$", ErrorMessage = "Title can only contain Arabic/English letters, numbers, and spaces.")]
        public string Title { get; set; } = string.Empty;
        public int LectureId { get; set; }
        public IFormFile Image { get; set; } = null!;
    }

}
