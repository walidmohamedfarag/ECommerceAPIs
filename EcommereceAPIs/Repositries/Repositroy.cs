using System.Linq.Expressions;

namespace ECommerceAPI.Repositries
{
    public class Repositroy<T> : IRepositroy<T> where T : class 
    {
        private readonly ApplicationDBContext context;

        private DbSet<T> dbSet;
        public Repositroy(ApplicationDBContext _context)
        {
            context = _context;
            dbSet = context.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            )
        {
            var entities = dbSet.AsQueryable();
            if (expression is not null)
                entities = dbSet.Where(expression);
            if (!tracked)
                entities = entities.AsNoTracking();
            if (includes is not null)
                foreach (var include in includes)
                    entities = entities.Include(include);
            return await entities.ToListAsync(cancellationToken);  
        }
        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            )
        {
            return (await GetAsync(expression , includes , tracked, cancellationToken)).FirstOrDefault();
        }
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await context.AddAsync(entity, cancellationToken);
        }
        public void Update(T entity)
        {
            context.Update(entity);
        }
        public void Delete(T entity)
        {
            context.Remove(entity);
        }
        public async Task CommitAsync(CancellationToken cancellation = default)
        {
            await context.SaveChangesAsync(cancellation);
        }
    }
}
