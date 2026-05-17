using Microsoft.EntityFrameworkCore;
using Tasarim.Core.Entities;
using Tasarim.Data;
using Tasarim.Service.Concrate;

public class KullaniciService : Service<Kullanici>, IKullaniciService
{
    public KullaniciService(DatabaseContext context) : base(context) { }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        // 1. Kullanıcıyı getir
        var kullanici = await _dbSet.FindAsync(userId);
        if (kullanici == null) return false;

        // 2. Mevcut şifre doğru mu kontrol et (BCrypt Verify)
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, kullanici.Sifre))
        {
            return false; // Şifre uyuşmuyor
        }

        // 3. Yeni şifreyi hashle ve ata
        kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(newPassword);

        // 4. Güncelle ve kaydet
        _dbSet.Update(kullanici);
        await _context.SaveChangesAsync();

        return true;
    }
}