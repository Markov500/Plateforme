using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace plateforme.Migrations
{
    public partial class first : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Departement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomDept = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departement", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Parametre",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Libelle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valeur = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametre", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Membre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prenom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastLogin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Mail = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Username = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Etat = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Profil = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bio = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Poste = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoProfil = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DepartementId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Membre_Departement_DepartementId",
                        column: x => x.DepartementId,
                        principalTable: "Departement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Avertissement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Note = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEnvoi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModerateurId = table.Column<int>(type: "int", nullable: false),
                    MembreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avertissement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avertissement_Membre_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Contenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Visibilite = table.Column<int>(type: "int", nullable: false),
                    MembreId = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tags = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDeb = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateFin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Cloture = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contenu_Membre_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetailNotif",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateEnvoi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Lu = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NoificationId = table.Column<int>(type: "int", nullable: false),
                    MembreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailNotif", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailNotif_Membre_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetailNotif_Notification_NoificationId",
                        column: x => x.NoificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Commentaire",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEnvoi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModif = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MembreId = table.Column<int>(type: "int", nullable: false),
                    ContenuId = table.Column<int>(type: "int", nullable: false),
                    CommentaireId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commentaire", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commentaire_Commentaire_CommentaireId",
                        column: x => x.CommentaireId,
                        principalTable: "Commentaire",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Commentaire_Contenu_ContenuId",
                        column: x => x.ContenuId,
                        principalTable: "Contenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commentaire_Membre_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ressource",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomRes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContenuId = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Chemin = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ressource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ressource_Contenu_ContenuId",
                        column: x => x.ContenuId,
                        principalTable: "Contenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Signal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MembreId = table.Column<int>(type: "int", nullable: false),
                    ContenuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signal_Contenu_ContenuId",
                        column: x => x.ContenuId,
                        principalTable: "Contenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signal_Membre_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Avertissement_MembreId",
                table: "Avertissement",
                column: "MembreId");

            migrationBuilder.CreateIndex(
                name: "IX_Commentaire_CommentaireId",
                table: "Commentaire",
                column: "CommentaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Commentaire_ContenuId",
                table: "Commentaire",
                column: "ContenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Commentaire_MembreId",
                table: "Commentaire",
                column: "MembreId");

            migrationBuilder.CreateIndex(
                name: "IX_Contenu_MembreId",
                table: "Contenu",
                column: "MembreId");

            migrationBuilder.CreateIndex(
                name: "IX_Departement_NomDept",
                table: "Departement",
                column: "NomDept",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailNotif_MembreId",
                table: "DetailNotif",
                column: "MembreId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailNotif_NoificationId",
                table: "DetailNotif",
                column: "NoificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Membre_DepartementId",
                table: "Membre",
                column: "DepartementId");

            migrationBuilder.CreateIndex(
                name: "IX_Membre_Mail",
                table: "Membre",
                column: "Mail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Membre_Username",
                table: "Membre",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ressource_ContenuId",
                table: "Ressource",
                column: "ContenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_ContenuId",
                table: "Signal",
                column: "ContenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_MembreId",
                table: "Signal",
                column: "MembreId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avertissement");

            migrationBuilder.DropTable(
                name: "Commentaire");

            migrationBuilder.DropTable(
                name: "DetailNotif");

            migrationBuilder.DropTable(
                name: "Parametre");

            migrationBuilder.DropTable(
                name: "Ressource");

            migrationBuilder.DropTable(
                name: "Signal");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Contenu");

            migrationBuilder.DropTable(
                name: "Membre");

            migrationBuilder.DropTable(
                name: "Departement");
        }
    }
}
