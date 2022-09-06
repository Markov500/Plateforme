using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace plateforme.Models
{
    public class Avertissement
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Veuillez saisir la note d'avertissement")]
        public string Note { get; set; }
        public DateTime DateEnvoi { get; set; }= DateTime.Now;
        public virtual Membre Membre { get; set; }
        public int ModerateurId { get; set; }
        [ForeignKey("Membre")]
        public int MembreId { get; set; }
    }
}
