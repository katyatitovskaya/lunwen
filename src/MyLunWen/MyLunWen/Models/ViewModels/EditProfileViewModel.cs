using System.ComponentModel.DataAnnotations;

namespace MyLunWen.Models.ViewModels
{
    public class EditProfileViewModel
    {
        
        [Display(Name = "Никнейм")]
        public string Nickname { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "О себе")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов")]
        public string About { get; set; }

        [Display(Name = "Фото профиля (URL)")]
        public string ProfilePhoto { get; set; }
    }
}
