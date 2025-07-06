namespace Engli3m.Domain.Enities
{
    public class QuizSubmission
    {
        public int QuizSubmissionId { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public string AnswerUrl { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    }
}
