using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class CreateQuestionAnswersDto
    {
        [Required]
        [StringLength(20, ErrorMessage = "Answer content cannot exceed 20 characters.")]
        public string Answer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public bool IsCorrect { get; set; } = false;
    }
}