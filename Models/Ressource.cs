using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace plateforme.Models
{
    public class Ressource
    {
        [Key]
        public int Id { get; set; }
        public string NomRes { get; set; }
        public Contenu Contenu { get; set; }
        [ForeignKey("Contenu")]
        public int ContenuId { get; set; }
    }
}
