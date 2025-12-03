using Engli3m.Domain.Enums;

namespace Engli3m.Application.DTOs.Auth
{
    public class TopTenStudentsDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public GradeLevel GradeLevel { get; set; }
        public double NetScore { get; set; }
        public int Rank { get; set; }
    }
}
