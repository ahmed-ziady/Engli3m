// Engli3m.Application.DTOs.EQuiz.EQuizSubmissionDto.cs
using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class EQuizSubmissionDto
    {
        [Required(ErrorMessage = "EQuizId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "EQuizId must be a positive integer.")]
        public int EQuizId { get; set; }

        [Required(ErrorMessage = "Questions are required.")]
        [MinLength(1, ErrorMessage = "At least one question is required.")]
        public List<EQuestionSubmissionDto> Questions { get; set; } = [];
    }
}
