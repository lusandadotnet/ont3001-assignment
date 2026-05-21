using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Models; 
namespace StoreManagementSystem.Controllers
{
    // The Controller handles all user requests related to Orders and coordinates the Views.
    public class OrdersController : Controller
    {
        // Private field for the Entity Framework database context
        private readonly StoreManagementDbContext _context;

        // Constructor: Dependency Injection provides the database context
        public OrdersController(StoreManagementDbContext context)
        {
            _context = context;
        }

        // --- READ: Get all orders ---
        // GET: Orders
        // This method fetches all orders and their related customer information.
        public async Task<IActionResult> Index()
        {
            // LINQ: Eagerly load the related Customer data for each order so we can show customer names
            var storeManagementDbContext = _context.Orders.Include(o => o.Customer);
            return View(await storeManagementDbContext.ToListAsync());
        }

        // --- READ: Get a single order ---
        // GET: Orders/Details/5
        // This method fetches a specific order by ID to show its details.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // LINQ: Find the order by ID and include the related Customer data
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // --- CREATE: Show the form ---
        // GET: Orders/Create
        // This method displays the empty form to create a new order.
        public IActionResult Create()
        {
            // Pass a list of Customers to the view for a dropdown selection menu
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email");
            return View();
        }

        // --- CREATE: Save the form data ---
        // POST: Orders/Create
        // This method receives the form data and saves the new order to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderDate,TotalAmount,OrderStatus,CustomerId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // If validation fails, reload the customer dropdown
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email", order.CustomerId);
            return View(order);
        }

        // --- UPDATE: Show the form ---
        // GET: Orders/Edit/5
        // This method fetches a specific order and populates the edit form.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            // Populate the customer dropdown, pre-selecting the current customer
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email", order.CustomerId);
            return View(order);
        }

        // --- UPDATE: Save the form data ---
        // POST: Orders/Edit/5
        // This method saves the edited order data to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderDate,TotalAmount,OrderStatus,CustomerId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email", order.CustomerId);
            return View(order);
        }

        // --- DELETE: Show confirmation screen ---
        // GET: Orders/Delete/5
        // This method displays a screen asking the user to confirm deleting the order.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // LINQ: Fetch the order and related customer for the confirmation screen
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // --- DELETE: Execute the deletion ---
        // POST: Orders/Delete/5
        // This method physically removes the order from the database.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper Method: Checks if an order exists to handle concurrent edit conflicts
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}