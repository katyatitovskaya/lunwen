using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLunWen.Models
{
    public class Subscription
    {
        [Key]
        public int SubscribtionId { get; set; }

        [Required]
        public string SubscriberId { get; set; }

        [Required]
        public string TargetUserId { get; set; }

        [ForeignKey("SubscriberId")]
        public User Subscriber { get; set; }

        [ForeignKey("TargetUserId")]
        public User TargetUser { get; set; }

    }
}
