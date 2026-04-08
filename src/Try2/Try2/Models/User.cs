using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Try2.Models.Enums;

namespace Try2.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        [RegularExpression(@"^[A-Za-z0-9_]+$", 
            ErrorMessage = "Имя пользователя может содержать только буквы английского алфавита, цифры и символ подчеркивания")]
        public string Username { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nickname { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [MaxLength(200)]
        public string? Bio {  get; set; }

        public string? ProfilePhoto { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;

        public string? EmailConfirmationCode { get; set; }

        public DateTime? EmailConfirmationCodeExpiry { get; set; }

        [Required]
        public bool IsAdmin { get; set; } = false;

        public ICollection<StudyGroup> StudyGroups { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostLike> Likes { get; set; }
        public ICollection<CommentLike> CommentLikes {  get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
        public ICollection<Subscription> Followers { get; set; }
        public ICollection<UserTag> Tags { get; set; }
    }
}
