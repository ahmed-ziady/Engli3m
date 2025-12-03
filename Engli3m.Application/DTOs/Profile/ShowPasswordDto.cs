namespace Engli3m.Application.DTOs.Profile
{
    using System.ComponentModel.DataAnnotations;

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "معرّف المستخدم مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "معرّف المستخدم غير صالح")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
            ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير، حرف صغير، رقم، ورمز خاص واحد على الأقل")]
        public required string NewPassword { get; set; }
    }
}
