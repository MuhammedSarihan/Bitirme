using System.Linq.Expressions;
using Tasarim.Core.Entities;
//using Tasarim.Core.Entities;

namespace Tasarim.Service.Abstract
{
    public interface IService<T> where T : class, new()
    {
        List<T> GetAll();
        List<T> GetAll(Expression<Func<T, bool>> expression);

        IQueryable<T> GetQueryable();
        T Get(Expression<Func<T, bool>> expression);
        T Find(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);

        int SaveChanges();

        //asenkronlar
        Task<T> FindAsync(int id);
        Task<T> GetAsync(Expression<Func<T, bool>> expression);
        Task<List<T>> GetAllAsync();
        Task<List<T>>GetAllAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task<int> SaveChangesAsync();
    }
}
