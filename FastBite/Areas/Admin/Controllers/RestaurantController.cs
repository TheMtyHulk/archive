using System.Linq;
using System.Threading.Tasks;
using FastBite.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FastBite.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using FastBite.Utility;
using Microsoft.AspNetCore.Authorization;

namespace FastBite.Areas.Admin.Controllers
{
     [Area("Admin")]
    public class RestaurantController : Controller
    {
        private readonly ApplicationDbContext _db;

        public RestaurantController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string GetUserId()
        {
            return ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IActionResult> Index()
        {
            var restaurants = await _db.Restaurant
                .OrderBy(r => r.Id)
                .ToListAsync();

            return View(restaurants);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Restaurant restaurant)
        {
            if (restaurant == null)
            {
                restaurant = new Restaurant();
            }

            restaurant.RestaurantName = string.IsNullOrWhiteSpace(restaurant.RestaurantName)
                ? Request.Form["RestaurantName"].ToString()
                : restaurant.RestaurantName;
            restaurant.Address = string.IsNullOrWhiteSpace(restaurant.Address)
                ? Request.Form["Address"].ToString()
                : restaurant.Address;

            if (string.IsNullOrWhiteSpace(restaurant.RestaurantName) || string.IsNullOrWhiteSpace(restaurant.Address))
            {
                return View(restaurant);
            }

            restaurant.OwenerID = GetUserId();
            restaurant.imageurl = StaticDefinitions.defaultimage;
            _db.Restaurant.Add(restaurant);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var restaurant = await _db.Restaurant.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Restaurant restaurant)
        {
            if (restaurant == null || restaurant.Id == 0)
            {
                return View(restaurant);
            }

            var entity = await _db.Restaurant.FindAsync(restaurant.Id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.RestaurantName = restaurant.RestaurantName;
            entity.Address = restaurant.Address;
            entity.OwenerID = GetUserId();
            _db.Restaurant.Update(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
   
}