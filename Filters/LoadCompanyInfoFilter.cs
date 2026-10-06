using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Website_Idea.Data;
using Website_Idea.Models;

namespace Website_Idea.Filters;

public class LoadCompanyInfoFilter(AppDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var company = await db.CompanyInfos.AsNoTracking().FirstOrDefaultAsync()
            ?? CompanyInfo.CreateDefault();

        if (context.Controller is Controller controller)
        {
            controller.ViewData["Company"] = company;
            controller.ViewData["Categories"] = await db.CategoryNamesAsync();
        }

        await next();
    }
}
