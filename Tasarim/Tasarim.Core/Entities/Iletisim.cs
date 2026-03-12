using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class İletisim
    {
        [Key]
        public int Id { get; set; }

        public string? AdSoyad { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Konu { get; set; }

        public string? Mesaj { get; set; }

        public DateTime MesajTarihi { get; set; } = DateTime.Now;
        public bool OkunduMu { get; set; } = false;
    }
}