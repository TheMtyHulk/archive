

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
            if (!await _db.Category.AnyAsync(c => c.Name == "Apetizer" || c.Name == "Apetizer1"))
            {
                _db.Category.Add(new Category { Name = "Apetizer" });
            }

            if (!await _db.Category.AnyAsync(c => c.Name == "Biryani"))
            {
                _db.Category.Add(new Category { Name = "Biryani" });
            }

            await _db.SaveChangesAsync();

            var apetizerRows = await _db.Category.Where(c => c.Name == "Apetizer" && _db.Category.Any(x => x.Name == "Apetizer1")).ToListAsync();
            if (apetizerRows.Any())
            {
                foreach (var category in apetizerRows)
                {
                    category.Name = "Apetizer1";
                }

                _db.Category.UpdateRange(apetizerRows);
                await _db.SaveChangesAsync();
            }

            return View(await _db.Category.OrderBy(c => c.Id).ToListAsync());
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

            var exists = await _db.Category.AnyAsync(c => c.Name == category.Name);
            if (!exists)
            {
                _db.Category.Add(category);
                await _db.SaveChangesAsync();
            }

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