using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using SmartBreadcrumbs.Attributes;
using System.Net.Mail;
using System.Text.Json;

namespace plateforme.Controllers
{
    public class ModerateursController : Controller
    {
       
        private readonly plateformeContext _context;
        private readonly IWebHostEnvironment _env;
        public ModerateursController(plateformeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _env = webHostEnvironment;
        }
        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }

        //GET : Moderateurs/Index
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Index()
        {
            if(MembreConnecte().Etat == "actif")
            {
                if(MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "moderateur")
                {
                    ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    var t = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    if (MembreConnecte().Profil == "superadmin")
                    {
                        return _context.Membre != null ?
                              View(await _context.Membre.Include(m => m.Contenus).ThenInclude(p => p.Signals).Include(p => p.Avertissements).Include(m => m.Departement).Include(m => m.Avertissements).
                              Where(m => m.Id != MembreConnecte().Id && (m.Contenus.Where(c => c.Signals.Count() > 0).Count() > 0  || m.Avertissements.Count() > 0)).
                              OrderBy(m => m.Nom).ToListAsync()) :
                              Problem("Entity set 'plateformeContext.Membre'  is null.");
                    }
                    else
                    {
                        return _context.Membre != null ?
                              View(await _context.Membre.Include(m => m.Contenus).ThenInclude(p => p.Signals).Include(m => m.Departement).Include(p => p.Avertissements).Include(m => m.Avertissements).
                              Where(m => m.Id != MembreConnecte().Id && (m.Contenus.Where(c => c.Signals.Count() > 0).Count() > 0 || m.Avertissements.Count() > 0) && m.DepartementId == MembreConnecte().DepartementId).
                              OrderBy(m => m.Nom).Where(m => m.DepartementId == MembreConnecte().DepartementId).ToListAsync()) :
                              Problem("Entity set 'plateformeContext.Membre'  is null.");
                    }
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

        //GET : Moderateurs/Avertrir/id
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> Avertir(int? id)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "moderateur")
                {
                    ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    Avertissement av = new Avertissement();
                    ViewBag.erreur = "";
                    if (id == null)
                        return NotFound();
                    else
                    {

                        av.MembreId = (int)id;
                        av.ModerateurId = (int)ViewBag.membre.Id;
                    }


                    return View(av);
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

        //POST : Moderateurs/Avertrir
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> Avertir([Bind( "MembreId","ModerateurId","Note")] Avertissement avertissement)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "moderateur")
                {
                    ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    ViewBag.erreur = "";
                    if (avertissement.Note == null)
                    {
                        ViewBag.Erreur = "Un avertissemennt ne peut être envoyé sans note d'avertissement";
                        return View(avertissement);
                    }
                    else
                    {
                        string objet = "Avertissement !!!";

                        if (Request.Form["objet"] != "")
                        {
                            objet = Request.Form["objet"];
                        }
                        var membre = _context.Membre.Find(avertissement.MembreId);
                        try
                        {
                            
                            EnvoiMail(membre.Mail, avertissement.Note, objet);
                            _context.Avertissement?.Add(avertissement);
                            await _context.SaveChangesAsync();


                            Notification notif = new Notification();
                            notif.Description = "L'avertissement à bien été envoyé à " + membre.Nom +
                                " " + membre.Prenom;

                            _context.Notification?.Add(notif);
                            await _context.SaveChangesAsync();

                            notif = _context.Notification.OrderBy(n => n.Id).Last();

                            DetailNotif details = new DetailNotif();
                            details.MembreId = MembreConnecte().Id;
                            details.NoificationId = notif.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        catch
                        {
                            Notification notif = new Notification();
                            notif.Description = "L'avertissement n'a pas pu être envoyer vérifier votre connexion internet puis réessayer";
                            ViewBag.erreur = "L'avertissement n'a pas pu être envoyer vérifier votre connexion internet puis réessayer";
                            _context.Notification?.Add(notif);
                            await _context.SaveChangesAsync();

                            notif = _context.Notification.OrderBy(n => n.Id).Last();

                            DetailNotif details = new DetailNotif();
                            details.MembreId = MembreConnecte().Id;
                            details.NoificationId = notif.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();
                            return View(avertissement);
                        }




                    }
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


        //GET : Moderateurs/Details/id
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> Details(int? id)
        {
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "moderateur")
                {
                    ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                    if (id == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        var pub = await _context.Contenu?.Include(p => p.Signals).Include(m => m.Commentaires).Include(p => p.Membre).Where(p => p.Signals.Count() > 0 && p.MembreId == id).ToListAsync();
                        return View(pub);
                    }
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




        public async Task<IActionResult> Retirer(int? id)
        {
           
            if (MembreConnecte().Etat == "actif")
            {
                if (MembreConnecte().Profil == "superadmin" || MembreConnecte().Profil == "moderateur")
                {
                    if (id == null) return NotFound();
                    else
                    {
                        Contenu contenu = _context.Contenu.Include(c => c.Ressources).Include(c => c.Commentaires).FirstOrDefault(c => c.Id == id);
                        if (contenu == null) return NotFound();
                        else
                        {
                           //Vérifier si le contenu est relié à un fichier afin de supprimer ce dernier
                            if (contenu.Ressources.Count() > 0)
                            {
                                foreach(var item in contenu.Ressources)
                                {
                                    if(item is Fichier)
                                    {
                                        Fichier f = (Fichier)item;
                                        var chemin = Path.Combine(_env.ContentRootPath+ "wwwroot"+ f.Chemin);
                                        

                                        if (System.IO.File.Exists(chemin))
                                        {
                                            System.IO.File.Delete(chemin);
                                        }
                                    }
                                }
                            }
                            //Vérifier si le contenu a des commentaires et supprimer ces derniers en commencant par les réponses aux commentaires
                            if (contenu.Commentaires.Count() > 0)
                            {
                                foreach(var com in contenu.Commentaires)
                                {
                                    if(com.Reponses.Count() > 0)
                                    {
                                        foreach(var rep in com.Reponses)
                                        {
                                            _context.Commentaire?.Remove(rep);
                                        }
                                    }
                                    _context.Commentaire?.Remove(com);
                                }
                            }
                            _context.Contenu.Remove(contenu);
                            await _context.SaveChangesAsync();

                            Notification notif = new Notification();
                            notif.Description = "Votre contenu du titre " + contenu.Titre+ " a été retiré par le modérateur suite son possible caractère non conforme";
                           
                            _context.Notification?.Add(notif);
                            await _context.SaveChangesAsync();


                            notif = _context.Notification.OrderBy(n => n.Id).Last();

                            DetailNotif details = new DetailNotif();
                            details.MembreId = contenu.MembreId;
                            details.NoificationId = notif.Id;
                            _context.DetailNotif?.Add(details);
                            await _context.SaveChangesAsync();

                            if (contenu is Publication)
                                return RedirectToAction("Index", "Publications");
                            else
                                return RedirectToAction("Index", "Sujets");

                        }
                    }
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





        public void EnvoiMail(string adr, string body, string subject)
        {


            MailMessage mail = new MailMessage();

            mail.Priority = MailPriority.High;
            mail.IsBodyHtml = true;
            mail.Body = body;
            mail.Subject = subject;
            Parametre email = _context.Parametre.Where(p => p.Id == "mailAdresse").First();
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

    }
}
