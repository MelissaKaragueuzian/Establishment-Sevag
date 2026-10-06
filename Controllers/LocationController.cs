using Microsoft.AspNetCore.Mvc;
using Website_Idea.Models;

namespace Website_Idea.Controllers;

[Route("location")]
public class LocationController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Location";
        return View((CompanyInfo)ViewData["Company"]!);
    }
}
