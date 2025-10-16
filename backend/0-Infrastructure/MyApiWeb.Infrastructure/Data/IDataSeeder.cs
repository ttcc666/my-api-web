namespace MyApiWeb.Infrastructure.Data
{
    public interface IDataSeeder
    {
        Task SeedAsync(bool forceReinitialize = false);
        string SeedName { get; }
    }
}
