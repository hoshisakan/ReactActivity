using Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;
        private readonly DataContext _db;

        public DbInitializer(ILogger<DbInitializer> logger, DataContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task SeedData()
        {
            _logger.LogInformation("Seeding database...");

            //TODO Auto migrate database from records of Migration folder.
            await _db.Database.MigrateAsync();

            if (_db.Activities.Any())
            {
                return;
            }

            var activities = new List<Activity>
            {
                new Activity
                {
                    Title = "Past Activity 1",
                    Date = DateTime.Now.AddMonths(-2),
                    Description = "Activity 2 months ago",
                    Category = "drinks",
                    City = "London",
                    Venue = "Pub",
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                },
                new Activity
                {
                    Title = "Past Activity 2",
                    Date = DateTime.Now.AddMonths(-1),
                    Description = "Activity 1 month ago",
                    Category = "culture",
                    City = "Paris",
                    Venue = "Louvre",
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                },
                new Activity {
                    Title = "Future Activity 1",
                    Date = DateTime.Now.AddMonths(1),
                    Description = "Activity 1 month in future",
                    Category = "culture",
                    City = "London",
                    Venue = "Natural History Museum",
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                },
            };

            await _db.Activities.AddRangeAsync(activities);
            await _db.SaveChangesAsync();
        }
    }
}