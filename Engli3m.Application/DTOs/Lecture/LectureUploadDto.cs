using Engli3m.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.Lecture
{
    public record LectureUploadDto
    {
        [Required]
        [Display(Name = "Post Title")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters long.")]
        public string Title { get; set; } = string.Empty;
        public GradeLevel Grade { get; set; }
        public IFormFile Video { get; set; } = null!;
    }

}
