using Try2.Models;
using Try2.Models.DTOs;

namespace Try2.ViewModels
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Role { get; set; }
        public string Bio { get; set; }

        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

        public bool IsCurrentUser { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }

        public List<PostDto> Posts { get; set; } = new();
    }
}
