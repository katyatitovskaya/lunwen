using System.ComponentModel.DataAnnotations;

namespace Try2.Models.DTOs
{
    public class TagDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsConfirmed { get; set; } = false;
        public int PostsCount { get; set; }
        public int UsersCount { get; set; }
        public int TotalUsage => PostsCount + UsersCount;
    }
}
