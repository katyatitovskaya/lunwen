using MyLunWen.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyLunWen.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [Display(Name = "Имя пользователя")]
        public string Username { get; set; }

        [Display(Name = "Никнейм")]
        public string Nickname { get; set; }

        [Required(ErrorMessage = "Роль обязательна")]
        [Display(Name = "Роль")]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен быть от {2} до {1} символов", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "О себе")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов")]
        public string About { get; set; }
    }
}
