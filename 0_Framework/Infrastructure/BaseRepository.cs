using _0_Framework.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace _0_Framework.Infrastructure
{
    public class BaseRepository<TKey, T> : IRepository<TKey, T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _set;

        public BaseRepository(DbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public virtual void Create(T entity)
        {
            _context.Add(entity);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression, CancellationToken cancellation)
        {
            return await _set.AnyAsync(expression, cancellation);
        }

        public virtual async Task<T?> GetAsync(TKey id, CancellationToken cancellation = default)
        {
            return await _set.FindAsync(id);
        }

        public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellation)
        {
            return await _set.ToListAsync(cancellation);
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellation)
        {
            await _context.SaveChangesAsync(cancellation);
        }


        //public bool Exists(Expression<Func<T, bool>> expression)
        //{
        //    return _context.Set<T>().Any(expression);
        //}
        //////////////////////
        //public T Get(TKey id)
        //{
        //    return _context.Find<T>(id);
        //}

        //public List<T> Get()
        //{
        //    return _context.Set<T>().ToList();
        //}

        //public void SaveChanges()
        //{
        //    _context.SaveChanges();
        //}
    }
}
