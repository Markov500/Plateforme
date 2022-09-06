using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using plateforme.Models.ViewModels;
using SmartBreadcrumbs.Attributes;


namespace plateforme.Controllers
{
    
    public class AdminsController : Controller
    {
        private readonly plateformeContext _context;

        public AdminsController(plateformeContext context)
        {
            _context = context;
        }

        private bool MembreExists(int id)
        {
          return (_context.Membre?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }







        //***************************************************************************************
        //***************************************************************************************
        //*                                                                                    **
        //*############################### LES FONCTIONS SUPERADMIN ###########################**
        //*                                                                                    **
        //***************************************************************************************
        //***************************************************************************************


        // GET: Admins/Create
        [Breadcrumb("ViewData.Title")]
        public IActionResult Create()
        {
           
            if (MembreConnecte().Etat == "actif" )
            {
                if(MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.erreur = "";
                    ViewBag.membre = _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    if (ViewBag.membre == null)
                    {
                        RedirectToAction("Login", "Membres");
                    }
                    ViewData["CodeDept"] = new SelectList(_context.Departement, "Id", "NomDept");
                    ViewData["Title"] = "Ajouter un compte";
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
        }



        // POST: Admins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Create([Bind("CodeMembre,Nom,Prenom,Mail,Profil,Poste,CodeDept")] CreateMembreVM m)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewData["Title"] = "Ajouter un compte";
                    ViewBag.membre = _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    if (ViewBag.membre == null)
                    {
                        RedirectToAction("Login", "Membres");
                    }
                    var test = _context.Membre.Any(x => x.Mail == m.Mail);
                    if (test)
                    {
                        ModelState.AddModelError("Mail", "Il existe déjà un compte avec cet adresse mail");
                    }

                    if (await _context.Departement.FindAsync(m.CodeDept) == null)
                    {
                        ModelState.AddModelError("CodeDept", "Choisissez un département");
                    }



                    if (ModelState.IsValid)
                    {
                        ViewBag.erreur = "";
                        Membre membre = new Membre();
                        membre.Nom = m.Nom;
                        membre.Prenom = m.Prenom;
                        membre.Profil = m.Profil;
                        membre.Mail = m.Mail;
                        membre.Poste = m.Poste;
                        membre.Etat = "inactif";
                        //membre.PhotoProfil = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", "parametre", "avatar.jpg");
                        membre.PhotoProfil = _context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;
                        string lien = _context.Parametre.Where(p => p.Id == "plateformeUrl").First().Valeur;
                        string password = genererPassword();
                        membre.Password = Membre.HashPassword(password);
                        membre.DepartementId = m.CodeDept;
                        membre.Departement = await _context.Departement.FindAsync(m.CodeDept);
                        string body = "<h1>Bienvenue " + membre.Nom + " " + membre.Prenom + "</h1>" +
                                        "<p>Votre compte vient d'être ajouté à la platefome avec pour mot de passe  <b>" + password +
                                        "</b><br/>   <a href=" + lien + " >Cliquez ici pour accéder à la plateforme</a></p>";
                        try
                        {
                            EnvoiMail(membre.Mail, body, "Compte ajouté");
                            _context.Add(membre);
                            await _context.SaveChangesAsync();


                            Notification notif = new Notification();
                            string profil = "membre";
                            if (membre.Profil == "admin")
                                profil = "administrateur";
                            else if (membre.Profil == "moderateur")
                                profil = "modérateur";
                            notif.Description = "Vous avez ajouter un compte " + profil + " pour " + membre.Nom + " " + membre.Prenom +
                                "au département " + _context.Departement.Find(membre.DepartementId).NomDept;
                            _context.Notification?.Add(notif);
                            await _context.SaveChangesAsync();


                            notif = _context.Notification.OrderBy(n => n.Id).Last();

                            DetailNotif details = new DetailNotif();
                            details.MembreId = MembreConnecte().Id;
                            details.NoificationId = notif.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();
                        }
                        catch
                        {
                            Notification notif = new Notification();
                            notif.Description = "Un problème est survenue empêchant l'envoi du mail, veuillez vérifier votre connexion internet puis réessaiyer";
                            _context.Notification?.Add(notif);
                            await _context.SaveChangesAsync();

                            ViewBag.erreur = "Un problème est survenue empêchant l'envoi du mail, veuillez vérifier votre connexion internet puis réessaiyer";
                            notif = _context.Notification.OrderBy(n => n.Id).Last();

                            DetailNotif details = new DetailNotif();
                            details.MembreId = MembreConnecte().Id;
                            details.NoificationId = notif.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();
                        }


                        return RedirectToAction("Index", "Admins");
                    }

                    ViewData["CodeDept"] = new SelectList(_context.Departement, "Id", "NomDept", m.CodeDept);
                    return View(m);
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

            
        }

        //GET Admins/Privilege
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Privilege(int? id)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.membre =await _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre?.Include(m => m.Departement).FirstOrDefault(p => p.Id == id);
                    if (m == null) return NotFound();

                    return View(m);
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

           
        }



        //POST Admins/Privilege
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Privilege(Membre membre)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre?.Include(m => m.Departement).FirstOrDefault(p => p.Id == membre.Id);
                    if (m == null) return NotFound();
                    m.Profil = Request.Form["Profil"];
                    try
                    {
                        _context.Update(m);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MembreExists(m.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }



            
        }

        //GET Admins/Desactive
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Desactive(int? id)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.membre = await _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre?.Include(m => m.Departement).FirstOrDefault(p => p.Id == id);
                    if (m == null) return NotFound();

                    return View(m);
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

            
        }



