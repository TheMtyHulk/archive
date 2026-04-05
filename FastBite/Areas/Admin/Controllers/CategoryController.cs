

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastBite.Data;
using FastBite.Models;
using Microsoft.AspNetCore.Authorization;
using FastBite.Utility;

namespace FastBite.Areas.Admin.Controllers
{
  
    [Area("Admin")]
     public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var cats = await _db.Category.OrderBy(c => c.Id).ToListAsync();
            System.Console.WriteLine($"[CAT-INDEX] Category count: {cats.Count}");
            return View(cats);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (category == null)
            {
                category = new Category();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                category.Name = Request.Form["Name"].ToString();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return View(category);
            }

            System.Console.WriteLine($"[CAT-CREATE] Adding category: {category.Name}");
            _db.Category.Add(category);
            await _db.SaveChangesAsync();
            System.Console.WriteLine($"[CAT-CREATE] Saved with Id: {category.Id}");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (category == null)
            {
                category = new Category();
            }

            if (category.Id == 0 && int.TryParse(Request.Form["Id"], out var formId))
            {
                category.Id = formId;
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                category.Name = Request.Form["Name"].ToString();
            }

            if (category.Id == 0)
            {
                return View(category);
            }

            var entity = await _db.Category.FindAsync(category.Id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = category.Name;
            _db.Category.Update(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var category = await _db.Category.FindAsync(id);
            if (category != null)
            {
                _db.Category.Remove(category);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}