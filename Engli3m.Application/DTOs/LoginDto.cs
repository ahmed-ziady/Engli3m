using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs
{
    public record LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MinLength(5, ErrorMessage = "Email must be at least 5 characters")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = default!;


        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]


        public string Password { get; set; } = default!;
    }
}
