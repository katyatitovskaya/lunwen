using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Try2.Models.Enums;

namespace Try2.Models
{
    public class UserTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MainTagId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1950, int.MaxValue)]
        public int StudyStartYear { get; set; }

        [Required]
        public TagStudyPhase Phase { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("MainTagId")]
        public Tag Tag { get; set; }

    }
}
