namespace plateforme.Models
{
    public class Sujet:Contenu
    {
        public Boolean Cloture { get; set; } = false;


        public static List<Sujet>? ContenuAccessible(List<Sujet> listContenu, Membre member)
        {
            List<Sujet> subs = listContenu.
                  Where(p => p.Membre.DepartementId == member.DepartementId || p.Membre.DepartementId != member.DepartementId && p.Visibilite == 1).
                  OrderByDescending(s => s.DateCreation).ToList();
            return subs;
        }

    }
}
