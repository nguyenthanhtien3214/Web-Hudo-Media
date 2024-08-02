using System.ComponentModel.DataAnnotations;

namespace website.Models
{
    public class Document
    {
        public int DocumentId { get; set; }

        [Required]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        public string FileType { get; set; } // Ví dụ: "pdf" hoặc "docx"
    }
}
