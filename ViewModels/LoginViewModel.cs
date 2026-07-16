using System.ComponentModel.DataAnnotations;

namespace ASP_MessageBoard.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入手機號碼")]
        [StringLength(20, ErrorMessage = "手機號碼不可超過 20 個字元")]
        [Phone(ErrorMessage = "手機號碼格式不正確")]
        [Display(Name = "手機號碼")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }
    }
}
