using MyLunWen.Models.Enums;

namespace MyLunWen.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Username { get; set; }
        public string Nickname { get; set; }
        public UserRole Role { get; set; }
        public string About { get; set; }
        public string ProfilePhoto { get; set; }
    }
}
