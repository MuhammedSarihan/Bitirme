namespace Tasarim.Core.Entities
{
    public class Favori : IEntity
    {
        public int ID { get; set; }

        public int KullaniciID { get; set; }
        public Kullanici Kullanici { get; set; }

        public int UrunID { get; set; }
        public Urun Urun { get; set; }
    }
}