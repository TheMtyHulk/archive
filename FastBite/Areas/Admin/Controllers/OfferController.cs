using System.Threading.Tasks;
using System.Linq;
using FastBite.Data;
using FastBite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OfferController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OfferController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Offer.OrderBy(o => o.Id).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Offer offer)
        {
            if (!ModelState.IsValid)
            {
                return View(offer);
            }

            _db.Offer.Add(offer);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var offer = await _db.Offer.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Offer offer)
        {
            if (!ModelState.IsValid)
            {
                return View(offer);
            }

            _db.Offer.Update(offer);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var offer = await _db.Offer.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var offer = await _db.Offer.FindAsync(id);
            if (offer != null)
            {
                _db.Offer.Remove(offer);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}