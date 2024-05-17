using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UploadProject.Data;
using UploadProject.Models;

namespace UploadProject.Pages
{
    public class UploadModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public ICollection<IFormFile> Upload { get; set; }

        public UploadModel(IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            int successfulProcessing = 0;
            int failedProcessing = 0;
            
            foreach (var uploadedFile in Upload)
            {
                var file = new UploadedFile
                {
                    OriginalName = uploadedFile.FileName,
                    UploaderId = userId,
                    UploadedAt = DateTime.Now,
                    ContentType = uploadedFile.ContentType,
                    Blob = new byte[uploadedFile.Length]
                };
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await uploadedFile.CopyToAsync(memoryStream);
                        file.Blob = memoryStream.ToArray();
                    }

                    _context.UploadedFiles.Add(file);

                    await _context.SaveChangesAsync();
                    successfulProcessing++;
                }
                catch
                {
                    failedProcessing++;
                }
                if (failedProcessing == 0)
                {
                    SuccessMessage = "All files has been uploaded successfuly.";
                }
                else
                {
                    ErrorMessage = "There were " + failedProcessing + " errors during uploading and processing of files.";
                }
            }
            return RedirectToPage("/Index");
        }
    }
}
