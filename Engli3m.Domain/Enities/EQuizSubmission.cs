namespace Engli3m.Domain.Enities
{
    public class EQuizSubmission
    {
        public int Id { get; set; }
        public int EQuizId { get; set; }
        public EQuiz EQuiz { get; set; } = null!;

        public int StudentId { get; set; }  // UserId للطالب
        public User Student { get; set; } = null!;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public double Score { get; set; }

        public List<EQuestionSubmission> EQuestionSubmissions { get; set; } = [];
    }
}
