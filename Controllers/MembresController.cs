using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using plateforme.Models.ViewModels;
using System.Text.Json;

namespace plateforme.Controllers
{
    public class MembresController : Controller
    {
        private readonly plateformeContext _context;
        private readonly IWebHostEnvironment _env;
        public MembresController(plateformeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _env = webHostEnvironment;
        }

        private bool MembreExists(int id)
        {
            return (_context.Membre?.Any(e => e.Id == id)).GetValueOrDefault();
            
        }

        //*************************************************************************************
        //*************************************************************************************
        //*
        //*############################# LES FONCTIONS DE CONNEXION ###########################
        //*
        //*************************************************************************************
        //*************************************************************************************

        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }

        //GET Membre/Signin
        public IActionResult Login()
        {
            ViewData["Title"] = "Connectez-vous";
            ViewBag.login = _context.Parametre.Where(p => p.Id == "login").First().Valeur;
            HttpContext.Session.Remove("user");

            return View();
        }

        //POST Membre/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Identifiant,mpass")] LoginVM membre)
        {
            ViewData["Title"] = "Connectez-vous";
            ViewBag.login = _context.Parametre.Where(p => p.Id == "login").First().Valeur;
            if (ModelState.IsValid)
            {
                Membre member = await _context.Membre.FirstOrDefaultAsync(m => m.Mail == membre.Identifiant || m.Username == membre.Identifiant);
                if (member != null)
                {
                    var check = Membre.VerifyPassword(member.Password, membre.mpass);
                    if (check)
                    {
                        if (member.Etat == "actif")
                        {
                            HttpContext.Session.SetString("user", JsonSerializer.Serialize(member));
                            member.LastLogin = DateTime.Now;
                            _context.Membre.Update(member);
                            await _context.SaveChangesAsync();
                            switch (member.Profil)
                            {
                                case "superadmin":
                                    return RedirectToAction("Index", "Admins");

                                case "admin":
                                    return RedirectToAction("Index", "Admins");

                                case "membre":
                                    return RedirectToAction("Index", "Publications");

                                case "moderateur":
                                    return RedirectToAction("Index", "Moderateurs");

                            }
                        }
                        else if (member.Etat == "inactif")
                        {
                            HttpContext.Session.SetString("user", JsonSerializer.Serialize(member));
                            return RedirectToAction("ChangePassword", "Membres");
                        }
                        else
                        {
                            ViewBag.erreur = "Vous ne pouvez accéder au site";
                            return View();
                        }

                    }
                    else
                    {
                        ViewBag.erreur = "Login ou mot de passe incorrect";
                        return View();
                    }
                }
                else
                {
                    ViewBag.erreur = "Login ou mot de passe incorrect";
                    return View();

                }

            }

            ViewBag.erreur = "Veuillez réessayer";
            return View();
        }

        //GET: Membres/Compte
        public async Task<IActionResult> Compte()
        {
            ViewBag.photo = _context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;
            if (MembreConnecte().Etat == "actif")
            {
                ViewData["Title"] = "Modifier mes informations";

                ViewBag.membre = await _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
        }

        //POST: Membres/Compte
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compte(membreModifVM m)
        {
            
            if(MembreConnecte().Etat == "actif")
            {
                ViewData["Title"] = "Modifier mes informations";
                ViewBag.membre = await _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                var membre = await _context.Membre.FindAsync(MembreConnecte().Id);
                if (membre == null) RedirectToAction("Login", "Membres");



                if (_context.Membre.Any(d => d.Mail == m.Mail) && m.Mail != membre.Mail)
                {
                    ModelState.AddModelError("Mail", "Cette adresse mail d'utilisateur existe déjà");
                }
                if (m.Photoprofil != null && !Fichier.checkType(m.Photoprofil).Equals("photo"))
                {
                    ModelState.AddModelError("Photoprofil", "Le fichier choisit n'est pas pris en charge");
                }
                if (ModelState.IsValid)
                {

                    membre.Nom = m.Nom;
                    membre.Prenom = m.Prenom;
                    membre.Mail = m.Mail;
                    membre.Poste = m.Poste;
                    membre.Bio = m.bio;


                    if (m.Photoprofil == null)
                    {
                        var sup = Request.Form["sup"];
                        if (sup == "oui")
                        {
                            membre.PhotoProfil = _context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;

                        }
                    }
                    else
                    {
                        var filename = membre.Nom + membre.Prenom + membre.Poste + new FileInfo(m.Photoprofil.FileName).Extension;
                        var chemin = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "profil", filename);
                        
                            
                        var cheminSup = Path.Combine(_env.ContentRootPath + "wwwroot" + membre.PhotoProfil);


                        if (System.IO.File.Exists(cheminSup))
                        {
                            System.IO.File.Delete(cheminSup);
                        }
                        
                        membre.PhotoProfil = "/images/profil/" + filename;

                        using (var stream = new FileStream(chemin, FileMode.Create))
                        {
                            m.Photoprofil.CopyTo(stream);
                        }
                    }

                    _context.Membre.Update(membre);
                    await _context.SaveChangesAsync();
                    return View();
                }



                return View();
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

        }

        //GET: Membres/ChangePassword
        public IActionResult ChangePassword()
        {
            if (MembreConnecte().Etat == "actif" || MembreConnecte().Etat == "inactif")
            {
                ViewData["Title"] = "Modifier mon mot de passe";
                ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
        }

        //POST: Membres/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(PasswordVM mpass)
        {

            if(MembreConnecte().Etat == "actif" || MembreConnecte().Etat == "inactif")
            {
                ViewData["Title"] = "Modifier mon mot de passe";
                ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                Membre m = _context.Membre.Find(MembreConnecte().Id);
                if (m == null) return NotFound();

                if (!Membre.VerifyPassword(m.Password, mpass.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Mot de passe erronée");
                }

                if (mpass.NewPassword != mpass.ConfPassword)
                {
                    ModelState.AddModelError("ConfPasssword", "Ces deux saisies doivent être identiques");
                    ModelState.AddModelError("NewPasssword", "Ces deux saisies doivent être identiques");
                }
                if (mpass.Username == null)
                {
                    ModelState.AddModelError("Username", "Vous devez choisir un nom d'utilisateur");
                }
                if (_context.Membre.Any(d => d.Username == mpass.Username && d.Id != MembreConnecte().Id))
                {
                    ModelState.AddModelError("Username", "Ce nom d'utilisateur existe déjà");
                }

                if (ModelState.IsValid)
                {
                    m.Password = Membre.HashPassword(mpass.NewPassword);
                    m.Username = mpass.Username;
                    if (m.Etat == "inactif") m.Etat = "actif";

                    _context.Membre.Update(m);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Login", "Membres");
                }
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
        }


        
    }
}
