using System.ComponentModel.DataAnnotations;

namespace Try2.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public ICollection<PostTag> PostTags { get; set; }
        public ICollection<UserTag> UserTags { get; set; }
    }
}
