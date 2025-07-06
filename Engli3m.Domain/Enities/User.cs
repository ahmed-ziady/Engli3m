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
        public string? CurrentJwtToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public ICollection<Lecture> LecturesAsAdmin { get; set; } = [];
        public ICollection<Quiz> QuizzesAsAdmin { get; set; } = [];
        public ICollection<QuizSubmission> QuizSubmissions { get; set; } = [];
    }


}

