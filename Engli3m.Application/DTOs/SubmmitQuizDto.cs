using Microsoft.AspNetCore.Http;

namespace Engli3m.Application.DTOs
{
    public record SubmmitQuizDto
    {
        public int QuizId { get; set; }
        public required IFormFile AsnwerImage { get; set; }

    }
}
