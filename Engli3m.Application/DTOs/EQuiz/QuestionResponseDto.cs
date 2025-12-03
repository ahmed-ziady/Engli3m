using Engli3m.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class QuestionResponseDto
    {
        public int QuestionId { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Question title cannot exceed 200 characters.")]
        public string QuestionContent { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public QuestionType Type { get; set; }
        public double Points { get; set; } = 1.0;
        public List<QuestionAnswersResponseDto> Answers { get; set; } = [];
    }
}