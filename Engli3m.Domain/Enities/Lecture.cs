using Engli3m.Domain.Enums;

namespace Engli3m.Domain.Enities
{

    public class Lecture
    {
        public int LectureId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string VideoUrl { get; set; } = string.Empty;
        public GradeLevel Grade { get; set; }
        public bool IsActive { get; set; }
        // Relationships
        public int AdminId { get; set; }
        public User Admin { get; set; } = null!;


        public ICollection<Quiz> Quizzes { get; set; } = [];
        public ICollection<VideoProgress> VideoProgressRecords { get; set; } = [];
    }
}
