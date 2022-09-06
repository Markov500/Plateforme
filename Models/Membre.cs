using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
namespace plateforme.Models

{
    public class Membre
    {
        [Key]
        public int Id { get; set; }
        [StringLength(60)]
        [RequiredAttribute(ErrorMessage = "Le champ nom est obligatoire")]
        public string Nom { get; set; }
        [StringLength(100)]
        public string? Prenom { get; set; }
        public DateTime? LastLogin { get; set; }
        [RequiredAttribute(ErrorMessage ="Le champ mail est obligatoire"),DataType(DataType.EmailAddress, ErrorMessage ="Le format de votre adresse mail est incorrect"),Display(Name ="Adresse mail")]
        public string Mail { get; set; }
        [StringLength(20)]
        public string? Username { get; set; }
        public string Etat { get; set; }
        [Required]
        public string Profil { get; set; }
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public string? Bio { get; set; }
        [RequiredAttribute(ErrorMessage = "Le champ poste est obligatoire")]
        public string Poste { get; set; }
        public string PhotoProfil { get; set; }
        public virtual  Departement Departement { get; set; }
       
        [ForeignKey("Departement")]
        public int DepartementId { get; set; }
        public virtual List<Contenu> Contenus { get; set; } = new List<Contenu>();
        public virtual List<DetailNotif> Notifications { get; set; } = new List<DetailNotif>();
        //[InverseProperty("MembreId")]
        public virtual List<Avertissement> Avertissements { get; set; } = new List<Avertissement>();

        public static string? HashPassword(string? password)
        {
            if(password == null)
            {
                password = "ab";
            }
            //ÉTAPE 1 Créer le sel de la valeur avec un chiffrement PRNG:
            byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                //ÉTAPE 2 Créer le Rfc2898DeriveBytes et obtenir la valeur de hachage:
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);

                //ÉTAPE 3 Combiner le sel et le mot de passe octets pour une utilisation ultérieure:
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

                //ÉTAPE 4 Tournez le combiné sel+hachage dans une chaîne de caractères pour le stockage
                return Convert.ToBase64String(hashBytes);
           
     
        }

        public static Boolean VerifyPassword(string savedPasswordHash, string? password)
        {
            Boolean test = true;
            if (password == null)
            {
                password = "ab";
            }
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    test = false;

          return test;
        }


        

    }
}
