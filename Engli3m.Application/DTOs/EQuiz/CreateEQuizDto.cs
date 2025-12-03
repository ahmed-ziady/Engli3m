using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs.EQuiz
{
    public class CreateEQuizDto
    {

        public string Title { get; set; } = string.Empty;
        public GradeLevel Grade { get; set; }
        public bool IsActive { get; set; } = false;
        public TimeOnly Duration { get; set; } = new TimeOnly(0, 10); // Default to 10 minutes
        public List<CreateQuestionDto> Questions { get; set; } = [];
    }

}
