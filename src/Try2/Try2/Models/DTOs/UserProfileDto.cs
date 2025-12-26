using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Nickname { get; set; }

        public UserRole Role { get; set; }

        public string? Bio { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public IFormFile? ProfilePhoto { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }

        public int FriendsCount { get; set; }

        public bool IsCurrentUser { get; set; }

        public bool IsFollowedByCurrentUser { get; set; }

        public List<PostDto> Posts { get; set; } = new();
    }

}
