namespace Tasarim.Areas.Admin.Models
{
    public class KampanyaUrunViewModel
    {
        public int UrunID { get; set; }
        public string UrunKod { get; set; }

        public string UrunAd { get; set; }
        public string KategoriAd { get; set; }
        public string MarkaAd { get; set; }
        public string ResimUrl { get; set; }
        public bool SeciliMi { get; set; } // Checkbox'ı işaretli mi getirelim?
    }
}
