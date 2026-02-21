using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Urun : IEntity
    {
        [Display(Name = "Ürün ID")]
        public int ID { get; set; }

        [Display(Name = "Ürün Kodu")]
        public string UrunKod { get; set; }
        [Display(Name = "Model / Grup Kodu")]
        public string? ModelKodu { get; set; }

        [Display(Name = "Başlık")]
        [Required(ErrorMessage = "{0} alanı gereklidir.")]
        public string Baslik { get; set; }
        [Display(Name = "Renk")]
        public string? Renk { get; set; }

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Fiyat")]
        public decimal Fiyat { get; set; }

        [Display(Name = "Ana Resim")]
        public string? AnaResim { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool AktifMi { get; set; }

        // Foreign Keys 
        public int KategoriID { get; set; }
        public int MarkaID { get; set; }

        // Navigation Properties
        public Kategori Kategori { get; set; }
        public Marka Marka { get; set; }

        // YENİ EKLENEN: Bir ürünün birden fazla beden/stok (varyasyon) bilgisi olabilir
        public ICollection<UrunVaryasyon>? Varyasyonlar { get; set; }

        public ICollection<Resim>? Resimler { get; set; }
        public ICollection<Yorum>? Yorumlar { get; set; }
        public ICollection<Favori>? Favoriler { get; set; }
        public ICollection<SepetItem>? SepetItems { get; set; }
        public ICollection<SiparisDetay>? SiparisDetaylari { get; set; }

        public LLSonuc LLSonuc { get; set; } // 1-1 İlişki
    }
}