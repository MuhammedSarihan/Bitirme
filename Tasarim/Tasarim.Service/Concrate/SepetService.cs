using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data; // Kendi DbContext'inin olduğu namespace'i buraya ekle
using Tasarim.Service.Abstract;

namespace Tasarim.Service.Concrate
{
    public class SepetService : ISepetService
    {
        private readonly DatabaseContext _context;

        // Dependency Injection ile DbContext'i alıyoruz
        public SepetService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Sepet?> GetSepetByKullaniciIdAsync(int kullaniciId)
        {
            return await _context.Sepetler
                .Include(s => s.SepetItems)
                    .ThenInclude(si => si.UrunVaryasyon)
                        .ThenInclude(uv => uv.Urun)
                            .ThenInclude(u => u.KampanyaUrunleri)
                                .ThenInclude(ku => ku.Kampanya)
                .FirstOrDefaultAsync(s => s.KullaniciID == kullaniciId);
        }

        public async Task UrunEkleAsync(int kullaniciId, int urunVaryasyonId, int miktar)
        {
            // GÜVENLİK DUVARI: Önce veritabanındaki gerçek stoğu öğrenelim!
            var varyasyon = await _context.Set<UrunVaryasyon>().FindAsync(urunVaryasyonId);
            if (varyasyon == null) throw new Exception("Seçtiğiniz ürün/beden bulunamadı.");

            var sepet = await _context.Sepetler
                .Include(s => s.SepetItems)
                .FirstOrDefaultAsync(s => s.KullaniciID == kullaniciId);

            if (sepet == null)
            {
                sepet = new Sepet { KullaniciID = kullaniciId };
                _context.Sepetler.Add(sepet);
                await _context.SaveChangesAsync();
            }

            var sepettekiUrun = sepet.SepetItems?.FirstOrDefault(si => si.UrunVaryasyonID == urunVaryasyonId);

            // EĞER ÜRÜN ZATEN SEPETTEYSE (Örn: Sepette 2 tane var, müşteri 1 tane daha ekliyor)
            if (sepettekiUrun != null)
            {
                int yeniToplamMiktar = sepettekiUrun.Adet + miktar;

                // Stok kontrolü yapıyoruz
                if (yeniToplamMiktar > varyasyon.StokAdedi)
                {
                    // Eğer toplam miktar stoğu aşıyorsa, sepetteki adeti maksimum stoğa eşitleriz.
                    sepettekiUrun.Adet = varyasyon.StokAdedi;
                    // İstersen burada throw new Exception("Stok yetersiz"); de diyebilirsin.
                }
                else
                {
                    sepettekiUrun.Adet = yeniToplamMiktar;
                }
            }
            // EĞER ÜRÜN SEPETE İLK DEFA EKLENİYORSA
            else
            {
                // YİNE STOK KONTROLÜ (Form ile oynanmış olabilir)
                int eklenecekMiktar = miktar > varyasyon.StokAdedi ? varyasyon.StokAdedi : miktar;

                var yeniItem = new SepetItem
                {
                    SepetID = sepet.ID,
                    UrunVaryasyonID = urunVaryasyonId,
                    Adet = eklenecekMiktar
                };
                _context.Set<SepetItem>().Add(yeniItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UrunGuncelleAsync(int kullaniciId, int urunVaryasyonId, int miktar)
        {
            var sepet = await _context.Sepetler
                .Include(s => s.SepetItems)
                .FirstOrDefaultAsync(s => s.KullaniciID == kullaniciId);

            if (sepet != null)
            {
                var sepettekiUrun = sepet.SepetItems?.FirstOrDefault(si => si.UrunVaryasyonID == urunVaryasyonId);

                if (sepettekiUrun != null)
                {
                    if (miktar <= 0)
                    {
                        // Miktar 0 veya altındaysa direkt sil
                        _context.Set<SepetItem>().Remove(sepettekiUrun);
                    }
                    else
                    {
                        sepettekiUrun.Adet = miktar;
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task UrunSilAsync(int kullaniciId, int urunVaryasyonId)
        {
            var sepet = await _context.Sepetler
                .Include(s => s.SepetItems)
                .FirstOrDefaultAsync(s => s.KullaniciID == kullaniciId);

            if (sepet != null)
            {
                var sepettekiUrun = sepet.SepetItems?.FirstOrDefault(si => si.UrunVaryasyonID == urunVaryasyonId);
                if (sepettekiUrun != null)
                {
                    _context.Set<SepetItem>().Remove(sepettekiUrun);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task HepsiniSilAsync(int kullaniciId)
        {
            var sepet = await _context.Sepetler
                .Include(s => s.SepetItems)
                .FirstOrDefaultAsync(s => s.KullaniciID == kullaniciId);

            if (sepet != null && sepet.SepetItems != null && sepet.SepetItems.Any())
            {
                // Sepetin içindeki tüm itemleri veritabanından sil
                _context.Set<SepetItem>().RemoveRange(sepet.SepetItems);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> ToplamTutarAsync(int kullaniciId)
        {
            var sepet = await GetSepetByKullaniciIdAsync(kullaniciId);

            if (sepet == null || sepet.SepetItems == null) return 0;

            return sepet.SepetItems.Sum(si => si.Adet * si.UrunVaryasyon.Urun.KampanyaliFiyat);
        }
    }
}