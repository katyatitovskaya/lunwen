using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Try2.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public int PostId { get; set; }

        public DateTime PublicationTime { get; set; }

        public int? ParentCommentId {  get; set; }

        [ForeignKey("AuthorId")]
        public User User { get; set; }  

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public Comment ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; }

        public ICollection<CommentLike> Likes { get; set; }

    }
}
