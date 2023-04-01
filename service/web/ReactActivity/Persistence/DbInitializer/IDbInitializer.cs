

namespace Persistence.DbInitializer
{
    public interface IDbInitializer
    {
        Task SeedData();
    }
}