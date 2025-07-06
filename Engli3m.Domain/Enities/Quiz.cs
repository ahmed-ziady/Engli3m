namespace Engli3m.Domain.Enities
{
    public class Quiz
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string QuizUrl { get; set; } = string.Empty;      // Path under wwwroot/uploads/quizzes

        // Relationships
        public int LectureId { get; set; }
        public Lecture Lecture { get; set; } = null!;

        public User Admin { get; set; } = null!;
        public int AdminId { get; set; }
        public ICollection<QuizSubmission> Submissions { get; set; } = [];
    }
}
