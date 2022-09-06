using System.ComponentModel.DataAnnotations;

namespace plateforme.Models.ViewModels
{
    public class PasswordVM
    {
        [Key]
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfPassword { get; set; }
    }
}
