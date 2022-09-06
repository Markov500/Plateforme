using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace plateforme.Models
{
    public class Signal
    {
        [Key]
        public int Id { get; set; }
        public Membre Membre { get; set; }
        public Contenu Contenu { get; set; }
        [ForeignKey("Membre")]
        public int MembreId { get; set; }
        [ForeignKey("Contenu")]
        public int ContenuId { get; set; }
        
    }
}
