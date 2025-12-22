using MyLunWen.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyLunWen.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Юзернейм обязателен")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Должен быть от 3 до 30 символов")]
        [Display(Name = "Юзернейм")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Никнейм")]
        public string Nickname { get; set; }

        [Required(ErrorMessage = "Роль обязательна")]
        [Display(Name = "Роль")]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 символов")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Подтвердить email")]
        public bool EmailConfirmed { get; set; } = true;

        public List<string> AvailableRoles { get; set; }
    }
}
