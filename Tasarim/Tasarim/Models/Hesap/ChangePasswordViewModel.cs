using System.ComponentModel.DataAnnotations;

namespace Tasarim.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifrenizi girmelisiniz.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı gereklidir.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler birbiriyle uyuşmuyor.")]
        public string ConfirmPassword { get; set; }
    }
}