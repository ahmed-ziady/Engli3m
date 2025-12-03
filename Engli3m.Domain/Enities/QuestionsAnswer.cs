namespace Engli3m.Domain.Enities
{
    public class QuestionsAnswer
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
        public string? Explanation { get; set; }

        public int QuestionId { get; set; }
        public Questions Question { get; set; } = null!;
    }

}
