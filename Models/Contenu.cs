using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace plateforme.Models
{
    public class Contenu
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(150)]
        public string Titre { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }
        [Required]
        public int Visibilite { get; set; }
        public Membre Membre { get; set; }
        [ForeignKey("Membre")]
        public int MembreId { get; set; }
        public virtual List<Commentaire> Commentaires { get; set; } = new List<Commentaire>();
        public virtual List<Ressource> Ressources { get; set; } = new List<Ressource>();
        public virtual List<Signal> Signals { get; set; } = new List<Signal>();




        public static List<Contenu>? ContenuAccessible(List<Contenu> listContenu, Membre member)
        {
            List<Contenu> contenus = listContenu.
                  Where(p => p.Membre.DepartementId == member.DepartementId || p.Membre.DepartementId != member.DepartementId && p.Visibilite == 1).
                  OrderByDescending(s => s.DateCreation).ToList();
            return contenus;
        }

        public string AffichageDate()
        {


            DateTime date = this.DateCreation;
            string format = "";
            if (this.DateModification != null)
            {
                date = (DateTime)this.DateModification;
                format = "modifié ";
            }

            if(DateTime.Now.Day == date.Day)
            {
                if (DateTime.Now.Hour - date.Hour > 0)
                {
                    format += "Il y a ";
                    format += DateTime.Now.Hour - date.Hour + " heure(s)";

                }

                else if (DateTime.Now.Minute - date.Minute > 0)
                {
                    format += "Il y a ";
                    format += DateTime.Now.Minute - date.Minute + " minute(s)";
                }

                else
                {
                    format += "Maintenant";
                }
            }
            else if (DateTime.Now.Day - date.Day == 1)
            {
                var min = (date.Minute < 10) ? "0" + date.Minute : "" + date.Minute;
                var heure = (date.Hour < 10) ? "0" + date.Hour : "" + date.Hour;
                format += "Hier à " + heure + ":" + min;
            }
            else
            {
                var min = (date.Minute < 10) ? "0" + date.Minute : "" + date.Minute;
                var heure = (date.Hour < 10) ? "0" + date.Hour : "" + date.Hour;
                var jour = (date.Day < 10) ? "0" + date.Day : "" + date.Day;
            
                format += jour +" "+ DateFormat.Mois(date.Month) +" "+date.Year+" à "+ heure + ":" + min;
            }

            return format;
        }



    }
}
