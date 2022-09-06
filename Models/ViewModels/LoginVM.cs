using System.ComponentModel.DataAnnotations;

namespace plateforme.Models.ViewModels
{
    public class LoginVM
    {
        [Key]
        public int Id { get; set; }
      
        public string? Identifiant { get; set; }
        [DataType(DataType.Password, ErrorMessage = " Mot de passe incorrect ")]
        public string? mpass { get; set; }
    }
}
