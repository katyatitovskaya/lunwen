using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Try2.Models
{
    public class NotebookPage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Theme { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        [Required]
        public string? JsonContents { get; set; }

        [Required]
        public int NotebookId { get; set; }

        [ForeignKey("NotebookId")]
        public Notebook Notebook { get; set; }


    }
}
