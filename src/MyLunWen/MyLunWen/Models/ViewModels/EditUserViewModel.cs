using MyLunWen.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyLunWen.Models.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Юзернейм обязателен")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Должен быть от 3 до 30 символов")]
        [Display(Name = "Юзернейм")]
        public string Username { get; set; }

        [Display(Name = "Никнейм")]
        public string Nickname { get; set; }

        [Required(ErrorMessage = "Роль обязательна")]
        [Display(Name = "Роль")]
        public UserRole Role { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "О себе")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов")]
        public string About { get; set; }

        [Display(Name = "Фото профиля (URL)")]
        public string ProfilePhoto { get; set; }

        public List<string> AvailableRoles { get; set; }
    }
}
