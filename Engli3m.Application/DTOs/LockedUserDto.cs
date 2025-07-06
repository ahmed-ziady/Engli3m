using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs
{
    public record LockedUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public GradeLevel? Grade { get; set; }
    }
}
