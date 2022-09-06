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
using SmartBreadcrumbs.Attributes;

namespace plateforme.Controllers
{
    public class ParametresController : Controller
    {
        private readonly plateformeContext _context;
        private readonly IWebHostEnvironment _env;

        public ParametresController(plateformeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _env = webHostEnvironment;
        }
        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }

        // GET: Parametres
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Index()
        {
            if(MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewData["Title"] = "Les paramètres de la plateforme";
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                return _context.Parametre != null ?
                              View(await _context.Parametre.ToListAsync()) :
                              Problem("Entity set 'plateformeContext.Parametre'  is null.");
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
            
        }





        // GET: Parametres/Edit/id
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> Edit(String? id)
        {
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewData["Title"] = "Modification de paramètre";
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (id == null || _context.Parametre == null)
                {
                    return NotFound();
                }

                var parametre = await _context.Parametre.FindAsync(id);
                if (parametre == null)
                {
                    return NotFound();
                }
                return View(parametre);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

            
        }

        // POST: Parametres/Edit/id
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> Edit(String id, [Bind("Id,Libelle,Valeur,Type")] Parametre parametre)
        {
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewData["Title"] = "Modification de paramètre";
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (id != parametre.Id)
                {
                    return NotFound();
                }
                if(parametre.Id == "mailPassword")
                {
                    if (Request.Form["ancien"] != parametre.Valeur)
                    {
                        ModelState.AddModelError("Valeur", "Mot de passe incorrect");
                    }
                    else if (Request.Form["nouveau"] != Request.Form["confirme"])
                    {
                        ModelState.AddModelError("Valeur", "Mot de passe incorrect");
                        ViewBag.erreur = "Ces deux mot de passe sont différents";
                    }
                    else
                    {
                        parametre.Valeur = Request.Form["nouveau"];
                    }
                }
                if (ModelState.IsValid)
                {
                    try
                    {

                        _context.Update(parametre);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ParametreExists(parametre.Id))
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
                return View(parametre);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }



            
        }



        // GET: Parametres/EditFile/5
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> EditFile(String? id)

        {
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewData["Title"] = "Modification de paramètre";
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (id == null || _context.Parametre == null)
                {
                    return NotFound();
                }

                var parametre = await _context.Parametre.FindAsync(id);
                if (parametre == null || parametre.Type != "fichier")
                {
                    return NotFound();
                }
                if (id == null || _context.Parametre == null)
                {
                    return NotFound();
                }
                return View(parametre);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }



            
        }

        // POST: Parametres/EditFile/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> EditFile(string id, [Bind("Id,Libelle,Valeur,Type")] Parametre parametre)
        {
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewData["Title"] = "Modification de paramètre";
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                IFormFile? file = Request.Form.Files["fichier"];
                if (file != null)
                {
                    if ( !Fichier.checkTaille(file))
                        ModelState.AddModelError("Valeur", "Ce fichier est trop lourd");
                    if (Fichier.checkType(file) != "photo")
                     ModelState.AddModelError("Valeur", "Ce fichier n'est pas une image");
                }
                else
                {
                    ModelState.AddModelError("Valeur", "Aucun changement effectuer");
                }

                if (id != parametre.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var chemin = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "parametre", file.FileName);

                    using (var stream = new FileStream(chemin, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    try
                    {
                        var oldChemin = Path.Combine(_env.ContentRootPath +"wwwroot"+ parametre.Valeur);
                        if(System.IO.File.Exists(oldChemin))
                        {
                            System.IO.File.Delete(oldChemin);
                        }
                        

                        if (parametre.Id == "PhotoProfilParDefaut")
                        {
                            var Liste = _context.Membre.Where(m => m.PhotoProfil == parametre.Valeur).ToList();

                            foreach (var mb in Liste)
                            {
                                mb.PhotoProfil = "/images/parametre/" + file.FileName;
                                _context.Membre.Update(mb);
                            }
                        }

                        parametre.Valeur = "/images/parametre/" + file.FileName;
                        _context.Update(parametre);
                        await _context.SaveChangesAsync();

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ParametreExists(parametre.Id))
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
                return View(parametre);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }


            
        }

        private bool ParametreExists(String id)
        {
          return (_context.Parametre?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}
