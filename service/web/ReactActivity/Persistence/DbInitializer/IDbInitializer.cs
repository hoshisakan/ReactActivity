namespace Persistence.DbInitializer
{
    public interface IDbInitializer
    {
        Task<bool> SeedData();
    }
}