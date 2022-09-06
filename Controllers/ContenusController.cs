using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using plateforme.Models.ViewModels;
using System.Text.Json;

namespace plateforme.Controllers
{
    public class ContenusController : Controller
    {
        private readonly plateformeContext _context;
        private readonly IWebHostEnvironment _env;
        public ContenusController(plateformeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _env = webHostEnvironment;
        }
        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }
        

        //*************************************************************************************
        //*************************************************************************************
        //*
        //*#################### LES FONCTIONS LIES AUX CONTENUS ###############################
        //*
        //*************************************************************************************
        //*************************************************************************************
        public async Task<IActionResult> Signaler(int? Id)
        {
            ViewBag.membre = MembreConnecte();

            if (Id != null)
            {
                var pub = await _context.Contenu.FirstOrDefaultAsync(p => p.Id == Id);
                if (pub != null)
                {
                    
                    Signal sign = new Signal();
                    sign.MembreId = MembreConnecte().Id;
                    sign.ContenuId = pub.Id;
                    if (!_context.Signal.Any(s => s.ContenuId == sign.ContenuId && s.MembreId == sign.MembreId))
                    {
                        _context.Signal.Add(sign);
                        await _context.SaveChangesAsync();

                    }
                    
                    if (pub is Publication)
                    {
                        return RedirectToAction("Index", "Publications");
                    }
                    else if(pub is Sujet)
                    {
                        return RedirectToAction("Index", "Sujets");
                    }
                }
            }

            return NotFound();


        }



        public async Task<IActionResult> MesContenus()
        {
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                return _context.Contenu != null ?
                               View(await _context.Contenu.Include(c => c.Commentaires).Include(c => c.Ressources).Include(c => c.Signals).Where(p => p.MembreId == MembreConnecte().Id).ToListAsync()) :
                               Problem("Entity set 'plateformeContext.Contenu'  is null.");
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
        }

