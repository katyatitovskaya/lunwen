using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Try2.Models.Enums;

namespace Try2.Models
{
    public class StudyGroup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NotebookId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public StudyGroupRole UserRole { get; set; }

        // Навигационные свойства
        [ForeignKey("NotebookId")]
        public Notebook Notebook { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
