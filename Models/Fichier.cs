using System.ComponentModel.DataAnnotations;

namespace plateforme.Models
{
    public class Fichier:Ressource
    {
        public string Chemin { get; set; }
        public string Type { get; set; }

        

        public static string checkType( IFormFile file)
        {
            List<string> ListVideo = new List<string>()
            {
                "video/mpeg",
                "video/mp2t",
                "video/webm",
                "video/3gpp",
                "video/3gp",
                "video/mp4"

            };
            List<string> ListPhoto = new List<string>()
            {
                "image/jpeg",
                "image/png",
                "image/tiff",
                "image/avif",
                "image/gif",
                "image/bmp",
                "image/svg+xml",
                "image/webp"
            };
            if (ListPhoto.Contains(file.ContentType))
            {
                return "photo";
            }
            else if (ListVideo.Contains(file.ContentType))
            {
                return "video";
            }
            else if (file.ContentType.ToLower() == ".pdf")
            {
                return "pdf";
            }
            else
            {
                return "invalide";
            }
        }

        public static Boolean checkTaille(IFormFile file)
        {
            if(file.Length /(1024*1024) > 10)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
