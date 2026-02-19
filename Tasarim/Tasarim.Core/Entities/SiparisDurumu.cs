using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class SiparisDurumu : IEntity
    {
        public int ID { get; set; }

        [Display(Name = "Durum Adı")]
        public string DurumAd { get; set; }

        public ICollection<Siparis>? Siparisler { get; set; }
    }
}