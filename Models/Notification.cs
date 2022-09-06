using System.ComponentModel.DataAnnotations;

namespace plateforme.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
