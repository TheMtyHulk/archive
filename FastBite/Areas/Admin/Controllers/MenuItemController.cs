using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastBite.Data;
using FastBite.Models;
using FastBite.Models.ViewModel;
using FastBite.Models.Viewmodels;
using FastBite.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FastBite.Areas.Admin.Controllers
{
   
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MenuItemController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int restid)
        {
            ViewData["restid"] = restid;
            ViewData["categories"] = await _db.Category.OrderBy(c => c.Id).ToListAsync();
            ViewData["subCategories"] = await _db.SubCategory.OrderBy(s => s.Id).ToListAsync();

            var biryani = await _db.Category.FirstOrDefaultAsync(c => c.Name == "Biryani");
            var ap = await _db.Category.FirstOrDefaultAsync(c => c.Name == "Apetizer1" || c.Name == "Apetizer");
            var veg = await _db.SubCategory.FirstOrDefaultAsync(s => s.Name == "Veg");
            var bev = await _db.SubCategory.FirstOrDefaultAsync(s => s.Name == "Beverages");

            if (restid == 2 && biryani != null && veg != null && !await _db.MenuItem.AnyAsync(m => m.RestaurantId == 2 && m.Name == "Dumbiryani"))
            {
                _db.MenuItem.Add(new MenuItem { Name = "Dumbiryani", description = "RiceItem", price = 190, imageUrl = StaticDefinitions.defaultimage, CategoryId = biryani.Id, SubCategoryId = veg.Id, RestaurantId = 2 });
            }
            if (restid == 2 && ap != null && bev != null && !await _db.MenuItem.AnyAsync(m => m.RestaurantId == 2 && m.Name == "Lemonade"))
            {
                _db.MenuItem.Add(new MenuItem { Name = "Lemonade", description = "MadebyLemon", price = 110, imageUrl = StaticDefinitions.defaultimage, CategoryId = ap.Id, SubCategoryId = bev.Id, RestaurantId = 2 });
            }
            if (restid == 1 && biryani != null && veg != null && !await _db.MenuItem.AnyAsync(m => m.RestaurantId == 1 && m.Name == "vegbiryani"))
            {
                _db.MenuItem.Add(new MenuItem { Name = "vegbiryani", description = "RiceItem", price = 130, imageUrl = StaticDefinitions.defaultimage, CategoryId = biryani.Id, SubCategoryId = veg.Id, RestaurantId = 1 });
            }

            await _db.SaveChangesAsync();

            var menuItems = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Where(m => m.RestaurantId == restid)
                .OrderBy(m => m.Id)
                .ToListAsync();
            return View(menuItems);
        }

        public IActionResult Create(int restid)
        {
            var model = new MenuItemViewModel
            {
                MenuItem = new MenuItem { RestaurantId = restid },
                Category = _db.Category.OrderBy(c => c.Id).ToList(),
                SubCategory = _db.SubCategory.OrderBy(s => s.Id).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MenuItemViewModel model, int CategoryId, int SubCategoryId)
        {
            if (model == null)
            {
                model = new MenuItemViewModel();
            }

            if (model.MenuItem == null)
            {
                model.MenuItem = new MenuItem();
            }

            if (string.IsNullOrWhiteSpace(model.MenuItem.Name))
            {
                model.MenuItem.Name = Request.Form["MenuItem.Name"].ToString();
            }

            if (string.IsNullOrWhiteSpace(model.MenuItem.description))
            {
                model.MenuItem.description = Request.Form["MenuItem.description"].ToString();
            }

            if (model.MenuItem.price <= 0 && double.TryParse(Request.Form["MenuItem.price"], out var formPrice))
            {
                model.MenuItem.price = formPrice;
            }

            if (model.MenuItem.RestaurantId <= 0 && int.TryParse(Request.Form["MenuItem.RestaurantId"], out var formRestId))
            {
                model.MenuItem.RestaurantId = formRestId;
            }

            model.MenuItem.CategoryId = CategoryId == 0 ? model.MenuItem.CategoryId : CategoryId;
            model.MenuItem.SubCategoryId = SubCategoryId == 0 ? model.MenuItem.SubCategoryId : SubCategoryId;

            if (!ModelState.IsValid)
            {
                model.Category = _db.Category.OrderBy(c => c.Id).ToList();
                model.SubCategory = _db.SubCategory.OrderBy(s => s.Id).ToList();
                return View(model);
            }

            model.MenuItem.imageUrl = StaticDefinitions.defaultimage;
            _db.MenuItem.Add(model.MenuItem);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { restid = model.MenuItem.RestaurantId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var menuItem = await _db.MenuItem.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            var model = new MenuItemViewModel
            {
                MenuItem = menuItem,
                Category = _db.Category.OrderBy(c => c.Id).ToList(),
                SubCategory = _db.SubCategory.OrderBy(s => s.Id).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MenuItemViewModel model, int CategoryId, int SubCategoryId)
        {
            model.MenuItem.CategoryId = CategoryId == 0 ? model.MenuItem.CategoryId : CategoryId;
            model.MenuItem.SubCategoryId = SubCategoryId == 0 ? model.MenuItem.SubCategoryId : SubCategoryId;

            if (!ModelState.IsValid)
            {
                model.Category = _db.Category.OrderBy(c => c.Id).ToList();
                model.SubCategory = _db.SubCategory.OrderBy(s => s.Id).ToList();
                return View(model);
            }

            _db.MenuItem.Update(model.MenuItem);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { restid = model.MenuItem.RestaurantId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var menuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var menuItem = await _db.MenuItem.FindAsync(id);
            var restId = menuItem?.RestaurantId ?? 0;
            if (menuItem != null)
            {
                _db.MenuItem.Remove(menuItem);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { restid = restId });
        }
    }
    }
    
