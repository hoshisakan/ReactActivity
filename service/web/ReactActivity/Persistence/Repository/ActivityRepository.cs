using Domain;
using Persistence.Repository.IRepository;

namespace Persistence.Repository
{
    public class ActivityRepository : Repository<Activity>, IActivityRepository
    {
        private readonly DataContext _context;

        public ActivityRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Activity activity)
        {
            activity.ModifiedAt = DateTime.Now;
            _context.Activities.Update(activity);
        }
    }
}