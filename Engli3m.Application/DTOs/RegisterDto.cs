using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs
{
    public record RegisterDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "First name must be between 3 and 10 characters.")]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "First name must be between 3 and 10 characters.")]

        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Grade is required.")]
        public string Grade { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;

    }
}
