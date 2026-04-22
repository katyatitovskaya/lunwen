using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class NotebookDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreationDate { get; set; }

        public bool CanEdit { get; set; }

        public List<NotebookUserDto> Users { get; set; } = new();
        public List<NotebookPageSummaryDto> Pages { get; set; } = new();
    }

    public class NotebookPageSummaryDto
    {
        public int Id { get; set; }
        public string Theme { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class NotebookPageDto
    {
        public int Id { get; set; }
        public string Theme { get; set; }
        public DateTime CreationDate { get; set; }
        public string? JsonContents { get; set; }
        public int NotebookId { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int? PreviousPageId { get; set; }
        public int? NextPageId { get; set; }
    }

    public class NotebookUserDto
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Nickname { get; set; }

        public StudyGroupRole Role { get; set; }
    }

}


