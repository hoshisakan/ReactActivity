using System.Linq.Expressions;

namespace Persistence.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAll(Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null);
        Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true);
        Task<T> GetById(Guid id);
        Task Add(T entity);
        void Remove(T entity);
    }
}