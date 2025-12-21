using MyLunWen.Models.Enums;

namespace MyLunWen.Models.ViewModels
{
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public List<string> AvailableRoles { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public UserRole Role { get; set; }
        public bool IsLockedOut { get; set; }
        public List<string> Roles { get; set; }
    }
}
