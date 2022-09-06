using System.ComponentModel.DataAnnotations;

namespace plateforme.Models.ViewModels
{
    public class membreModifVM
    {
        [Key]
        public int Id { get; set; }
        [StringLength(60)]
        [Required(ErrorMessage = "Le champ nom est obligatoire")]
        public string Nom { get; set; }
        [StringLength(100)]
        public string? Prenom { get; set; }
        [Required(ErrorMessage = "Le champ mail est obligatoire"), DataType(DataType.EmailAddress, ErrorMessage = "Le format de votre adresse mail est incorrect"), Display(Name = "Adresse mail")]
        public string Mail { get; set; }
        [StringLength(20,ErrorMessage ="Votre nom d'utilisateur ne peut contenir plus de 20 caractères")]
        public string? Username { get; set; }
        public string? bio { get; set; }
        [Required(ErrorMessage = "Le champ poste est obligatoire")]
        public string Poste { get; set; }

        public IFormFile? Photoprofil { get; set; }


    }
}
