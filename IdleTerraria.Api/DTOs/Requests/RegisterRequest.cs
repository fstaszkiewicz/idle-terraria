using System.ComponentModel.DataAnnotations;

namespace IdleTerraria.Api.DTOs.Requests
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 3)]
        [RegularExpression(
            "^[a-zA-Z0-9_]+$",
            ErrorMessage = "Username może zawierać wyłącznie litery, cyfry i znak _.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}