        //POST Admins/Desactive
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Desactive(Membre membre)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre?.Include(m => m.Departement).FirstOrDefault(p => p.Id == membre.Id);
                    if (m == null) return NotFound();
                    if(m.Etat != "desactive")
                            m.Etat = "desactive";
                    else
                        m.Etat = "actif";
                    try
                    {
                        _context.Update(m);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MembreExists(m.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

            
        }


        //GET Admins/Supprime
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Supprime(int? id)
        {

            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre?.Include(m => m.Departement).FirstOrDefault(p => p.Id == id);
                    if (m == null) return NotFound();

                    return View(m);
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }


            
        }



        //POST Admins/Supprime
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Supprime(Membre membre)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.membre = _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre?.Include(m => m.Departement).FirstOrDefault(p => p.Id == membre.Id);
                    if (m == null) return NotFound();
                    m.Etat = "supprime";
                    try
                    {
                        _context.Update(m);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MembreExists(m.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

           
        }



        //GET Admins/Index
        [DefaultBreadcrumb(Title = "Accueil")]
        public async Task<IActionResult> Index()
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.max = _context.Parametre.Where(p => p.Id == "DureeMaxInactifEnMois").First().Valeur;

                    ViewData["Title"] = "Liste des membres";
                    ViewBag.membre = _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    List<Membre>? membres = new List<Membre>();
                    if (m.Profil == "superadmin")
                    {
                        membres = await _context.Membre.Include(m => m.Avertissements).Where(m => m.Id != MembreConnecte().Id && m.Etat != "supprime").
                            Include(d => d.Departement).OrderByDescending(m => m.Id).ToListAsync();

                    }
                    else
                    {
                        membres = await _context.Membre.Include(m => m.Avertissements).Where(m => m.Profil != "superadmin" && m.Profil != "admin" && m.DepartementId == MembreConnecte().DepartementId && m.Etat != "supprime").
                            Include(d => d.Departement).OrderByDescending(m => m.Id).ToListAsync();


                    }
                    return _context.Membre != null ?
                                View(membres) :
                                Problem("Entity set 'plateformeContext.Membre'  is null.");
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }



            
        }


        //GET Admins/Desactive
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Zero(int? id)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewBag.membre = await _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    var m = _context.Membre?.Include(m => m.Departement).FirstOrDefault(p => p.Id == id);
                    if (m == null) return NotFound();

