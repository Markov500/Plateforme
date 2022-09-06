using System.ComponentModel.DataAnnotations;

namespace plateforme.Models
{
    public class Parametre
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [Display(Name = "Paramètre")]
        public string Libelle { get; set; }
        [Required]
        public string Valeur { get; set; }
        public string Type { get; set; }
    }
}
