using System.ComponentModel.DataAnnotations;
using Try2.Models.Enums;

namespace Try2.Models.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(30)]
        public string Username { get; set; }

        [Required, MaxLength(50)]
        public string Nickname { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

    }
}
