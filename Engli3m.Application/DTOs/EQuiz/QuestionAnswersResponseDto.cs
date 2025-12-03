using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class QuestionAnswersResponseDto
    {
        public int AnswerId { get; set; }
        [Required]
        [StringLength(20, ErrorMessage = "Answer content cannot exceed 20 characters.")]
        public string Answer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public bool IsCorrect { get; set; }
    }
}