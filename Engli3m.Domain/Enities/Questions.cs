using Engli3m.Domain.Enums;

namespace Engli3m.Domain.Enities
{
    public class Questions
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public QuestionType Type { get; set; }

        public double Points { get; set; } = 1.0;

        public int EQuizId { get; set; }
        public EQuiz EQuiz { get; set; } = null!;

        public List<QuestionsAnswer> Answers { get; set; } = [];
        public List<EQuestionSubmission> EQuestionSubmissions { get; set; } = [];

    }

}
