using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLunWen.Models
{
    public class Like
    {
        [Key]
        public int LikeId { get; set; }

        [Required]
        public string UserId {  get; set; }

        [Required]
        public string ObjectType { get; set; }

        [Required]
        public int ObjectId {  get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
