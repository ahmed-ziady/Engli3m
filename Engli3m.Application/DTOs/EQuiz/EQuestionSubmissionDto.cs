using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class EQuestionSubmissionDto
    {
        public int QuestionId { get; set; }
        public int? SelectedAnswerId { get; set; } //  MCQ  TrueFalse
        [StringLength(15, ErrorMessage = "Written Answer must be at most 15 characters.")]
        public string? WrittenAnswer { get; set; } // لو سؤال مقالي

    }
}
