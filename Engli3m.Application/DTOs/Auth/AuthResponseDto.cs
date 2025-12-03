using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs.Auth
{
    public record AuthResponseDto

    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public GradeLevel? GradeLevel { get; set; }
        public List<string>? Roles { get; set; }
        public List<string>? Errors { get; set; }
    }
}
