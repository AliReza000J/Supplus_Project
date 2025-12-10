using System.Linq.Expressions;

namespace _0_Framework.Domain
{
    public interface IRepository<TKey, T> where T : class
    {
        Task<T?> GetAsync(TKey id);
        //T? Get(TKey id);

        Task<List<T>> GetAllAsync();
        //List<T> Get();

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        //bool Exists(Expression<Func<T, bool>> predicate);

        void Create(T entity);

        Task SaveChangesAsync();
        //void SaveChanges();
    }
}