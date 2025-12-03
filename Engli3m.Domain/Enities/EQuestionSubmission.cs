namespace Engli3m.Domain.Enities
{
    public class EQuestionSubmission
    {
        public int Id { get; set; }

        public int EQuizSubmissionId { get; set; }
        public EQuizSubmission EQuizSubmission { get; set; } = null!;

        public int QuestionId { get; set; }
        public Questions Question { get; set; } = null!;

        public int? SelectedAnswerId { get; set; } // MCQ/TrueFalse
        public QuestionsAnswer? SelectedAnswer { get; set; }

        public string? WrittenAnswer { get; set; } // Essay answer

        public double EarnedPoints { get; set; }


    }
}
