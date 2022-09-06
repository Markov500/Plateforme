using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plateforme.Data;
using plateforme.Models;
using plateforme.Models.ViewModels;
using System.Text.Json;

namespace plateforme.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly plateformeContext _context;
        private readonly IWebHostEnvironment _env;
        public NotificationsController(plateformeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _env = webHostEnvironment;
        }
        public Membre MembreConnecte()
        {
            var value = HttpContext.Session.GetString("user");
            return value == null ? new Membre() : JsonSerializer.Deserialize<Membre>(value);
        }
        

        
        public async Task<IActionResult> ToutSupprimer()
        {
                var membre = await _context.Membre.Include(m => m.Notifications).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
            if (membre == null) return NotFound();
                
                foreach (var i in membre.Notifications)
                {
                    _context.DetailNotif?.Remove(i);
                }
                await _context.SaveChangesAsync();
            
            return RedirectToAction("Notifications", "Membres", new { id = MembreConnecte().Id });

        }



        public async Task<IActionResult> ToutLu()
        {
            
                var membre = await _context.Membre.Include(m => m.Notifications).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);

                foreach (var i in membre.Notifications)
                {
                    i.Lu = true;
                    _context.DetailNotif?.Update(i);
                }
                
                await _context.SaveChangesAsync();
            
            return RedirectToAction("Notifications", "Membres", new { id = MembreConnecte().Id });

        }


        public async Task<IActionResult> SupprimerNotif(int? id)
        {
            if (id != null)
            {
                DetailNotif notif = await _context.DetailNotif.Include(m => m.Notification).FirstOrDefaultAsync(m => m.Id == id);

                if (notif == null) return NotFound();
               _context.DetailNotif.Remove(notif);
               
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Notifications", "Membres", new { id = id });

        }

        public async Task<IActionResult> Lu(int? id)
        {
            if (id != null)
            {
                DetailNotif notif = await _context.DetailNotif.Include(m => m.Notification).FirstOrDefaultAsync(m => m.Id == id);

                if (notif == null) return NotFound();
                notif.Lu = true;
                _context.DetailNotif.Update(notif);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Notifications", "Membres", new { id = id });

        }


        public async Task<IActionResult> Notifications()
        {
            if (MembreConnecte().Etat == "actif")
            {
                ViewBag.membre = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                Membre m = await _context.Membre.Include(m => m.Notifications).ThenInclude(n => n.Notification).FirstOrDefaultAsync(m => m.Id == MembreConnecte().Id);
                return View(m.Notifications.OrderByDescending(n => n.DateEnvoi) );
            }
            else
            {
                return RedirectToAction("Login", "Membres");
            }


        }
    }
}
