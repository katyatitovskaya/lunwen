using System.ComponentModel.DataAnnotations;
using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Имя пользователя должно содержать от 3 до 30 символов")]
        [RegularExpression(@"^[A-Za-z0-9_]+$", ErrorMessage = "Имя пользователя может содержать только буквы английского алфавита, цифры и символ подчеркивания (_)")]
        [Display(Name = "Имя пользователя")]
        public string Username { get; set; }

        [Required, MaxLength(50)]
        public string Nickname { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

    }
}
