using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Odeme : IEntity
    {
        public int ID { get; set; }

        [Display(Name = "İşlem No")]
        public string IslemNo { get; set; }

        public decimal Tutar { get; set; }
        public DateTime OdemeTarihi { get; set; }
        public string OdemeDurumu { get; set; }

        public int SiparisID { get; set; }
        public Siparis Siparis { get; set; }
    }
}