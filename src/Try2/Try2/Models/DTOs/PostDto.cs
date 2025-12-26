namespace Try2.Models.DTOs
{
    public class PostDto
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

        public ICollection<CommentDto> Comments { get; set; }

        public ICollection<PostLike> Likes {  get; set; }
    }

}
