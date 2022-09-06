using System.ComponentModel.DataAnnotations;

namespace plateforme.Models
{
    public class Publication:Contenu
    {
        public string Tags { get; set; } = "";
        [Required]
        [StringLength(10)]
        public string Type { get; set; }
        public DateTime? DateDeb { get; set; }
        public DateTime? DateFin { get; set; }



        public static List<Publication>? ContenuAccessible(List<Publication> listContenu, Membre member)
        {
            List<Publication> pubs = listContenu.
                  Where(p => p.Membre.DepartementId == member.DepartementId || p.Membre.DepartementId != member.DepartementId && p.Visibilite == 1).
                  OrderByDescending(s => s.DateCreation).ToList();
            return pubs;
        }

    }
}
