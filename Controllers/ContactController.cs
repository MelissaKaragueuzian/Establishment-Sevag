using Microsoft.AspNetCore.Mvc;
using Website_Idea.Models;

namespace Website_Idea.Controllers;

[Route("contact")]
public class ContactController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Contact";
        return View((CompanyInfo)ViewData["Company"]!);
    }
}
