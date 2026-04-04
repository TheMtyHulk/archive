using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FastBite.Data;
using FastBite.Models;
using FastBite.Models.ViewModel;
using FastBite.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartItemController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartItemController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string GetUserId()
        {
            return ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int id)
        {
            var menuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            var cartObj = new CartItem
            {
                MenuItemId = id,
                MenuItem = menuItem,
                Count = 1
            };

            return View(cartObj);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(CartItem cartItem)
        {
            var userId = GetUserId();
            var newItem = await _db.MenuItem.FindAsync(cartItem.MenuItemId);
            if (newItem == null)
            {
                return NotFound();
            }

            var userCart = await _db.CartItem.Where(c => c.ApplicationUserId == userId).OrderBy(c => c.Id).ToListAsync();
            if (userCart.Count > 0)
            {
                var firstMenuItem = await _db.MenuItem.FindAsync(userCart.First().MenuItemId);
                if (firstMenuItem != null && firstMenuItem.RestaurantId != newItem.RestaurantId)
                {
                    _db.CartItem.RemoveRange(userCart);
                    await _db.SaveChangesAsync();
                    userCart.Clear();
                }
            }

            var itemInCart = userCart.FirstOrDefault(c => c.MenuItemId == cartItem.MenuItemId);
            if (itemInCart == null)
            {
                _db.CartItem.Add(new CartItem
                {
                    ApplicationUserId = userId,
                    MenuItemId = cartItem.MenuItemId,
                    Count = cartItem.Count < 1 ? 1 : cartItem.Count
                });
            }
            else
            {
                itemInCart.Count += cartItem.Count < 1 ? 1 : cartItem.Count;
                _db.CartItem.Update(itemInCart);
            }

            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("cartCount", await _db.CartItem.CountAsync(c => c.ApplicationUserId == userId));

            var menuItems = await _db.MenuItem
                .Include(m => m.Category)
                .Where(m => m.RestaurantId == newItem.RestaurantId)
                .OrderBy(m => m.Id)
                .ToListAsync();

            var model = new CategoryAndMenuItemViewModel
            {
                MenuItem = menuItems,
                Category = menuItems.Select(m => m.Category).Where(c => c != null).Distinct().ToList(),
                Restaurant = await _db.Restaurant.FindAsync(newItem.RestaurantId)
            };

            return View("~/Areas/Customer/Views/Home/MenuItems.cshtml", model);
        }

    }
}