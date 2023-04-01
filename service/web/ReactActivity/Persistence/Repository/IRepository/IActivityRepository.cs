using Domain;

namespace Persistence.Repository.IRepository
{
    public interface IActivityRepository : IRepository<Activity>
    {
        void Update(Activity activity);
    }
}