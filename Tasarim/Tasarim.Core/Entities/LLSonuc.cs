using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class LLSonuc : IEntity
    {
        public int ID { get; set; }

        public int ToplamYorum { get; set; }
        public string DuyguDagilim { get; set; }
        public string TopArtilar { get; set; }
        public string TopEksiler { get; set; }
        public string TopSikayetler { get; set; }
        public string TopOneriler { get; set; }
        public DateTime SonGuncelleme { get; set; }

        public int UrunID { get; set; }
        public Urun Urun { get; set; }
    }
}