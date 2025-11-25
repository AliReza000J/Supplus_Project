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

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            return await _set.AnyAsync(expression);
        }

        public virtual async Task<T?> GetAsync(TKey id)
        {
            return await _set.FindAsync(id);
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _set.ToListAsync();
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        public bool Exists(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Any(expression);
        }

        public T Get(TKey id)
        {
            return _context.Find<T>(id);
        }

        public List<T> Get()
        {
            return _context.Set<T>().ToList();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
