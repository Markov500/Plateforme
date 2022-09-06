using System.ComponentModel.DataAnnotations;

namespace plateforme.Models
{
    public class Lien:Ressource
    {
        [DataType(DataType.Url),Display(Name ="Lien")]
        public string Url { get; set; }
    }
}
