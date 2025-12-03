namespace Engli3m.Application.DTOs.EQuiz
{
    public class QuestionResultDto
    {
        public string QuestionText { get; set; } = string.Empty;

        public string CorrectAnswer { get; set; } = string.Empty;

        public string? SubmittedAnswer { get; set; }
        public double Points { get; set; }
        public bool IsCorrectAnswer { get; set; }


        public string? AnswerExplanation { get; set; }


    }
}
