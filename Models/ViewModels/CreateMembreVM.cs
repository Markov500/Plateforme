using System.ComponentModel.DataAnnotations;

namespace plateforme.Models.ViewModels
{
    public class CreateMembreVM
    {
        [Key]
        public int CodeMembre { get; set; }
        [Required(ErrorMessage= "Le champ Nom est obligatoire")]
        public string Nom { get; set; }
        public string? Prenom { get; set; }
        [ Required(ErrorMessage = "Le champ Mail est obligatoire"), DataType(DataType.EmailAddress)]
        public string Mail { get; set; }
        [Required(ErrorMessage = "Vous devez attribuer un profil")]
        public string Profil { get; set; }
        [Required(ErrorMessage = "Le champ poste est obligatoire")]
        public string Poste { get; set; }
        [Required]
        public int CodeDept { get; set; }

    }
}