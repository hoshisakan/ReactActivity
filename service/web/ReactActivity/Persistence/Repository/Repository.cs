using Persistence.Repository.IRepository;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;


namespace Persistence.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DataContext _context;
        internal DbSet<T> dbSet;

        public Repository(DataContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAll(Expression<Func<T, bool>>? filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy, string? includeProperties)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (includeProperties != null)
            {
                foreach(var includeProp in includeProperties.Split(
                    new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                )
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<T> GetById(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;
            
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach(var includeProp in includeProperties.Split(
                    new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                )
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync() ?? throw new Exception("No record found");
        }

        public async Task Add(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }
    }
}