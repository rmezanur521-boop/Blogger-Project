using Blogger_Project.Data;
using Blogger_Project.Models;
using Blogger_Project.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Blogger_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHost;
        private readonly string[] _allowedExtension = { ".jpg", ".jpeg", ".png" };
        public PostController(AppDbContext context, IWebHostEnvironment webHost)
        {
            _context = context;
            _webHost = webHost;
        }
        [AllowAnonymous]
        public IActionResult Index(int? categoryId)
        {
            var postQuery = _context.Posts.Include(p => p.Category).AsQueryable();
            if(categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId);
            }
            var posts = postQuery.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View(posts);
        }
        
        public IActionResult Create()
        {
            CategoryDropdowns();
            var postviewModel = new PostViewModel();
            postviewModel.Categories = _context.Categories.Select(c =>
            new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View(postviewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create (PostViewModel postviewModel)
        {
            if (ModelState.IsValid)
            {
                var inputFileExtension = Path.GetExtension(postviewModel
                    .FeatureImage.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image format. Allowed forma are .jpg,.jpeg,.png");
                    return View(postviewModel);
                }

              postviewModel.Post.FeatureImagePath= await UploadFiletofolder(postviewModel.FeatureImage);
               await _context.Posts.AddAsync(postviewModel.Post);
               await _context.SaveChangesAsync();
               return RedirectToAction("Index");
            }
            CategoryDropdowns();
            return View(postviewModel);
        }
        private async Task<String> UploadFiletofolder (IFormFile file)
        {
            var inputFileExtension = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid().ToString() + inputFileExtension;
            var wwwRootPath = _webHost.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images");
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }
            var filePath = Path.Combine(imagesFolderPath, fileName);
            try
            {
                await using(var fileStream = new FileStream(filePath, FileMode.Create)) {
                    {
                        await file.CopyToAsync(fileStream);
                    } }
            }
            catch (Exception ex)
            {
                return "Error Uploding Images:" + ex.Message;
            }
            return "/images/" + fileName;
        }
        [AllowAnonymous]

        public IActionResult Detail(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var post = _context.Posts.Include(c=>c.Comments).Include(p=>p.Category)
                .FirstOrDefault(t=>t.Id==id);
            if(post == null)
            {
                return NotFound();                  
            }
            return View(post);
        }
        [Authorize]
        [AllowAnonymous]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Json(new
            {
                userName = comment.UserName,
                commentDate = comment.CommentDate.ToString("MMMM dd, yyyy"),
                content = comment.Content
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            CategoryDropdowns();
            var postViewModel = new PostViewModel
            {
                
               Post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id),
            };  
            return View(postViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int? id, PostViewModel postViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            if (postViewModel.FeatureImage != null)
            {
                var inputFileExtension = Path.GetExtension(postViewModel
                   .FeatureImage.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("Image", "Invalid image format. Allowed formats are .jpg, .jpeg, .png");
                    return View(postViewModel);
                }
                var existingFilePath = Path.Combine(
                    _webHost.WebRootPath, "Images",
                    Path.GetFileName(postFromDb.FeatureImagePath));
                if(System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
                postViewModel.Post.FeatureImagePath = await UploadFiletofolder(postViewModel.FeatureImage);

            }
            else
            {
                postViewModel.Post.FeatureImagePath=postFromDb.FeatureImagePath;
            }
            CategoryDropdowns();
            _context.Entry(postFromDb).State = EntityState.Detached;
            _context.Posts.Update(postViewModel.Post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

            
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);


            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            if (id < 0)
            {
                return BadRequest();
            }

            var postFromDb = await _context.Posts.FindAsync(id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(postFromDb.FeatureImagePath))
            {
                var imagePath = Path.Combine(_webHost.WebRootPath, "images", Path.GetFileName(postFromDb.FeatureImagePath));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            _context.Posts.Remove(postFromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        //Category
        public IActionResult FetchCategory() 
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCategory(Category cat)
        {
            _context.Categories.Add(cat);
            _context.SaveChanges();
            TempData["Success"] = "Category added successfully!";
            return RedirectToAction("FetchCategory");
        }

        public IActionResult UpdateCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCategory(Category cat)
        {
            var existing = _context.Categories.Find(cat.Id);
            if (existing == null)
                return NotFound();

            existing.Name = cat.Name;

            _context.Categories.Update(existing);
            _context.SaveChanges();

            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction("FetchCategory");

        }
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteParCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["Success"] = "Category deleted successfully!";
            }
            return RedirectToAction("FetchCategory");
        }
        // ================== Private Helper Methods ==================

        private void CategoryDropdowns()
        {

            ViewBag.Categries = new SelectList(_context.Categories.OrderBy(t => t.Name), "Id", "Name");
        }
    }
}
