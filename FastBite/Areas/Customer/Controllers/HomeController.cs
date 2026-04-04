using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FastBite.Models;
using FastBite.Data;
using FastBite.Utility;
using Microsoft.EntityFrameworkCore;
using FastBite.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FastBite.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _db.Restaurant.AnyAsync(r => r.RestaurantName == "Restaurant1"))
            {
                _db.Restaurant.Add(new Restaurant { RestaurantName = "Restaurant1", Address = "Wallstreet", imageurl = StaticDefinitions.defaultimage });
            }
            if (!await _db.Restaurant.AnyAsync(r => r.RestaurantName == "Restaurant2"))
            {
                _db.Restaurant.Add(new Restaurant { RestaurantName = "Restaurant2", Address = "Wallstreet", imageurl = StaticDefinitions.defaultimage });
            }
            await _db.SaveChangesAsync();

            var restaurants = await _db.Restaurant.OrderBy(r => r.Id).ToListAsync();
            return View(restaurants);
        }

        public async Task<IActionResult> MenuItems(int id)
        {
            var biryani = await _db.Category.FirstOrDefaultAsync(c => c.Name == "Biryani");
            var ap = await _db.Category.FirstOrDefaultAsync(c => c.Name == "Apetizer1" || c.Name == "Apetizer");
            if (ap != null && ap.Name == "Apetizer")
            {
                ap.Name = "Apetizer1";
                _db.Category.Update(ap);
                await _db.SaveChangesAsync();
            }

            var veg = await _db.SubCategory.FirstOrDefaultAsync(s => s.Name == "Veg");
            var bev = await _db.SubCategory.FirstOrDefaultAsync(s => s.Name == "Beverages");

            if (id == 1 && biryani != null && veg != null && !await _db.MenuItem.AnyAsync(m => m.RestaurantId == 1 && m.Name == "vegbiryani"))
            {
                _db.MenuItem.Add(new MenuItem { Name = "vegbiryani", description = "RiceItem", price = 130, CategoryId = biryani.Id, SubCategoryId = veg.Id, RestaurantId = 1, imageUrl = StaticDefinitions.defaultimage });
            }
            if (id == 2 && biryani != null && veg != null && !await _db.MenuItem.AnyAsync(m => m.RestaurantId == 2 && m.Name == "Dumbiryani"))
            {
                _db.MenuItem.Add(new MenuItem { Name = "Dumbiryani", description = "RiceItem", price = 190, CategoryId = biryani.Id, SubCategoryId = veg.Id, RestaurantId = 2, imageUrl = StaticDefinitions.defaultimage });
            }
            if (id == 2 && ap != null && bev != null && !await _db.MenuItem.AnyAsync(m => m.RestaurantId == 2 && m.Name == "Lemonade"))
            {
                _db.MenuItem.Add(new MenuItem { Name = "Lemonade", description = "MadebyLemon", price = 110, CategoryId = ap.Id, SubCategoryId = bev.Id, RestaurantId = 2, imageUrl = StaticDefinitions.defaultimage });
            }

            await _db.SaveChangesAsync();

            var menuItems = await _db.MenuItem
                .Include(m => m.Category)
                .Where(m => m.RestaurantId == id)
                .OrderBy(m => m.Id)
                .ToListAsync();

            var model = new CategoryAndMenuItemViewModel
            {
                MenuItem = menuItems,
                Category = menuItems.Select(m => m.Category).Where(c => c != null).Distinct().ToList(),
                Restaurant = await _db.Restaurant.FindAsync(id)
            };

            return View(model);
        }
    }
}
