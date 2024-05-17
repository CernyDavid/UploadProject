using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using UploadProject.Data;
using UploadProject.Models;

namespace UploadProject.Pages
{
    public class IndexModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public IList<UploadedFile> Files { get; set; } = default!;

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _logger = logger;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Files = await _context.UploadedFiles.ToListAsync();
        }

        public IActionResult OnGetFile(Guid id)
        {
            var file = _context.UploadedFiles.FirstOrDefault(f => f.Id == id);
            if (file != null)
            {
                return File(file.Blob, file.ContentType, file.OriginalName);
            }
            else
            {
                ErrorMessage = "File not found";
                return RedirectToPage();
            }
        }
    }
}
