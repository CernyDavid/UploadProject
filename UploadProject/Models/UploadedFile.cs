using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UploadProject.Models
{
    public class UploadedFile
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("UploaderId")]
        public IdentityUser Uploader { get; set; }
        [Required]
        public string UploaderId { get; set; }
        [Required]
        public DateTime UploadedAt { get; set; }
        [Required]
        public string OriginalName { get; set; }
        [Required]
        public string ContentType { get; set; }
        [Required]
        public byte[] Blob { get; set; }
    }
}
