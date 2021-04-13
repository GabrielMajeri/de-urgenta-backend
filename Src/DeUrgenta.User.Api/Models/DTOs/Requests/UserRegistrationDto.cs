using System.ComponentModel.DataAnnotations;

namespace DeUrgenta.User.Api.Models.DTOs.Requests
{
    public class UserRegistrationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}