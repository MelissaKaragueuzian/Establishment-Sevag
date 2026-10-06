using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Idea.Data;

namespace Website_Idea.Controllers;

[Route("gallery")]
public class GalleryController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Gallery";
        var images = await db.GalleryImages.AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
        return View(images);
    }
}
