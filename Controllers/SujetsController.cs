using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using plateforme.Models.ViewModels;

namespace plateforme.Controllers
{
    public class SujetsController : Controller
    {
        private readonly plateformeContext _context;
        private readonly IWebHostEnvironment _env;

        public SujetsController(plateformeContext context, IWebHostEnvironment webHostEnvironment)
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
            if (MembreConnecte().Etat == "actif")
            {
                ViewData["Title"] = "Créer un sujet de discussion";
                ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(m => m.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
            
        }



        // POST : Sujets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( SujetVM sub)
        {
            if (MembreConnecte().Etat == "actif")
            {
                ViewData["Title"] = "Créer un sujet de discussion";
                ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(m => m.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);


                if (sub.fichier != null)
                {
                    if (Fichier.checkType(sub.fichier) == "invalide")
                    {
                        ModelState.AddModelError("fichier", "Ce type de fichier n'est pas pris en charge");
                    }
                    else if (!Fichier.checkTaille(sub.fichier))
                    {
                        ModelState.AddModelError("fichier", "Ce fichier est trop lourd");
                    }
                }
                if (ModelState.IsValid)
                {
                    //Enregistrement du sujet
                    Sujet sujet = new Sujet();
                    sujet.MembreId = MembreConnecte().Id;
                    sujet.Visibilite = Convert.ToInt16(sub.visiblite);
                    sujet.Description = sub.description;
                    sujet.DateCreation = DateTime.Now;
                    sujet.Titre = sub.Titre;
                    _context.Sujet.Add(sujet);
                    await _context.SaveChangesAsync();

                    //Récupération du sujet pour ajouter les ressources s'il y en a
                    sujet = _context.Sujet.Include(s => s.Membre).OrderBy(s => s.DateCreation).Last();

                    //Ajout d'un lien s'il y en a
                    if (sub.lien != null)
                    {
                        Lien l = new Lien();
                        l.Url = sub.lien;
                        l.ContenuId = sujet.Id;
                        l.NomRes = sub.lien;
                        _context.Lien.Add(l);
                        await _context.SaveChangesAsync();
                    }

                    //Ajout d'un fichier s'il y en a
                    if (sub.fichier != null)
                    {
                        Fichier f = new Fichier();
                        var filename = MembreConnecte().Nom + MembreConnecte().Prenom + DateTime.Now.Millisecond + new FileInfo(sub.fichier.FileName).Extension;
                        var chemin = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "sujet", filename);
                        f.Chemin = "/images/sujet/" + filename;
                        f.NomRes = sub.fichier.FileName;
                        f.Type = Fichier.checkType(sub.fichier);
                        f.ContenuId = sujet.Id;

                        using (var stream = new FileStream(chemin, FileMode.Create))
                        {
                            sub.fichier.CopyTo(stream);
                        }
                        _context.Fichier?.Add(f);
                        await _context.SaveChangesAsync();
                    }

                    //Choix des membres à qui la notification sera adressée
                    List<Membre> destinataire = new List<Membre>();
                    if (sujet.Visibilite == 1)
                    {
                        destinataire = _context.Membre.Where(m => m.Id != sujet.MembreId).ToList();
                    }
                    else
                    {
                        destinataire = _context.Membre.Where(m => m.DepartementId == sujet.Membre.DepartementId).ToList();
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

                    return RedirectToAction("Index", "Sujets");

                }

                return View(sub);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
            
        }



        public async Task<IActionResult> Index()
        {
            ViewBag.photo = _context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(m => m.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                ViewData["Title"] = "Les sujets existants";
                List<Sujet>? sujets = Sujet.ContenuAccessible(_context.Sujet.Include(s => s.Membre).Include(p => p.Signals).Include(s => s.Commentaires).ToList(), MembreConnecte());
                try
                {
                    string? recherche = Request.Form["search"];

                    return _context.Sujet != null ?
                               View(sujets.Where(p => p.Titre.ToLower().Contains(recherche.ToLower()) || p.Description.ToLower().Contains(recherche.ToLower()))) :
                             Problem("Entity set 'plateformeContext.Publication'  is null.");

                }

                catch
                {
                    return _context.Sujet != null ?
                              View(sujets) :
                              Problem("Entity set 'plateformeContext.Sujet'  is null.");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

            
            
        }

    





        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.photo = _context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(m => m.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (id == null) return NotFound();
                Sujet sujet = _context.Sujet?.Include(s => s.Membre).Include(s => s.Ressources).Include(s => s.Commentaires).ThenInclude(c => c.Membre).FirstOrDefault(s => s.Id == id);
                if (sujet == null) return NotFound();
                ViewData["Title"] = sujet.Titre;
                return View(sujet);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
            
        }




        //POST: Sujets/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([Bind("ContenuId", "CommentaireId", "MembreId", "Description")] Commentaire commentaire)
        {
            //int[] tab = new int[1];
            //tab[0] = commentaire.ContenuId;
       
            if (commentaire.Description == null)
                return RedirectToAction("Details","Sujets", new { id = commentaire.ContenuId });
            
            _context.Commentaire?.Add(commentaire);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Sujets", new { id = commentaire.ContenuId });

        }



        public async Task<IActionResult> Cloturer(int? id)
        {
            if(id == null) return NotFound();
            else
            {
                
                var sujet = await _context.Sujet.FindAsync(id);
                if (sujet == null) return NotFound();
                else
                {
                    sujet.Cloture = true;
                    _context.Sujet.Update(sujet);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Sujets", new { id = sujet.Id });
                }
            }
        }
        private bool SujetExists(int id)
        {
            return (_context.Sujet?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}
