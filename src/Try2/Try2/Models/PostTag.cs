using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Try2.Models
{
    public class PostTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MainTagId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int Priority { get; set; }

        [ForeignKey(nameof(MainTagId))]
        public Tag Tag { get; set; }

        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; }
    }
}
