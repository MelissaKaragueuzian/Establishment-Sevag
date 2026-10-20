using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Idea.Data;
using Website_Idea.Models;
using Website_Idea.ViewModels;

namespace Website_Idea.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var company = (CompanyInfo)ViewData["Company"]!;

        var featured = await db.Products.AsNoTracking()
            .Include(p => p.Images)
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToListAsync();

        if (featured.Count == 0)
        {
            featured = await db.Products.AsNoTracking()
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();
        }

        Product? spotlight = null;
        if (company.HeroProductId is int heroId)
        {
            spotlight = await db.Products.AsNoTracking()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == heroId);
        }

        return View(new HomeViewModel
        {
            Company = company,
            Featured = featured,
            Spotlight = spotlight,
            HeroImage = company.HeroImagePath
                ?? spotlight?.Photos.FirstOrDefault()?.ImagePath
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewData["Title"] = "Something went wrong";
        return View();
    }
}
