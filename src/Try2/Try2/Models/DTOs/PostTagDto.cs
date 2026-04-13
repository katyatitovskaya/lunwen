using System.ComponentModel.DataAnnotations;

namespace Try2.Models.DTOs
{
    public class PostTagDto
    {
        public int Id { get; set; }
        public int MainTagId { get; set; }
        public string TagName { get; set; }
        public int PostId { get; set; }
        public int Priority { get; set; }
    }
}
