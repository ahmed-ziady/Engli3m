using Engli3m.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Engli3m.Domain.Enities
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public GradeLevel? Grade { get; set; }
        public bool IsLocked { get; set; }
        public bool IsPayed { get; set; }
        public string? ProfilePictureUrl { get; set; } = null;
        public string? CurrentJwtToken { get; set; }
        public string? CleanPassword { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public double NetScore { get; set; }
        public string FcmToken { get; set; } = string.Empty;

        public ICollection<Lecture> LecturesAsAdmin { get; set; } = [];
        public ICollection<Quiz> QuizzesAsAdmin { get; set; } = [];
        public ICollection<QuizSubmission> QuizSubmissions { get; set; } = [];
        public ICollection<Post> Posts { get; set; } = [];
        public ICollection<EQuiz> EQuizzes { get; set; } = [];
        public ICollection<EQuizSubmission> EQuizSubmissions { get; set; } = [];

        public ICollection<FavPost> FavPosts { get; set; } = [];
        public ICollection<VideoProgress> VideoProgressRecords { get; set; } = [];
    }


}

