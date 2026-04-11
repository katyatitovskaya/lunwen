using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class UserTagDto
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; }
        public int StudyStartYear { get; set; }
        public TagStudyPhase Phase { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
