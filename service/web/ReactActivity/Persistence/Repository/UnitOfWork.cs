using Persistence.Repository.IRepository;

namespace Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private DataContext _db;

        public UnitOfWork(DataContext context)
        {
            _db = context;
            Activity = new ActivityRepository(_db);
        }

        public IActivityRepository Activity { get; private set; }
    
        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}