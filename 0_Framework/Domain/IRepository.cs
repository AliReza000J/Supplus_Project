using System.Linq.Expressions;

namespace _0_Framework.Domain
{
    public interface IRepository<TKey, T> where T : class
    {
        Task<T?> GetAsync(TKey id, CancellationToken cancellation);
        //T? Get(TKey id);

        Task<List<T>> GetAllAsync(CancellationToken cancellation);
        //List<T> Get();

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellation);
        //bool Exists(Expression<Func<T, bool>> predicate);

        void Create(T entity);

        Task SaveChangesAsync(CancellationToken cancellation);
        //void SaveChanges();
    }
}