                    return View(m);
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Zero(Membre membre)
        {

            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "admin")
                {
                    ViewData["Title"] = "Remetre à zéro";
                    ViewBag.membre = _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefault(m => m.Id == MembreConnecte().Id);
                    if (ViewBag.membre == null)
                    {
                        RedirectToAction("Login", "Membres");
                    }
                        membre.Etat = "inactif";
                        membre.Username = null;
                        

                        membre.PhotoProfil = _context.Parametre.Where(p => p.Id == "PhotoProfilParDefaut").First().Valeur;
                        string lien = _context.Parametre.Where(p => p.Id == "plateformeUrl").First().Valeur;
                        string password = genererPassword();
                        membre.Password = Membre.HashPassword(password);
                      


                    string body = "<h1>" + membre.Nom + " " + membre.Prenom + "</h1>" +
                                        "<p>Votre compte a été réinitialiser, seul vos contenus publiés sur la plateforme ont été gardé <br/>" +
                                        " Veuillez <a href=" + lien + " >cliquez ici pour accéder à la plateforme</a></p> puis connectez-vous avec les identifiant suivant  " +
                                        "Login : <b>" + membre.Mail + "</b> <br/> " +
                                         "Mot de passe :  <b>" + password + "</b>";

                        try
                        {
                            List<DetailNotif> listNotif = _context.Membre.Include(n => n.Notifications).FirstOrDefault(m => m.Id == membre.Id).Notifications;
                            foreach (var i in listNotif)
                            {
                                _context.DetailNotif.Remove(i);

                            }

                            List<Avertissement> listAv = _context.Membre.Include(n => n.Avertissements).FirstOrDefault(m => m.Id == membre.Id).Avertissements;
                            foreach (var i in listAv)
                            {
                                _context.Avertissement.Remove(i);

                            }
                            EnvoiMail(membre.Mail, body, "Compte remis à zéro");
                            _context.Membre.Update(membre);


                            Notification notif = new Notification();
                            
                            notif.Description = "Vous avez ajouter réinitialisez le compte de "+ membre.Nom+" "+membre.Prenom +" du  département " + _context.Departement.Find(membre.DepartementId).NomDept;
                            _context.Notification?.Add(notif);


                            notif = _context.Notification.OrderBy(n => n.Id).Last();

                            DetailNotif details = new DetailNotif();
                            details.MembreId = MembreConnecte().Id;
                            details.NoificationId = notif.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();
                        }
                        catch
                        {
                            Notification notif = new Notification();
                            notif.Description = "Un problème est survenue empêchant l'envoi du mail et la remise à zéro du compte, veuillez vérifier votre connexion internet puis réessaiyer";
                            _context.Notification?.Add(notif);


                            notif = _context.Notification.OrderBy(n => n.Id).Last();

                            DetailNotif details = new DetailNotif();
                            details.MembreId = MembreConnecte().Id;
                            details.NoificationId = notif.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();
                        }


                        return RedirectToAction("Index", "Admins");
                
                }
                else
                {
                    return RedirectToAction("Login", "Membres");
                }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

        }



        //***************************************************************************************
        //***************************************************************************************
        //*                                                                                    **
        //*############################### LES FONCTIONS ANNEXES ###########################**
        //*                                                                                    **
        //***************************************************************************************
        //***************************************************************************************



        public void EnvoiMail(string adr, string body, string subject)
        {


            MailMessage mail = new MailMessage();
            
            mail.Priority = MailPriority.High;
            mail.IsBodyHtml = true;
            mail.Body = body;
            mail.Subject = subject;
            Parametre email =  _context.Parametre.Where(p => p.Id == "mailAdresse").First();
            Parametre password = _context.Parametre.Where(p => p.Id == "mailPassword").First();
            Parametre port = _context.Parametre.Where(p => p.Id == "mailPort").First();
            mail.From = new MailAddress(email.Valeur);
            mail.To.Add(new MailAddress(adr));
            Console.WriteLine(port.Valeur);
            Console.WriteLine(email.Valeur);
            Console.WriteLine(password.Valeur);
            SmtpClient client = new SmtpClient("smtp.gmail.com", Convert.ToInt32(port.Valeur));
           
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            client.Credentials = new System.Net.NetworkCredential(email.Valeur, password.Valeur);
            client.Send(mail);

        }

        private string genererPassword()
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#@*";
            var Charsarr = new char[8];
            var random = new Random();

            for (int i = 0; i < Charsarr.Length; i++)
            {
                Charsarr[i] = characters[random.Next(characters.Length)];
            }

            return new String(Charsarr);
        }

         
    }
}