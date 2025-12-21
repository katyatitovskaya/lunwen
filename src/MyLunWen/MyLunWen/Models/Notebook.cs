using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLunWen.Models
{
    public class Notebook
    {
        [Key]
        public int NotebookId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        // Внешний ключ
        public string UserId { get; set; }

        // Навигационные свойства
        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<StudyGroup> StudyGroups { get; set; }
    }
}
