namespace Persistence.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IActivityRepository Activity { get; }
        Task Save();
    }
}