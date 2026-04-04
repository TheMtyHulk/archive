using System.Collections.Generic;
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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string GetUserId()
        {
            return ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private async Task<CartViewModel> BuildCartViewModel(string userId)
        {
            var cartItems = await _db.CartItem.Where(c => c.ApplicationUserId == userId).OrderBy(c => c.Id).ToListAsync();
            foreach (var item in cartItems)
            {
                item.MenuItem = await _db.MenuItem.Include(m => m.Restaurant).FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
            }

            var applicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(u => u.Id == userId);
            var amountWithoutDiscount = cartItems.Sum(i => i.Count * (i.MenuItem?.price ?? 0));

            var cart = await _db.Cart.FirstOrDefaultAsync(c => c.userId == userId && c.orderStatus == null);
            if (cart == null)
            {
                cart = new Cart
                {
                    userId = userId,
                    name = applicationUser?.Name,
                    email = applicationUser?.Email,
                    mobilenumber = applicationUser?.PhoneNumber,
                    Address = applicationUser?.Address,
                    offercode = null
                };
            }

            cart.AmountWithoutDiscount = amountWithoutDiscount;
            var offer = await _db.Offer.FirstOrDefaultAsync(o => o.Name == cart.offercode && o.isActive);
            cart.OrderTotal = StaticDefinitions.DiscountPrice(offer, amountWithoutDiscount);
            cart.discount = amountWithoutDiscount - cart.OrderTotal;

            return new CartViewModel
            {
                Cart = cart,
                CartItemList = cartItems
            };
        }

        [Authorize]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = GetUserId();
            // Initialize session cart count if not already set
            if (HttpContext.Session.GetInt32("cartCount") == null)
            {
                int count = await _db.CartItem.CountAsync(c => c.ApplicationUserId == userId);
                HttpContext.Session.SetInt32("cartCount", count);
            }
            var model = await BuildCartViewModel(userId);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Add(int id)
        {
            var cartItem = await _db.CartItem.FindAsync(id);
            if (cartItem != null)
            {
                cartItem.Count += 1;
                _db.CartItem.Update(cartItem);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(PlaceOrder));
        }

        [Authorize]
        public async Task<IActionResult> minus(int id)
        {
            var cartItem = await _db.CartItem.FindAsync(id);
            if (cartItem != null)
            {
                if (cartItem.Count <= 1)
                {
                    _db.CartItem.Remove(cartItem);
                }
                else
                {
                    cartItem.Count -= 1;
                    _db.CartItem.Update(cartItem);
                }
                await _db.SaveChangesAsync();
            }

            HttpContext.Session.SetInt32("cartCount", await _db.CartItem.CountAsync(c => c.ApplicationUserId == GetUserId()));
            return RedirectToAction(nameof(PlaceOrder));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Apply(string offercode)
        {
            var userId = GetUserId();
            var cart = await _db.Cart.FirstOrDefaultAsync(c => c.userId == userId && c.orderStatus == null);
            if (cart == null)
            {
                cart = new Cart { userId = userId };
                _db.Cart.Add(cart);
            }

            cart.offercode = offercode;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(PlaceOrder));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Remove()
        {
            var cart = await _db.Cart.FirstOrDefaultAsync(c => c.userId == GetUserId() && c.orderStatus == null);
            if (cart != null)
            {
                cart.offercode = null;
                _db.Cart.Update(cart);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(PlaceOrder));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(CartViewModel viewModel)
        {
            var userId = GetUserId();
            var model = await BuildCartViewModel(userId);
            if (!model.CartItemList.Any())
            {
                return RedirectToAction(nameof(PlaceOrder));
            }

            var orderHeader = await _db.Cart.FirstOrDefaultAsync(c => c.userId == userId && c.orderStatus == null);
            if (orderHeader == null)
            {
                orderHeader = new Cart { userId = userId };
                _db.Cart.Add(orderHeader);
            }

            orderHeader.AmountWithoutDiscount = model.Cart.AmountWithoutDiscount;
            orderHeader.OrderTotal = model.Cart.OrderTotal;
            orderHeader.discount = model.Cart.discount;
            orderHeader.offercode = model.Cart.offercode;
            orderHeader.orderStatus = StaticDefinitions.PendingConfirmation;
            orderHeader.Address = model.Cart.Address;
            orderHeader.email = model.Cart.email;
            orderHeader.name = model.Cart.name;
            orderHeader.mobilenumber = model.Cart.mobilenumber;

            await _db.SaveChangesAsync();

            foreach (var item in model.CartItemList)
            {
                _db.OrderDetails.Add(new OrderDetails
                {
                    OrderId = orderHeader.Id,
                    MenuItemId = item.MenuItemId,
                    Count = item.Count,
                    Name = item.MenuItem?.Name,
                    Description = item.MenuItem?.description
                });
            }

            _db.CartItem.RemoveRange(model.CartItemList);
            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("cartCount", 0);

            return RedirectToAction("ConfirmOrder", "Order", new { area = "Customer", id = orderHeader.Id });
        }
    }
}