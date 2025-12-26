namespace Try2.Models.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string Text { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorNickname { get; set; }

        public DateTime PublicationTime { get; set; }

        public ICollection<CommentLike> Likes { get; set; }

        public ICollection<CommentDto> Replies { get; set; }
    }
}
