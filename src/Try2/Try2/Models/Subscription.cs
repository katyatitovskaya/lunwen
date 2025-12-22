using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Try2.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TargetUserId { get; set; }

        [Required]
        public int FollowerId { get; set; }

        [ForeignKey("FollowerId")]
        public User Follower { get; set; }

        [ForeignKey("TargetUserId")]
        public User TargetUser { get; set; }
    }
}
