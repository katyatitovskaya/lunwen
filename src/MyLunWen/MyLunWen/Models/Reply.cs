using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLunWen.Models
{
    public class Reply
    {
        [Key]
        public int ReplyId { get; set; }

        [Required]
        public int CommentId { get; set; }

        [Required]
        public string AuthorId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ReplyText { get; set; }

        public DateTime PublicationTime { get; set; }

        // Навигационные свойства
        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }

        [ForeignKey("AuthorId")]
        public User Author { get; set; }
    }
}
