using Engli3m.Application.DTOs.Quiz;
using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs.Lecture
{
    public record LectureWithQuizzesDto
    {
        public int LectureId { get; init; }
        public GradeLevel Grade { get; init; }
        public string LectureTitle { get; init; } = string.Empty;
        public string VideoUrl { get; init; } = string.Empty;
        public List<QuizItemDto> Quizzes { get; init; } = [];
    }
}
