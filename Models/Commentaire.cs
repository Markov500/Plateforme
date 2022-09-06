﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace plateforme.Models
{
    public class Commentaire
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime DateEnvoi { get; set; } = DateTime.Now;
        public DateTime? DateModif { get; set; }

        public Membre Membre { get; set; }
        public Contenu Contenu { get; set; }
        public Commentaire? CommentaireParent { get; set; }
        [ForeignKey("Membre")]
        public int MembreId { get; set; }
        [ForeignKey("Contenu")]
        public int ContenuId { get; set; }
        [ForeignKey("CommentaireParent")]
        public int? CommentaireId { get; set; }
        public virtual List<Commentaire> Reponses { get; set; } = new List<Commentaire>();

        public string AffichageDate()
        {


            DateTime date = this.DateEnvoi;
            string format = "";
            if (this.DateModif != null)
            {
                date = (DateTime)this.DateModif;
                format = "modifié ";
            }

            if (DateTime.Now.Day == date.Day)
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

                format += jour + " " + DateFormat.Mois(date.Month) + " " + date.Year + " à " + heure + ":" + min;
            }

            return format;
        }

    }
}