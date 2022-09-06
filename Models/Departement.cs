using System.ComponentModel.DataAnnotations;

namespace plateforme.Models
{
    public class Departement
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name ="Département"),StringLength(100)]
        public string NomDept { get; set; }
        public virtual List<Membre>? Membres { get; set; }
    }
}
