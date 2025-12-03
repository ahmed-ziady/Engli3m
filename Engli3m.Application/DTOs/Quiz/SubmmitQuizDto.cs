using Microsoft.AspNetCore.Http;

namespace Engli3m.Application.DTOs.Quiz
{
    public record SubmmitQuizDto
    {
        public int QuizId { get; set; }
        public required IFormFile AsnwerImage { get; set; }

    }
}
