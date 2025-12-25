using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class NotebookDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Text { get; set; }

        public DateTime CreationDate { get; set; }

        public bool CanEdit { get; set; }

        public List<NotebookUserDto> Users { get; set; } = new();
    }

    public class NotebookUserDto
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Nickname { get; set; }

        public StudyGroupRole Role { get; set; }
    }

}


