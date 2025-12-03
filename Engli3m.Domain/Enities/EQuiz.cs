using Engli3m.Domain.Enums;

namespace Engli3m.Domain.Enities
{
    public class EQuiz
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public GradeLevel Grade { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double TotalPoints { get; set; } = 1.0;
        public TimeOnly Duration { get; set; } = new TimeOnly(0, 10); // Default to 10 minutes
        public int UserID { get; set; }
        public User User { get; set; } = null!;

        public List<Questions> Questions { get; set; } = [];
        public List<EQuizSubmission> Submissions { get; set; } = [];

    }

}
