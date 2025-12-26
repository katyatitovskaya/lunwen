using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Try2.Models
{
    public class Notebook
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description {  get; set; }

        public string? Text { get; set; }

        public DateTime CreationDate { get; set; }

        public ICollection<StudyGroup> StudyGroups { get; set; }

    }
}
