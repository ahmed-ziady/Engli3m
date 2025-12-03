using System.ComponentModel.DataAnnotations;

namespace Engli3m.Application.DTOs.Profile
{
    public class UpdateUserNameDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 10 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 10 characters.")]
        public string LastName { get; set; } = string.Empty;
    }
}
