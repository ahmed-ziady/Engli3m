using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs.Profile
{
    public class ProfileResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public GradeLevel? Grade { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
