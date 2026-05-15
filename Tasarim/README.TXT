YAPILACAKLAR :

1 - Admin panelindei kampanya kısmı komple düzenlenmeli

2 - Siparişlerim kısmındaki kargo takip numarası tıklandığında kargo takip sayfasına yönlendirme yapılmalı ( sürat kargo olduğunu varsayıyoruz )

3 - Hata varsa tespit edili buraya yazılmalı hatanın ne olduğu önemli değil önemli olan hatanın tespit edilmesi ve düzeltilmesi

4 -         public async Task<IActionResult> SifreleriGuvenliHaleGetir()
        {
            var guncellenecekKullanicilar = await _context.Kullanicilar
                .Where(k => !k.Sifre.StartsWith("$"))
                .ToListAsync();

            int islemGorenKullanici = 0;

            foreach (var kullanici in guncellenecekKullanicilar)
            {
                kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(kullanici.Sifre);
                islemGorenKullanici++;
            }
            if (islemGorenKullanici > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Content($"İşlem Başarılı! Toplam {islemGorenKullanici} kullanıcının şifresi doğrudan DbContext üzerinden BCrypt ile güncellendi.");
        }

// Yukarıdaki kod, şifreleri güvenli hale getirmek için kullanılıcak. Home Controller a ekleyip tarayıcıda /Home/SifreleriGuvenliHaleGetir yazarak çalıştırabilirsiniz. Bu kod, şifreleri BCrypt ile hashleyerek güvenli hale getirir. SADECE BİR DEFA ÇALIŞTIRILMALIDIR. ÇALIŞTIRILDIKTAN SONRA KODU SİLMEYİ UNUTMAYINIZ.