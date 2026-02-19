using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class Sepet : IEntity
    {
        public int ID { get; set; }

        public int KullaniciID { get; set; }
        public Kullanici Kullanici { get; set; }

        public ICollection<SepetItem>? SepetItems { get; set; }
    }
}