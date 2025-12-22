using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using MyLunWen.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MyLunWen.Models
{
    public class User: IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Nickname { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public string Email {  get; set; }

        public string ProfilePhoto { get; set; }

        [MaxLength(500)]
        public string About { get; set; }

        public ICollection<Notebook> Notebooks { get; set; }
        public ICollection<StudyGroup> StudyGroups { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Reply> Replies { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
