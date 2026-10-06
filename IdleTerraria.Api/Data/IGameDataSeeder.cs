namespace IdleTerraria.Api.Data;

public interface IGameDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}