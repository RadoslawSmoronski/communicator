using System.Linq.Expressions;

namespace Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> WherePagedAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy,
            bool orderByDescending,
            int pageSize,
            int pageNumber,
            params Expression<Func<T, object>>[] includes);
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
