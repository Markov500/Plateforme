using System.ComponentModel.DataAnnotations;

namespace plateforme.Models.ViewModels
{
    public class SujetVM
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Vous devez donner un titre à votre sujet")]
        public string Titre { get; set; }

        [Required(ErrorMessage = "Expliquez votre problème")]
        public string description { get; set; }

        public string visiblite { get; set; }
        [DataType(DataType.Url)]
        public string? lien { get; set; }
        public IFormFile? fichier { get; set; }

    }
}
