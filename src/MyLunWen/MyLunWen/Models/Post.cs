using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace MyLunWen.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public string AuthorId { get; set; }

        [Required]
        public string PostText { get; set; }

        public DateTime PublicationTime { get; set; }

        // Навигационные свойства

        [ForeignKey("AuthorId")]
        public User Author { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}

