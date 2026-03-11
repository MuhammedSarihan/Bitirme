using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tasarim.Core.Entities
{
    public class KampanyaUrunleri
    {

        [Display(Name = "Urun ID")]
        public int UrunID { get; set; }
        public Urun Urun { get; set; }


        [Display(Name = "Kampanya ID")]
        public int KampanyaID { get; set; }
        public Kampanya Kampanya { get; set; }
    }
}
