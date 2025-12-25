using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class NotebookDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public StudyGroupRole UserRole { get; set; }
    }

}
