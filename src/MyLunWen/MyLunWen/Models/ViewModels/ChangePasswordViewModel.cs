using System.ComponentModel.DataAnnotations;

namespace MyLunWen.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Текущий пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Новый пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен быть от {2} до {1} символов", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите новый пароль")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
