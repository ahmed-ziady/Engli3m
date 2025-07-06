namespace Engli3m.Application.DTOs
{
    public record QuizItemDto
    {
        public int QuizId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string ImageUrl { get; init; } = string.Empty;
    }
}
