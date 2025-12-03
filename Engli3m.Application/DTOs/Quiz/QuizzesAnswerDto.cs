using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs.Quiz
{
    public record QuizzesAnswerDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string LectureTitle { get; set; } = null!;
        public string QuizAnwerURl { get; set; } = null!;
        public GradeLevel? Grade { get; set; }
    }
}
