using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ASP_MessageBoard.ViewModels
{
    public class EditPostViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required(ErrorMessage = "請輸入文章內容")]
        [StringLength(2000, ErrorMessage = "文章內容不可超過 2000 個字元")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "文章內容")]
        public string Content { get; set; } = string.Empty;

        public string? ExistingImagePath { get; set; }

        [Display(Name = "更換圖片")]
        public IFormFile? Image { get; set; }

        [Display(Name = "移除目前圖片")]
        public bool RemoveImage { get; set; }
    }
}