        public async Task<IActionResult> Supprime(int? id)
        {

            if (MembreConnecte().Etat == "actif")
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
                                foreach (var item in contenu.Ressources)
                                {
                                    if (item is Fichier)
                                    {
                                        Fichier f = (Fichier)item;
                                        var chemin = Path.Combine(_env.ContentRootPath + "wwwroot" + f.Chemin);


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
                                foreach (var com in contenu.Commentaires)
                                {
                                    if (com.Reponses.Count() > 0)
                                    {
                                        foreach (var rep in com.Reponses)
                                        {
                                            _context.Commentaire?.Remove(rep);
                                        }
                                    }
                                    _context.Commentaire?.Remove(com);
                                }
                            }
                            _context.Contenu.Remove(contenu);
                            await _context.SaveChangesAsync();

                        return RedirectToAction("MesContenus");

                        }
                    }
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
            
        }

        //GET Membres/ModifierContnenu
        public async Task<IActionResult> ModifierContenu(int? id)
        {
            
            if(id == null) return NotFound();  
            
                Contenu cont = _context.Contenu?.Include(c => c.Ressources).Include(c => c.Membre).Include(c => c.Commentaires).ThenInclude(c => c.Membre).FirstOrDefault(c => c.Id == id);
                if(cont == null) return NotFound();
            
            if (MembreConnecte().Etat == "actif" && cont.MembreId == MembreConnecte().Id)
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                ContenuModifVM contVM = new ContenuModifVM();
                contVM.Titre = cont.Titre;
                contVM.Description = cont.Description;
                contVM.Id = cont.Id;
                contVM.visiblite = ""+cont.Visibilite;
                foreach (var item in cont.Ressources)
                {
                    if (item is Lien)
                    {
                        Lien l = (Lien)item;
                        contVM.Lien = l.Url;
                    }
                    else if(item is Fichier)
                    {
                        Fichier f = (Fichier)item;
                        contVM.LienFichier = f.Chemin;
                    }
                }

                if(cont is Publication)
                {
                    Publication pub = (Publication)cont;
                    if(pub.Tags != null)
                    {
                        contVM.Tags = pub.Tags;
                    }

                    if(pub.DateDeb != null)
                        contVM.DateDeb = pub.DateDeb;

                    if (pub.DateFin != null)
                        contVM.DateFin = pub.DateFin;
                }
                
                return View(contVM) ;
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
        }


        //POST: Membres/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierContenu(ContenuModifVM contVM)
        {
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Departement).Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (contVM.DateDeb != null)
                {
                    
                    if (contVM.DateDeb < DateTime.Now)
                    {
                        ModelState.AddModelError("DateDeb", "L'évènement ne peut être programmé à une date antérieur");
                    }
                    if (contVM.DateDeb > contVM.DateFin)
                    {
                        ModelState.AddModelError("DateDeb", "La date de fin ne peut être antérieur à la date de début");
                    }
                }
                if (contVM.Tags != null)
                {
                    if (!contVM.Tags.Contains("#"))
                    {
                        ModelState.AddModelError("Tags", "Le format est incorrect");
                    }
                }
                if (contVM.Fichier != null)
                {
                    if (Fichier.checkType(contVM.Fichier) == "invalide")
                    {
                        ModelState.AddModelError("Fichier", "Ce type de fichier n'est pas pris en charge");
                    }
                    else if (!Fichier.checkTaille(contVM.Fichier))
                    {
                        ModelState.AddModelError("Fichier", "Ce fichier est trop lourd");
                    }
                }
                if (ModelState.IsValid)
                {
                    Contenu cont = _context.Contenu.Include(c => c.Ressources).FirstOrDefault(c => c.Id == contVM.Id);
                    if (cont == null) return NotFound();
                    cont.Titre = contVM.Titre;
                    cont.Description = contVM.Description;
                    cont.Visibilite = Convert.ToInt32(contVM.visiblite);

                    if(cont is Publication)
                    {
                        Publication pub = (Publication)cont;
                        if(contVM.Tags !=null)
                        {
                            pub.Tags = contVM.Tags;
                        }

                        if(contVM.DateDeb != null)
                        {
                            pub.DateDeb = contVM.DateDeb;
                        }

                        if(contVM.DateFin != null)
                        {
                            pub.DateFin = contVM.DateFin;
                        }
                        _context.Publication?.Update(pub);
                    }
                    else
                    {
                        _context.Contenu.Update(cont);
                    }

                    if(contVM.Lien != null || contVM.Fichier != null)
                    {
                        if(cont.Ressources.Count() > 0)
                        {
                            foreach (var item in cont.Ressources)
                            {
                                if (item is Lien && contVM.Lien != null)
                                {
                                    Lien lien = (Lien)item;
                                    lien.Url = contVM.Lien;
                                    lien.NomRes = contVM.Lien;
                                    _context.Lien?.Update(lien);
                                    await _context.SaveChangesAsync();
                                }
                                //Ajout d'un fichier s'il y en a
                                else if (item is Fichier && contVM.Fichier != null)
                                {
                                    Fichier f = new Fichier();
                                    var filename = MembreConnecte().Nom + MembreConnecte().Prenom + DateTime.Now.Millisecond + new FileInfo(contVM.Fichier.FileName).Extension;
                                    string chemin = "";
                                    if (System.IO.File.Exists(Path.Combine(_env.ContentRootPath + "wwwroot" + f.Chemin)))
                                    {
                                        System.IO.File.Delete(Path.Combine(_env.ContentRootPath + "wwwroot" + f.Chemin));
                                    }

                                    if (cont is Publication)
                                    {
                                        chemin = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "publication", filename);

                                        f.Chemin = "/images/publication/" + filename;
                                    }
                                    else if (cont is Sujet)
                                    {
                                        chemin = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "sujet", filename);
                                        f.Chemin = "/images/sujet/" + filename;
                                    }
                                    f.NomRes = "Publication de " + MembreConnecte().Username;
                                    f.Type = Fichier.checkType(contVM.Fichier);



                                    using (var stream = new FileStream(chemin, FileMode.Create))
                                    {
                                        contVM.Fichier.CopyTo(stream);
                                    }
                                    _context.Fichier?.Update(f);
                                    await _context.SaveChangesAsync();

                                }
                            }
                        }
                        else
                        {
                            //Ajout d'un lien s'il y en a
                            if (contVM.Lien != null)
                            {
                                Lien l = new Lien();
                                l.Url = contVM.Lien;
                                l.ContenuId = cont.Id;
                                l.NomRes = contVM.Lien;
                                _context.Lien.Add(l);
                                await _context.SaveChangesAsync();
                            }

                            //Ajout d'un fichier s'il y en a
                            if (contVM.Fichier != null)
                            {
                                Fichier f = new Fichier();
                                var filename = MembreConnecte().Nom + MembreConnecte().Prenom + DateTime.Now.Millisecond + new FileInfo(contVM.Fichier.FileName).Extension;
                                var chemin = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "publication", filename);
                                f.NomRes = "Publication de " + MembreConnecte().Username;
                                f.Chemin = "/images/publication/" + filename;
                                f.Type = Fichier.checkType(contVM.Fichier);
                                f.ContenuId = cont.Id;


                                using (var stream = new FileStream(chemin, FileMode.Create))
                                {
                                    contVM.Fichier.CopyTo(stream);
                                }
                                _context.Fichier?.Add(f);
                               
                            }
                        }
                        
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("MesContenus", "Membres");
                }
                return View(contVM);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
        }


        
    }
}
