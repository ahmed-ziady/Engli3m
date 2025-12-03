using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class EQuizResponeDto
    {
        public int EQuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public GradeLevel Grade { get; set; }
        public bool IsActive { get; set; } = true;
        public double TotalPoints { get; set; } = 1.0;
        public TimeOnly Duration { get; set; } = new TimeOnly();
        public List<QuestionResponseDto> Questions { get; set; } = [];
    }

}
