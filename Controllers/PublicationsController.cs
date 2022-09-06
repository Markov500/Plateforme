using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using plateforme.Models.ViewModels;
using System.Text.Json;

namespace plateforme.Controllers
{
    public class PublicationsController : Controller
    {
        private readonly plateformeContext _context;
        private readonly IWebHostEnvironment _env;

        public PublicationsController(plateformeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _env = webHostEnvironment;
        }
        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }





        // GET : Sujets/Create
        public IActionResult Create()
        {
            if(MembreConnecte().Etat == "actif")
            {
                ViewData["Title"] = "Faire une publication";
                ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                return View();
            }
            else
            {
                return RedirectToAction("Login","Membres");
            }
            
        }






        // POST : Sujets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublicationVM pub)
        {
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                ViewData["Title"] = "Faire une publication";
                if (pub == null) return NotFound();
                if (pub.type == "event")
                {
                    if (pub.DateDeb == null)
                    {
                        ModelState.AddModelError("DateDeb", "Un évènement doit toujours avoir une date de début");
                    }
                    if (pub.DateDeb < DateTime.Now)
                    {
                        ModelState.AddModelError("DateDeb", "L'évènement ne peut être programmé à une date antérieur");
                    }
                    if (pub.DateDeb > pub.DateFin)
                    {
                        ModelState.AddModelError("DateDeb", "La date de fin ne peut être antérieur à la date de début");
                    }
                }
                if (pub.Tags != null)
                {
                    if (!pub.Tags.Contains("#"))
                    {
                        ModelState.AddModelError("Tags", "Le format est incorrect");
                    }
                }
                if (pub.Fichier != null)
                {
                    if (Fichier.checkType(pub.Fichier) == "invalide")
                    {
                        ModelState.AddModelError("Fichier", "Ce type de fichier n'est pas pris en charge");
                    }
                    else if (!Fichier.checkTaille(pub.Fichier))
                    {
                        ModelState.AddModelError("Fichier", "Ce fichier est trop lourd");
                    }
                }
                if (ModelState.IsValid)
                {
                    //Enregistrement de la publication
                    Publication publication = new Publication();
                    publication.MembreId = MembreConnecte().Id;
                    publication.Visibilite = Convert.ToInt16(pub.visiblite);
                    publication.DateCreation = DateTime.Now;
                    publication.Description = pub.Description;
                    publication.Titre = pub.Titre;
                    publication.Type = pub.type;

                    if (publication.Type == "event")
                    {
                        publication.DateDeb = pub.DateDeb;
                        publication.DateFin = pub.DateFin;
                    }
                    if (pub.Tags != null)
                    {
                        publication.Tags = pub.Tags;
                    }

                    _context.Publication.Add(publication);
                    await _context.SaveChangesAsync();

                    //Récupération de la publication pour ajouter les ressources s'il y en a
                    publication = _context.Publication.Include(p => p.Membre).OrderBy(s => s.DateCreation).Last();

                    //Ajout d'un lien s'il y en a
                    if (pub.Lien != null)
                    {
                        Lien l = new Lien();
                        l.Url = pub.Lien;
                        l.ContenuId = publication.Id;
                        l.NomRes = pub.Lien;
                        _context.Lien.Add(l);
                        await _context.SaveChangesAsync();
                    }

                    //Ajout d'un fichier s'il y en a
                    if (pub.Fichier != null)
                    {
                        Fichier f = new Fichier();
                        var filename = MembreConnecte().Nom + MembreConnecte().Prenom + DateTime.Now.Millisecond + new FileInfo(pub.Fichier.FileName).Extension;
                        var chemin = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "publication", filename);
                        f.NomRes = "Publication de " + MembreConnecte().Username;
                        f.Chemin = "/images/publication/" + filename;
                        f.Type = Fichier.checkType(pub.Fichier);
                        f.ContenuId = publication.Id;


                        using (var stream = new FileStream(chemin, FileMode.Create))
                        {
                            pub.Fichier.CopyTo(stream);
                        }
                        _context.Fichier?.Add(f);
                        await _context.SaveChangesAsync();
                    }

                    //Choix des membres à qui la notification sera adressée
                    List<Membre> destinataire = new List<Membre>();
                    if (publication.Visibilite == 1)
                    {
                        destinataire = _context.Membre.Where(m => m.Id != publication.MembreId).ToList();
                    }
                    else
                    {
                        destinataire = _context.Membre.Where(m => m.DepartementId == publication.Membre.DepartementId).ToList();
                    }

                    if (destinataire.Count() > 0)
                    {
                        //Création de la notification
                        Notification notif = new Notification();
                        notif.Description = MembreConnecte().Nom + " " + MembreConnecte().Prenom + " a créé un nouveau sujet de discussion";
                        _context.Notification?.Add(notif);
                        await _context.SaveChangesAsync();

                        notif = _context.Notification.OrderBy(n => n.Id).Last();



                        //Envoi de la notification aux destinataires
                        foreach (var i in destinataire)
                        {
                            DetailNotif details = new DetailNotif();
                            details.NoificationId = notif.Id;
                            details.MembreId = i.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();
                        }
                    }

                    return RedirectToAction("Index", "Publications");

                }

                return View(pub);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }


            
           
            
        }





        public async Task<IActionResult> Index()
        {
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(m => m.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                ViewBag.photo =_context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;
                ViewData["Title"] = "Les sujets existants";
                List<Publication>? pubs = Publication.ContenuAccessible(_context.Publication.Include(s => s.Membre).Include(p => p.Ressources).
                                              Include(p => p.Signals).Include(s => s.Commentaires).ThenInclude(p => p.Membre).Include(s => s.Commentaires).
                                              ThenInclude(s => s.Reponses).ToList(), MembreConnecte());
                try
                {
                    string? recherche = Request.Form["search"];

                    return _context.Publication != null ?
                               View(pubs.Where(p => p.Titre.ToLower().Contains(recherche.ToLower()) || p.Description.ToLower().Contains(recherche.ToLower())  )) :
                             Problem("Entity set 'plateformeContext.Publication'  is null.");

                }

                catch
                {
                    return _context.Publication != null ?
                              View(pubs) :
                              Problem("Entity set 'plateformeContext.Publication'  is null.");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }



        }




        //POST: Publications/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(Commentaire commentaire)
        {
            Publication pub = await _context.Publication.FindAsync(commentaire.ContenuId); 
            
            if (commentaire.Description == null)
                if(pub.Type =="pub")
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Détails");
            

            _context.Commentaire?.Add(commentaire);
                await _context.SaveChangesAsync();
            if (pub.Type == "pub")
                return RedirectToAction("Index");
            else
                return RedirectToAction("Details", new { id = pub.Id });

        }


        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.photo = _context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(m => m.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (id == null) return NotFound();
                Publication publication = _context.Publication?.Include(s => s.Membre).Include(s => s.Ressources).Include(s => s.Commentaires).ThenInclude(c => c.Membre).FirstOrDefault(s => s.Id == id);
                if (publication == null) return NotFound();
                ViewData["Title"] = publication.Titre;
                return View(publication);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

        }

        private bool SujetExists(int id)
        {
            return (_context.Sujet?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

