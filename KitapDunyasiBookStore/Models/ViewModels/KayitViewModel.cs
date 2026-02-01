using System.ComponentModel.DataAnnotations;

namespace Veritabani_Odevi.Models.ViewModels
{
    public class KayitViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrar zorunludur")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
