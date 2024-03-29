﻿using System.ComponentModel.DataAnnotations;

namespace plateforme.Models.ViewModels
{
    public class PublicationVM
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Donnez un titre à votre publication")]
        public string Titre { get; set; }
        [Required(ErrorMessage = "Faites une brève description de votre publication")]
        public string Description { get; set; }
        [DataType(DataType.Url)]
        public string? Lien { get; set; }
        public IFormFile? Fichier { get; set; }
        public string? Tags { get; set; }
        [Required(ErrorMessage = "Choisissez à quel échelle la publication peut être consultée")]
        public string visiblite { get; set; }
        [Required(ErrorMessage = "Choisissez le type de votre publication")]
        public string type { get; set; }
        public DateTime? DateDeb { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
