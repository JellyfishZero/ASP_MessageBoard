using System.ComponentModel.DataAnnotations;

namespace ASP_MessageBoard.ViewModels
{
    public class CreateCommentViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required(ErrorMessage = "請輸入留言內容")]
        [StringLength(1000, ErrorMessage = "留言不可超過 1000 個字元")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "留言內容")]
        public string Content { get; set; } = string.Empty;
    }
}
