using Engli3m.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class QuestionUserResponseDto
    {
        public int QuestionId { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Question title cannot exceed 200 characters.")]
        public string QuestionContent { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public QuestionType Type { get; set; }
        public double Points { get; set; } = 1.0;
        public List<QuestionAnswersUserResponseDto> Answers { get; set; } = [];
    }
}