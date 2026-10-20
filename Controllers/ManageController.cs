using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Idea.Data;
using Website_Idea.Models;
using Website_Idea.Services;
using Website_Idea.ViewModels;

namespace Website_Idea.Controllers;

[Authorize]
[Route("manage")]
public class ManageController(AppDbContext db, IImageStorage images, IConfiguration config) : Controller
{
    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Index));

        ViewData["Title"] = "Sign in";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["Title"] = "Sign in";
        if (!ModelState.IsValid)
            return View(model);

        if (!PinMatches(model.Pin))
        {
            ModelState.AddModelError(string.Empty, "That password is not correct.");
            return View(model);
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "owner"),
                new Claim(ManagePinStamp.ClaimType, ManagePinStamp.For(config["ManagePin"]))
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var company = await db.CompanyInfos.AsNoTracking().FirstOrDefaultAsync();
        ViewData["Title"] = "Overview";
        return View(new DashboardViewModel
        {
            ProductCount = await db.Products.CountAsync(),
            GalleryCount = await db.GalleryImages.CountAsync(),
            CompanyName = company?.CompanyName ?? "Company"
        });
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products()
    {
        ViewData["Title"] = "Products";
        var products = await db.Products.AsNoTracking()
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(products);
    }

    [HttpGet("products/new")]
    public IActionResult CreateProduct()
    {
        ViewData["Title"] = "Add a product";
        return View("ProductForm", new ProductFormViewModel());
    }

    [HttpPost("products/new")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(160 * 1024 * 1024)]
    public async Task<IActionResult> CreateProduct(ProductFormViewModel model)
    {
        ViewData["Title"] = "Add a product";
        var files = SelectedFiles(model);
        await ValidateProductAsync(model, files);
        if (!ModelState.IsValid)
            return View("ProductForm", model);

        var product = new Product { CreatedAt = DateTime.UtcNow, Images = [] };
        Apply(product, model);
        if (!await TryStoreImagesAsync(product, files))
            return View("ProductForm", model);

        var media = await TryStoreMediaAsync(model);
        if (media is null)
        {
            foreach (var image in product.Images)
                images.Delete(image.ImagePath);
            return View("ProductForm", model);
        }

        product.VideoPath = media.Value.Video;
        product.ModelPath = media.Value.Model;
        db.Products.Add(product);
        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            foreach (var image in product.Images)
                images.Delete(image.ImagePath);
            images.Delete(product.VideoPath);
            images.Delete(product.ModelPath);
            throw;
        }

        TempData["Status"] = "The product was added to the showroom.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("products/{id:int}/edit")]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return Missing();

        ViewData["Title"] = "Edit product";
        return View("ProductForm", ToForm(product));
    }

    [HttpPost("products/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(160 * 1024 * 1024)]
    public async Task<IActionResult> EditProduct(int id, ProductFormViewModel model)
    {
        ViewData["Title"] = "Edit product";
        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return Missing();

        var files = SelectedFiles(model);
        await ValidateProductAsync(model, files);
        if (!ModelState.IsValid)
            return RedisplayEdit(id, model, product);

        var removeIds = model.RemoveImageIds ?? [];
        var removing = product.Images.Where(i => removeIds.Contains(i.Id)).ToList();
        var stored = new List<string>();
        try
        {
            foreach (var file in files)
                stored.Add(await images.SaveAsync(file, "products"));
        }
        catch (InvalidOperationException ex)
        {
            foreach (var path in stored)
                images.Delete(path);

            ModelState.AddModelError(nameof(model.NewImages), ex.Message);
            return RedisplayEdit(id, model, product);
        }

        var media = await TryStoreMediaAsync(model);
        if (media is null)
        {
            foreach (var path in stored)
                images.Delete(path);
            return RedisplayEdit(id, model, product);
        }

        var oldVideo = product.VideoPath;
        var oldModel = product.ModelPath;
        Apply(product, model);
        foreach (var image in removing)
            product.Images.Remove(image);
        foreach (var path in stored)
            product.Images.Add(new ProductImage { ImagePath = path });
        if (media.Value.Video is not null)
            product.VideoPath = media.Value.Video;
        else if (model.RemoveVideo)
            product.VideoPath = null;
        if (media.Value.Model is not null)
            product.ModelPath = media.Value.Model;
        else if (model.RemoveModel)
            product.ModelPath = null;

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            foreach (var path in stored)
                images.Delete(path);
            images.Delete(media.Value.Video);
            images.Delete(media.Value.Model);
            throw;
        }

        foreach (var image in removing)
            images.Delete(image.ImagePath);
        if (oldVideo is not null && oldVideo != product.VideoPath)
            images.Delete(oldVideo);
        if (oldModel is not null && oldModel != product.ModelPath)
            images.Delete(oldModel);

        TempData["Status"] = "The product was updated.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost("products/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return Missing();

        var paths = product.Images.Select(i => i.ImagePath).ToList();
        var videoPath = product.VideoPath;
        var modelPath = product.ModelPath;
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        foreach (var path in paths)
            images.Delete(path);
        images.Delete(videoPath);
        images.Delete(modelPath);

        TempData["Status"] = "The product was removed.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("gallery")]
    public async Task<IActionResult> Gallery()
    {
        ViewData["Title"] = "Gallery";
        await LoadGalleryPageAsync();
        return View(new GalleryUploadViewModel());
    }

    [HttpPost("gallery")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(40 * 1024 * 1024)]
    public async Task<IActionResult> Gallery(GalleryUploadViewModel model)
    {
        ViewData["Title"] = "Gallery";
        if (model.Image is null || model.Image.Length == 0)
            ModelState.AddModelError(nameof(model.Image), "Choose an image.");
        else if (images.Validate(model.Image) is { } problem)
            ModelState.AddModelError(nameof(model.Image), problem);

        if (!string.IsNullOrWhiteSpace(model.Category) && !(await db.CategoryNamesAsync()).Contains(model.Category))
            ModelState.AddModelError(nameof(model.Category), "Choose a category.");

        if (!ModelState.IsValid)
        {
            await LoadGalleryPageAsync();
            return View(model);
        }

        string? stored = null;
        try
        {
            stored = await images.SaveAsync(model.Image!, "gallery");
            db.GalleryImages.Add(new GalleryImage
            {
                ImagePath = stored,
                Title = Clean(model.Title),
                Category = Clean(model.Category),
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            if (stored is not null)
                images.Delete(stored);
            ModelState.AddModelError(nameof(model.Image), ex.Message);
            await LoadGalleryPageAsync();
            return View(model);
        }
        catch
        {
            if (stored is not null)
                images.Delete(stored);
            throw;
        }

        TempData["Status"] = "The image was added to the gallery.";
        return RedirectToAction(nameof(Gallery));
    }

    [HttpPost("gallery/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGalleryImage(int id)
    {
        var image = await db.GalleryImages.FirstOrDefaultAsync(g => g.Id == id);
        if (image is null)
            return Missing();

        var path = image.ImagePath;
        db.GalleryImages.Remove(image);
        await db.SaveChangesAsync();
        images.Delete(path);

        TempData["Status"] = "The image was removed.";
        return RedirectToAction(nameof(Gallery));
    }

    [HttpPost("categories")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(string? name)
    {
        var cleaned = Clean(name);
        if (cleaned is null)
            TempData["Error"] = "Enter a name for the new category.";
        else if (cleaned.Length > 80)
            TempData["Error"] = "Category names can be up to 80 characters.";
        else if (await db.Categories.AnyAsync(c => c.Name == cleaned))
            TempData["Error"] = $"The category \"{cleaned}\" already exists.";
        else
        {
            db.Categories.Add(new Category { Name = cleaned });
            await db.SaveChangesAsync();
            TempData["Status"] = $"The category \"{cleaned}\" was added.";
        }

        return RedirectToAction(nameof(Gallery));
    }

    [HttpPost("categories/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return Missing();

        var productCount = await db.Products.CountAsync(p => p.Category == category.Name);
        if (productCount > 0)
        {
            TempData["Error"] = $"\"{category.Name}\" is used by {productCount} {(productCount == 1 ? "product" : "products")}. Move them to another category first.";
            return RedirectToAction(nameof(Gallery));
        }

        var photos = await db.GalleryImages.Where(g => g.Category == category.Name).ToListAsync();
        foreach (var photo in photos)
            photo.Category = null;

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        TempData["Status"] = photos.Count == 0
            ? $"The category \"{category.Name}\" was deleted."
            : $"The category \"{category.Name}\" was deleted. Its {photos.Count} {(photos.Count == 1 ? "photograph is" : "photographs are")} now under General.";
        return RedirectToAction(nameof(Gallery));
    }

    private async Task LoadGalleryPageAsync()
    {
        ViewBag.Images = await db.GalleryImages.AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var productCounts = await db.Products
            .GroupBy(p => p.Category)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
        var photoCounts = await db.GalleryImages
            .Where(g => g.Category != null)
            .GroupBy(g => g.Category!)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        ViewBag.CategoryUsage = (await db.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync())
            .Select(c => new CategoryUsage(
                c.Id,
                c.Name,
                productCounts.GetValueOrDefault(c.Name),
                photoCounts.GetValueOrDefault(c.Name)))
            .ToList();
    }

    [HttpGet("company")]
    public async Task<IActionResult> Company()
    {
        ViewData["Title"] = "Company information";
        var company = await GetCompanyAsync();
        return View(CompanyFormViewModel.From(company));
    }

    [HttpPost("company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Company(CompanyFormViewModel model)
    {
        ViewData["Title"] = "Company information";
        if (!string.IsNullOrWhiteSpace(model.GoogleMapsUrl))
        {
            var valid = Uri.TryCreate(model.GoogleMapsUrl.Trim(), UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!valid)
                ModelState.AddModelError(nameof(model.GoogleMapsUrl), "Enter a full link starting with https://");
        }

        if (!ModelState.IsValid)
            return View(model);

        var company = await GetCompanyAsync();
        company.CompanyName = model.CompanyName.Trim();
        company.Phone = Clean(model.Phone);
        company.WhatsApp = Clean(model.WhatsApp);
        company.Email = Clean(model.Email);
        company.Address = Clean(model.Address);
        company.GoogleMapsUrl = Clean(model.GoogleMapsUrl);
        company.AboutDescription = Clean(model.AboutDescription)?.Replace("\r\n", "\n").Replace('\r', '\n');
        await db.SaveChangesAsync();

        TempData["Status"] = "Company information was saved. The public site is updated.";
        return RedirectToAction(nameof(Company));
    }

    [HttpGet("home")]
    public async Task<IActionResult> HomePage()
    {
        ViewData["Title"] = "Home page";
        var company = await GetCompanyAsync();
        await LoadProductChoicesAsync();
        return View(HomePageFormViewModel.From(company));
    }

    [HttpPost("home")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(40 * 1024 * 1024)]
    public async Task<IActionResult> HomePage(HomePageFormViewModel model)
    {
        ViewData["Title"] = "Home page";
        var company = await GetCompanyAsync();

        var uploads = new (IFormFile? File, string Field)[]
        {
            (model.HeroImage, nameof(model.HeroImage)),
            (model.HeroLampOff, nameof(model.HeroLampOff)),
            (model.HeroLampOn, nameof(model.HeroLampOn))
        };
        foreach (var (file, field) in uploads)
        {
            if (file is { Length: > 0 } && images.Validate(file) is { } problem)
                ModelState.AddModelError(field, problem);
        }

        if (model.HeroProductId is int productId && !await db.Products.AnyAsync(p => p.Id == productId))
            ModelState.AddModelError(nameof(model.HeroProductId), "Choose a piece from the list.");

        if (!ModelState.IsValid)
        {
            model.ShowCurrentImages(company);
            await LoadProductChoicesAsync();
            return View(model);
        }

        var oldImages = new[] { company.HeroImagePath, company.HeroLampOffPath, company.HeroLampOnPath };
        var stored = new List<string>();
        async Task<string?> ReplaceAsync(IFormFile? file, bool remove, string? current, string field)
        {
            if (file is not { Length: > 0 })
                return remove ? null : current;

            try
            {
                var path = await images.SaveAsync(file, "home");
                stored.Add(path);
                return path;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(field, ex.Message);
                return current;
            }
        }

        company.HeroImagePath = await ReplaceAsync(model.HeroImage, model.RemoveHeroImage, company.HeroImagePath, nameof(model.HeroImage));
        company.HeroLampOffPath = await ReplaceAsync(model.HeroLampOff, model.RemoveHeroLampOff, company.HeroLampOffPath, nameof(model.HeroLampOff));
        company.HeroLampOnPath = await ReplaceAsync(model.HeroLampOn, model.RemoveHeroLampOn, company.HeroLampOnPath, nameof(model.HeroLampOn));

        if (!ModelState.IsValid)
        {
            stored.ForEach(images.Delete);
            db.Entry(company).Reload();
            model.ShowCurrentImages(company);
            await LoadProductChoicesAsync();
            return View(model);
        }

        company.HeroBadge = Clean(model.HeroBadge);
        company.HeroTitle = Clean(model.HeroTitle);
        company.HeroHighlight = Clean(model.HeroHighlight);
        company.HeroText = Clean(model.HeroText)?.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
        company.HeroFeatures = Clean(model.HeroFeatures)?.Replace("\r\n", "\n").Replace('\r', '\n');
        company.HeroProductId = model.HeroProductId;
        company.CollectionKicker = Clean(model.CollectionKicker);
        company.CollectionTitle = Clean(model.CollectionTitle);
        company.CollectionText = Clean(model.CollectionText)?.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            stored.ForEach(images.Delete);
            throw;
        }

        var currentImages = new[] { company.HeroImagePath, company.HeroLampOffPath, company.HeroLampOnPath };
        foreach (var old in oldImages.Where(o => o is not null && !currentImages.Contains(o)))
            images.Delete(old);

        TempData["Status"] = "The home page was saved. The public site is updated.";
        return RedirectToAction(nameof(HomePage));
    }

    [HttpGet("about")]
    public async Task<IActionResult> AboutPage()
    {
        ViewData["Title"] = "About page";
        return View(new AboutPageFormViewModel { Content = await GetAboutPageAsync() });
    }

    [HttpPost("about")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(40 * 1024 * 1024)]
    public async Task<IActionResult> AboutPage(AboutPageFormViewModel model)
    {
        ViewData["Title"] = "About page";
        var page = await GetAboutPageAsync();

        foreach (var (file, field) in new[] { (model.Image1, nameof(model.Image1)), (model.Image2, nameof(model.Image2)) })
        {
            if (file is { Length: > 0 } && images.Validate(file) is { } problem)
                ModelState.AddModelError(field, problem);
        }

        if (!ModelState.IsValid)
            return AboutPageForm(model, page);

        var oldImages = new[] { page.Image1Path, page.Image2Path };
        var stored = new List<string>();
        try
        {
            if (model.Image1 is { Length: > 0 })
            {
                page.Image1Path = await images.SaveAsync(model.Image1, "home");
                stored.Add(page.Image1Path);
            }
            else if (model.RemoveImage1)
            {
                page.Image1Path = null;
            }

            if (model.Image2 is { Length: > 0 })
            {
                page.Image2Path = await images.SaveAsync(model.Image2, "home");
                stored.Add(page.Image2Path);
            }
            else if (model.RemoveImage2)
            {
                page.Image2Path = null;
            }

            page.CopyTextFrom(model.Content);
            await db.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            foreach (var path in stored)
                images.Delete(path);
            ModelState.AddModelError(string.Empty, ex.Message);
            return AboutPageForm(model, page);
        }
        catch
        {
            foreach (var path in stored)
                images.Delete(path);
            throw;
        }

        foreach (var old in oldImages)
        {
            if (old != page.Image1Path && old != page.Image2Path)
                images.Delete(old);
        }

        TempData["Status"] = "The About page was saved. The public site is updated.";
        return RedirectToAction(nameof(AboutPage));
    }

    private ViewResult AboutPageForm(AboutPageFormViewModel model, AboutPage page)
    {
        model.Content.Image1Path = page.Image1Path;
        model.Content.Image2Path = page.Image2Path;
        return View(model);
    }

    private async Task<AboutPage> GetAboutPageAsync()
    {
        var page = await db.AboutPages.FirstOrDefaultAsync();
        if (page is not null)
            return page;

        page = Models.AboutPage.CreateDefault();
        db.AboutPages.Add(page);
        await db.SaveChangesAsync();
        return page;
    }

    private async Task LoadProductChoicesAsync()
    {
        ViewBag.Products = await db.Products.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductChoice(p.Id, p.Name, p.Images.Any()))
            .ToListAsync();
    }

    private async Task<CompanyInfo> GetCompanyAsync()
    {
        var company = await db.CompanyInfos.FirstOrDefaultAsync();
        if (company is not null)
            return company;

        company = CompanyInfo.CreateDefault();
        db.CompanyInfos.Add(company);
        await db.SaveChangesAsync();
        return company;
    }

    private async Task ValidateProductAsync(ProductFormViewModel model, List<IFormFile> files)
    {
        if (!(await db.CategoryNamesAsync()).Contains(model.Category ?? ""))
            ModelState.AddModelError(nameof(model.Category), "Choose a category.");

        if (files.Count > 8)
            ModelState.AddModelError(nameof(model.NewImages), "Upload up to 8 images at a time.");

        foreach (var file in files)
        {
            if (images.Validate(file) is { } problem)
                ModelState.AddModelError(nameof(model.NewImages), problem);
        }
        if (model.NewVideo is { Length: > 0 } video && images.ValidateVideo(video) is { } videoProblem)
            ModelState.AddModelError(nameof(model.NewVideo), videoProblem);

        if (model.NewModel is { Length: > 0 } model3d && images.ValidateModel(model3d) is { } modelProblem)
            ModelState.AddModelError(nameof(model.NewModel), modelProblem);
    }

    // Stores the optional video and 3D model. Returns null, with the problem recorded on the
    // form, when either file is rejected; nothing new is left on disk in that case.
    private async Task<(string? Video, string? Model)?> TryStoreMediaAsync(ProductFormViewModel model)
    {
        string? video = null;
        try
        {
            if (model.NewVideo is { Length: > 0 } videoFile)
                video = await images.SaveVideoAsync(videoFile, "products");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.NewVideo), ex.Message);
            return null;
        }

        try
        {
            var model3d = model.NewModel is { Length: > 0 } modelFile
                ? await images.SaveModelAsync(modelFile, "products")
                : null;
            return (video, model3d);
        }
        catch (InvalidOperationException ex)
        {
            images.Delete(video);
            ModelState.AddModelError(nameof(model.NewModel), ex.Message);
            return null;
        }
    }

    private ViewResult RedisplayEdit(int id, ProductFormViewModel model, Product product)
    {
        model.Id = id;
        model.ExistingImages = product.Photos;
        model.ExistingVideo = product.VideoPath;
        model.ExistingModel = product.ModelPath;
        return View("ProductForm", model);
    }

    private async Task<bool> TryStoreImagesAsync(Product product, List<IFormFile> files)
    {
        var stored = new List<string>();
        try
        {
            foreach (var file in files)
            {
                var path = await images.SaveAsync(file, "products");
                stored.Add(path);
                product.Images.Add(new ProductImage { ImagePath = path });
            }

            return true;
        }
        catch (InvalidOperationException ex)
        {
            foreach (var path in stored)
            {
                images.Delete(path);
                product.Images.RemoveAll(i => i.ImagePath == path);
            }

            ModelState.AddModelError(nameof(ProductFormViewModel.NewImages), ex.Message);
            return false;
        }
    }

    private static List<IFormFile> SelectedFiles(ProductFormViewModel model) =>
        (model.NewImages ?? []).Where(file => file is { Length: > 0 }).ToList();

    private static ProductFormViewModel ToForm(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Category = product.Category,
        Material = product.Material,
        Dimensions = product.Dimensions,
        IsFeatured = product.IsFeatured,
        ExistingImages = product.Photos,
        ExistingVideo = product.VideoPath,
        ExistingModel = product.ModelPath
    };

    private static void Apply(Product product, ProductFormViewModel model)
    {
        product.Name = model.Name.Trim();
        product.Description = model.Description.Trim();
        product.Category = model.Category;
        product.Material = Clean(model.Material);
        product.Dimensions = Clean(model.Dimensions);
        product.IsFeatured = model.IsFeatured;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private bool PinMatches(string? provided)
    {
        var configured = config["ManagePin"];
        if (string.IsNullOrEmpty(configured))
            return false;

        var left = Encoding.UTF8.GetBytes(configured);
        var right = Encoding.UTF8.GetBytes(provided ?? "");
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private IActionResult Missing()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        ViewData["Title"] = "Not found";
        return View("NotFound");
    }
}
