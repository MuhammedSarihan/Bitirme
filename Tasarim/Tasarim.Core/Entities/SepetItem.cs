using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class SepetItem : IEntity
    {
        public int ID { get; set; }

        [Display(Name = "Adet")]
        public int Adet { get; set; }

        public int SepetID { get; set; }
        public Sepet Sepet { get; set; }

        public int UrunID { get; set; }
        public Urun? Urun { get; set; }
    }
}