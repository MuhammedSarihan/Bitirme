using System.ComponentModel.DataAnnotations;

namespace Tasarim.Core.Entities
{
    public class YorumAnaliz : IEntity
    {
        public int ID { get; set; }

        public string Duygu { get; set; } // Pozitif/Negatif/Nötr
        public string Artilar { get; set; }
        public string Eksiler { get; set; }
        public string Sikayetler { get; set; }
        public string Oneriler { get; set; }

        public int YorumID { get; set; }
        public Yorum Yorum { get; set; }
    }
}