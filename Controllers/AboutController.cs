using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Idea.Data;
using Website_Idea.Models;
using Website_Idea.ViewModels;

namespace Website_Idea.Controllers;

[Route("about")]
public class AboutController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "About Us";
        return View(new AboutViewModel
        {
            Company = (CompanyInfo)ViewData["Company"]!,
            Page = await db.AboutPages.AsNoTracking().FirstOrDefaultAsync() ?? AboutPage.CreateDefault()
        });
    }
}
