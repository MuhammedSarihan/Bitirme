using Tasarim.Core.Entities;
using Tasarim.Service.Abstract;

public interface IKullaniciService : IService<Kullanici>
{
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}