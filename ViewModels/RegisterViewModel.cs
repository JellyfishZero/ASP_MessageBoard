using System.ComponentModel.DataAnnotations;

namespace ASP_MessageBoard.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入使用者名稱")]
        [StringLength(50, ErrorMessage = "使用者名稱不可超過 50 個字元")]
        [Display(Name = "使用者名稱")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入手機號碼")]
        [StringLength(20, ErrorMessage = "手機號碼不可超過 20 個字元")]
        [Phone(ErrorMessage = "手機號碼格式不正確")]
        [Display(Name = "手機號碼")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入 Email")]
        [StringLength(254)]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "自我介紹")]
        [StringLength(500, ErrorMessage = "自我介紹不可超過 500 個字元")]
        [DataType(DataType.MultilineText)]
        public string? Biography { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "密碼長度必須介於 8 到 100 個字元")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請再次輸入密碼")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "密碼與確認密碼不一致")]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
