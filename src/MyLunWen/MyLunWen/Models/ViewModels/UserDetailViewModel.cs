using MyLunWen.Models.Enums;

namespace MyLunWen.Models.ViewModels
{
    public class UserDetailViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public UserRole Role { get; set; }
        public string About { get; set; }
        public string ProfilePhoto { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public List<string> Roles { get; set; }
        public List<string> AllRoles { get; set; }
    }
}
