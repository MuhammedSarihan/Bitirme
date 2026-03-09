using System.ComponentModel.DataAnnotations;

namespace Tasarim.Models.Hesap
{
    public class LoginViewModel
    {
        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        public string KullaniciAd { get; set; }

        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; }
    }
}