using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using plateforme.Models;

namespace plateforme.Data
{
    public class plateformeContext : DbContext
    {
        public plateformeContext (DbContextOptions<plateformeContext> options)
            : base(options)
        {
        }
        public DbSet<plateforme.Models.Avertissement>? Avertissement { get; set; }
        public DbSet<plateforme.Models.Contenu>? Contenu { get; set; }
        public DbSet<plateforme.Models.Departement>? Departement { get; set; }
        public DbSet<plateforme.Models.DetailNotif>? DetailNotif { get; set; }
        public DbSet<plateforme.Models.Fichier>? Fichier { get; set; }
        public DbSet<plateforme.Models.Lien>? Lien { get; set; }
        public DbSet<plateforme.Models.Membre>? Membre { get; set; }
        public DbSet<plateforme.Models.Commentaire>? Commentaire { get; set; }
        public DbSet<plateforme.Models.Notification>? Notification { get; set; }
        public DbSet<plateforme.Models.Publication>? Publication { get; set; }
        public DbSet<plateforme.Models.Ressource>? Ressource { get; set; }
        public DbSet<plateforme.Models.Signal>? Signal { get; set; }
        public DbSet<plateforme.Models.Sujet>? Sujet { get; set; }
        public DbSet<plateforme.Models.Parametre>? Parametre { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Membre>(entity => {
                entity.HasIndex(e => e.Mail).IsUnique();
            });

            builder.Entity<Membre>(entity => {
                entity.HasIndex(e => e.Username).IsUnique();
            });

            builder.Entity<Departement>(entity => {
                entity.HasIndex(e => e.NomDept).IsUnique();
            });

            
        }




        

    }
}
