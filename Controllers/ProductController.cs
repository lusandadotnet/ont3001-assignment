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
    // The Controller handles all user requests and coordinates between the Views and the Models.
    public class ProductsController : Controller
    {
        // Private field to hold our database context
        private readonly StoreManagementDbContext _context;

        // Constructor: Dependency Injection provides the database context to the controller
        public ProductsController(StoreManagementDbContext context)
        {
            _context = context;
        }

        // --- READ: Get all products ---
        // GET: Products
        // This method fetches the list of products from the database and passes it to the Index view.
        public async Task<IActionResult> Index()
        {
            // We use .Include() to eagerly load the related Category data for each product.
            // LINQ: We use .ToListAsync() to execute the query asynchronously.
            var storeManagementDbContext = _context.Products.Include(p => p.Category);
            
            // Returns the Index view, passing in the list of products
            return View(await storeManagementDbContext.ToListAsync());
        }

        // --- READ: Get a single product ---
        // GET: Products/Details/5
        // This method fetches a single product based on the provided ID to show its details.
        public async Task<IActionResult> Details(int? id)
        {
            // If no ID is provided in the URL, return a 404 Not Found error
            if (id == null)
            {
                return NotFound();
            }

            // LINQ: We use .FirstOrDefaultAsync() to find the first product matching the ID.
            // LINQ: We also .Include() the Category to show the category name instead of just the ID.
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            
            // If the database returns null (product doesn't exist), return 404 Not Found
            if (product == null)
            {
                return NotFound();
            }

            // Return the Details view with the specific product
            return View(product);
        }

        // --- CREATE: Show the form ---
        // GET: Products/Create
        // This method displays the empty form for a user to create a new product.
        public IActionResult Create()
        {
            // We pass a list of Categories to the view using ViewData so the user can select a category from a dropdown.
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // --- CREATE: Save the form data ---
        // POST: Products/Create
        // This method receives the submitted form data and saves the new product to the database.
        [HttpPost]
        [ValidateAntiForgeryToken] // Security measure to prevent Cross-Site Request Forgery attacks
        public async Task<IActionResult> Create([Bind("ProductID,ProductName,Price,StockQuantity,CategoryID")] Product product)
        {
            // Verify that the submitted data passes all validation rules defined in the Model
            if (ModelState.IsValid)
            {
                // Add the new product to the Entity Framework tracking context
                _context.Add(product);
                
                // Save the changes asynchronously to the SQL Server database
                await _context.SaveChangesAsync();
                
                // Redirect the user back to the product list (Index method)
                return RedirectToAction(nameof(Index));
            }
            
            // If validation fails, repopulate the category dropdown and return the view with the user's data to show errors
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", product.CategoryId);
            return View(product);
        }

        // --- UPDATE: Show the form ---
        // GET: Products/Edit/5
        // This method fetches a specific product and populates a form so the user can edit it.
        public async Task<IActionResult> Edit(int? id)
        {
            // If no ID is provided, return 404 Not Found
            if (id == null)
            {
                return NotFound();
            }

            // Find the product in the database using its primary key
            var product = await _context.Products.FindAsync(id);
            
            // If it doesn't exist, return 404 Not Found
            if (product == null)
            {
                return NotFound();
            }
            
            // Populate the dropdown list for Categories, pre-selecting the product's current category
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", product.CategoryId);
            
            // Return the Edit view with the product data
            return View(product);
        }

        // --- UPDATE: Save the form data ---
        // POST: Products/Edit/5
        // This method receives the updated form data and applies the changes to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,ProductName,Price,StockQuantity,CategoryID")] Product product)
        {
            // Ensure the ID in the URL matches the ID of the submitted product
            if (id != product.ProductId)
            {
                return NotFound();
            }

            // Verify the submitted data is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the product in the tracking context
                    _context.Update(product);
                    
                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues (if another user deleted/modified the product at the exact same time)
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                // Redirect back to the product list
                return RedirectToAction(nameof(Index));
            }
            
            // If validation fails, return the form with errors
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", product.CategoryId);
            return View(product);
        }

        // --- DELETE: Show confirmation screen ---
        // GET: Products/Delete/5
        // This method fetches a product and displays a screen asking the user to confirm deletion.
        public async Task<IActionResult> Delete(int? id)
        {
            // If no ID provided, return 404 Not Found
            if (id == null)
            {
                return NotFound();
            }

            // LINQ: Fetch the product and its related category
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            
            if (product == null)
            {
                return NotFound();
            }

            // Return the Delete confirmation view
            return View(product);
        }

        // --- DELETE: Execute the deletion ---
        // POST: Products/Delete/5
        // This method physically removes the product from the database after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the product by ID
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Remove it from the tracking context
                _context.Products.Remove(product);
            }

            // Save the deletion to the database
            await _context.SaveChangesAsync();
            
            // Redirect back to the product list
            return RedirectToAction(nameof(Index));
        }

        // Helper Method: Checks if a product exists in the database
        // This is used during the Edit process to handle concurrency exceptions.
        private bool ProductExists(int id)
        {
            // LINQ: .Any() returns true if at least one product matches the ID
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}