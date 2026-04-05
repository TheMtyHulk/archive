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
            var restaurants = await _db.Restaurant.OrderBy(r => r.Id).ToListAsync();
            return View(restaurants);
        }

        public async Task<IActionResult> MenuItems(int id)
        {
            var menuItems = await _db.MenuItem
                .Include(m => m.Category)
                .Where(m => m.RestaurantId == id)
                .OrderBy(m => m.CategoryId)
                .ThenBy(m => m.Id)
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
