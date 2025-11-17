using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerceAPI.Repositries
{
    public interface IRepositroy<T> where T : class
    {
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            );
        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            );
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        public void Update(T entity);
        public void Delete(T entity);
        Task CommitAsync(CancellationToken cancellation = default);
    }
}
