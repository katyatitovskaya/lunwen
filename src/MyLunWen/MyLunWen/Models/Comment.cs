using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLunWen.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AuthorId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public string CommentText { get; set; }

        public DateTime PublicationTime { get; set; }

        // Навигационные свойства
        [ForeignKey("AuthorId")]
        public User Author { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }
        public ICollection<Reply> Replies { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}
