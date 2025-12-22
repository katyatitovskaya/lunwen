using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Try2.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime PublicationDate { get; set; }

        [ForeignKey("AuthorId")]
        public User Author { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostLike> Likes { get; set; }

    }
}
