using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Idea.Data;
using Website_Idea.Models;
using Website_Idea.Services;
using Website_Idea.ViewModels;

namespace Website_Idea.Controllers;

[Route("products")]
public class ProductsController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? category)
    {
        var categories = await db.CategoryNamesAsync();
        var selected = categories.Contains(category ?? "") ? category : null;
        var query = db.Products.AsNoTracking().Include(p => p.Images).AsQueryable();

        if (selected is not null)
            query = query.Where(p => p.Category == selected);

        var products = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewData["Title"] = selected ?? "Products";
        return View(new ProductListViewModel
        {
            Category = selected,
            Products = products
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var product = await db.Products.AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            ViewData["Title"] = "Not found";
            return View("NotFound");
        }

        var related = await db.Products.AsNoTracking()
            .Include(p => p.Images)
            .Where(p => p.Category == product.Category && p.Id != product.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .ToListAsync();

        var company = (CompanyInfo)ViewData["Company"]!;
        ViewData["Title"] = product.Name;

        return View(new ProductDetailsViewModel
        {
            Product = product,
            Related = related,
            WhatsAppUrl = ContactLinks.WhatsApp(
                company.WhatsApp,
                $"Hello, I would like to inquire about {product.Name}.")
        });
    }
}
