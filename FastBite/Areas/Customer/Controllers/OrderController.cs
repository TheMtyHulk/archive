using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FastBite.Data;
using FastBite.Models;
using FastBite.Models.ViewModel;
using FastBite.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string GetUserId()
        {
            return ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var order = await _db.Cart.FirstOrDefaultAsync(c => c.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var details = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync();
            var model = new OrderViewModel
            {
                cart = order,
                OrderDetailsList = details
            };

            return View(model);
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();
            var orders = await _db.Cart
                .Where(c => c.userId == userId && c.orderStatus != null)
                .OrderBy(c => c.Id)
                .ToListAsync();

            var model = new List<OrderViewModel>();
            foreach (var order in orders)
            {
                model.Add(new OrderViewModel
                {
                    cart = order,
                    OrderDetailsList = await _db.OrderDetails.Where(o => o.OrderId == order.Id).ToListAsync()
                });
            }

            return View(model);
        }

        public async Task<IActionResult> PendingRestaurantOrderDetails(int restid)
        {
            var orderIds = await _db.OrderDetails
                .Include(o => o.MenuItem)
                .Where(o => o.MenuItem.RestaurantId == restid)
                .Select(o => o.OrderId)
                .Distinct()
                .ToListAsync();

            var orderHeaders = await _db.Cart
                .Where(c => orderIds.Contains(c.Id)
                            && c.orderStatus != StaticDefinitions.OrderCancelled
                            && c.orderStatus != StaticDefinitions.OrderDelivered)
                .OrderBy(c => c.Id)
                .ToListAsync();

            var model = new List<OrderViewModel>();
            foreach (var header in orderHeaders)
            {
                model.Add(new OrderViewModel
                {
                    cart = header,
                    Restaurant = await _db.Restaurant.FindAsync(restid),
                    OrderDetailsList = await _db.OrderDetails
                        .Include(o => o.MenuItem)
                        .Where(o => o.OrderId == header.Id && o.MenuItem.RestaurantId == restid)
                        .ToListAsync()
                });
            }

            return View(model);
        }

        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _db.Cart.FindAsync(id);
            if (order != null)
            {
                order.orderStatus = StaticDefinitions.OrderConfirmed;
                _db.Cart.Update(order);
                await _db.SaveChangesAsync();
            }

            var restId = await _db.OrderDetails
                .Include(o => o.MenuItem)
                .Where(o => o.OrderId == id)
                .Select(o => o.MenuItem.RestaurantId)
                .FirstOrDefaultAsync();

            return RedirectToAction(nameof(PendingRestaurantOrderDetails), new { restid = restId });
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _db.Cart.FindAsync(id);
            if (order != null)
            {
                order.orderStatus = StaticDefinitions.OrderCancelled;
                _db.Cart.Update(order);
                await _db.SaveChangesAsync();
            }

            var restId = await _db.OrderDetails
                .Include(o => o.MenuItem)
                .Where(o => o.OrderId == id)
                .Select(o => o.MenuItem.RestaurantId)
                .FirstOrDefaultAsync();

            return RedirectToAction(nameof(PendingRestaurantOrderDetails), new { restid = restId });
        }

        public async Task<IActionResult> Ready(int id)
        {
            var order = await _db.Cart.FindAsync(id);
            if (order != null)
            {
                order.orderStatus = StaticDefinitions.orderReady;
                _db.Cart.Update(order);
                await _db.SaveChangesAsync();
            }

            var restId = await _db.OrderDetails
                .Include(o => o.MenuItem)
                .Where(o => o.OrderId == id)
                .Select(o => o.MenuItem.RestaurantId)
                .FirstOrDefaultAsync();

            return RedirectToAction(nameof(PendingRestaurantOrderDetails), new { restid = restId });
        }
    }
}