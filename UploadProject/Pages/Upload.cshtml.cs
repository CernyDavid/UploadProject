using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UploadProject.Data;
using UploadProject.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats;

namespace UploadProject.Pages
{
    public class UploadModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
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
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            int successfulProcessing = 0;
            int failedProcessing = 0;

            foreach (var uploadedFile in Upload)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await uploadedFile.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();

                        if (uploadedFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var image = Image.Load(fileBytes))
                            {
                                var maxPixels = 2_000_000;
                                var originalWidth = image.Width;
                                var originalHeight = image.Height;
                                var originalPixels = originalWidth * originalHeight;
                                if (originalPixels > maxPixels)
                                {
                                    var ratio = Math.Sqrt((double)maxPixels / originalPixels);
                                    var newWidth = (int)(originalWidth * ratio);
                                    var newHeight = (int)(originalHeight * ratio);

                                    image.Mutate(x => x.Resize(newWidth, newHeight));
                                }

                                using (var outputStream = new MemoryStream())
                                {
                                    var extension = Path.GetExtension(uploadedFile.FileName).ToLower();
                                    IImageEncoder encoder = extension switch
                                    {
                                        ".png" => new PngEncoder(),
                                        ".gif" => new GifEncoder(),
                                        _ => new JpegEncoder(),
                                    };

                                    image.Save(outputStream, encoder);
                                    fileBytes = outputStream.ToArray();
                                }
                            }
                        }

                        var file = new UploadedFile
                        {
                            OriginalName = uploadedFile.FileName,
                            UploaderId = userId,
                            UploadedAt = DateTime.Now,
                            ContentType = uploadedFile.ContentType,
                            Blob = fileBytes
                        };
                        _context.UploadedFiles.Add(file);
                    }

                    await _context.SaveChangesAsync();
                    successfulProcessing++;
                }
                catch
                {
                    failedProcessing++;
                }
            }

            if (failedProcessing == 0)
            {
                SuccessMessage = "All files have been uploaded successfully.";
            }
            else
            {
                ErrorMessage = $"There were {failedProcessing} errors during the uploading and processing of files.";
            }

            return RedirectToPage("/Index");
        }
    }

}