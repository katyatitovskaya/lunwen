using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class SearchRequestDto
    {
        public string? Query { get; set; } = "";
        public string SearchType { get; set; } = "All";
        public int? TagId { get; set; }
        public TagStudyPhase? Phase { get; set; }
    }

    public class SearchResultDto
    {
        public List<UserSearchResultDto> Users { get; set; } = new();
        public List<PostSearchResultDto> Posts { get; set; } = new();
    }

    public class UserSearchResultDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
        public List<UserTagDto> Tags { get; set; } = new();
    }

    public class PostSearchResultDto
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorNickname { get; set; }
        public string? AuthorProfilePhoto { get; set; }
        public string Text { get; set; }
        public DateTime PublicationDate { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<PostTagDto> Tags { get; set; } = new();
    }

    public class TagSuggestionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UsageCount { get; set; }
    }
}
