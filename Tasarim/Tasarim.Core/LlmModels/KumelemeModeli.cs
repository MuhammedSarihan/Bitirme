using Microsoft.ML.Data;

namespace Tasarim.Core.LLMmodel
{
    public class YorumVerisi
    {
        public string BirlesikYorum { get; set; } = string.Empty;
        public int UrunID { get; set; }
        public int ToplamYorumSayisi { get; set; }
    }

    public class KumeTahmini
    {
        [ColumnName("PredictedLabel")]
        public uint SelectedClusterId { get; set; }
    }
}
