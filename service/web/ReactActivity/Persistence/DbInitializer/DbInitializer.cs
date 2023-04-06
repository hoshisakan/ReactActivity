using Domain;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;
        private readonly DataContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        public DbInitializer(ILogger<DbInitializer> logger, DataContext db,
            UserManager<AppUser> userManager, IConfiguration config)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
            _config = config;
        }

        public async Task SeedData()
        {
            _logger.LogInformation("Seeding database...");

            if (_db.Database.GetPendingMigrationsAsync().Result.Any())
            {
                _logger.LogInformation("Applying migrations...");
                await _db.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("No migrations to apply.");
            }

            if (!_userManager.Users.Any())
            {
                var users = new List<AppUser>
                {
                    new AppUser
                    {
                        DisplayName = "Bob",
                        UserName = "bob",
                        Email = "bob@test.com"
                    },
                    new AppUser
                    {
                        DisplayName = "Tom",
                        UserName = "tom",
                        Email = "tom@tset.com"
                    },
                    new AppUser
                    {
                        DisplayName = "Jane",
                        UserName = "jane",
                        Email = "jane@test.com"
                    }
                };

                string defaultNormalUserPassword = _config.GetSection("Roles:NormalUser:Password").Value ?? string.Empty;

                if (string.IsNullOrEmpty(defaultNormalUserPassword))
                {
                    _logger.LogError("Default normal user password is not set in appsettings.json");
                    return;
                }

                foreach (var user in users)
                {
                    await _userManager.CreateAsync(user, defaultNormalUserPassword);
                }
            };

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