using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using plateforme.Controllers;
using System.Text.Json;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

namespace plateforme.Controllers
{
    public class DepartementsController : Controller
    {
        private readonly plateformeContext _context;

        public DepartementsController(plateformeContext context)
        {
            _context = context;
        }

        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null  ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }

        // GET: Departements
        [Breadcrumb(Title = "ViewData.Title")]

        public async Task<IActionResult> Index()
        {
            if(MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                ViewData["Title"] = "Liste des départements";
                ViewBag.icone = _context.Parametre.Where(p => p.Id == "iconeDepartement").First().Valeur;
                return _context.Departement != null ?
                              View(await _context.Departement.Include(d => d.Membres).ToListAsync()) :
                              Problem("Entity set 'plateformeContext.Departement'  is null.");
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
            
        }

        // GET: Departements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Departement == null)
            {
                return NotFound();
            }

            var departement = await _context.Departement
                .FirstOrDefaultAsync(m => m.Id == id);
            if (departement == null)
            {
                return NotFound();
            }

            return View(departement);
        }

        // GET: Departements/Create
        [Breadcrumb("ViewData.Title")]
        public async Task<IActionResult> Create()
        {
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewData["Title"] = "Ajouter un département";
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }
            

        }

        // POST: Departements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Breadcrumb("ViewData.Title", FromAction = "Index")]
        public async Task<IActionResult> Create([Bind("codeDept,NomDept")] Departement departement)
        {
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewData["Title"] = "Ajouter un département";
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (_context.Departement.Any(d => d.NomDept == departement.NomDept))
                {
                    ModelState.AddModelError("NomDept", "Ce département existe déjà");
                }
                if(departement.NomDept == null)
                {
                    ModelState.AddModelError("NomDept", "Vous devez renseigner un nom pour le département");
                }
                if (ModelState.IsValid)
                {
                    _context.Add(departement);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(departement);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }


            
        }

        // GET: Departements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                if (id == null || _context.Departement == null)
                {
                    return NotFound();
                }

                var departement = await _context.Departement.FindAsync(id);
                if (departement == null)
                {
                    return NotFound();
                }
                return View(departement);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

            
        }

        // POST: Departements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("codeDept,NomDept")] Departement departement)
        {
            ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
            if (MembreConnecte().Etat == "actif" && MembreConnecte().Profil == "superadmin")
            {
                if (id != departement.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(departement);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!DepartementExists(departement.Id))
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
                return View(departement);
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }

            
        }

        // GET: Departements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["Title"] = "Delete";
            if (id == null || _context.Departement == null)
            {
                return NotFound();
            }

            var departement = await _context.Departement
                .FirstOrDefaultAsync(m => m.Id == id);
            if (departement == null)
            {
                return NotFound();
            }

            return View(departement);
        }

        // POST: Departements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewData["Title"] = "Delete";
            if (_context.Departement == null)
            {
                return Problem("Entity set 'plateformeContext.Departement'  is null.");
            }
            var departement = await _context.Departement.FindAsync(id);
            if (departement != null)
            {
                _context.Departement.Remove(departement);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartementExists(int id)
        {
          return (_context.Departement?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
