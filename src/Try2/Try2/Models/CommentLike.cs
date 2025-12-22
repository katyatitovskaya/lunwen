using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Try2.Models
{
    public class CommentLike
    {
        [Key]
        public int LikeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CommentId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }
    }
}
