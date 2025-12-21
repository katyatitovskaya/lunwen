using MyLunWen.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLunWen.Models
{
    public class StudyGroup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NotebookId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public StudyGroupUserRole UserRole { get; set; }

        // Навигационные свойства
        [ForeignKey("NotebookId")]
        public Notebook Notebook { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        
    }
}
