namespace Engli3m.Application.DTOs.EQuiz
{
    public class EQuizResultDto
    {
        public string QuizTitle { get; set; } = string.Empty;
        public double Score { get; set; }
        public List<QuestionResultDto> Questions { get; set; } = [];
    }
}
