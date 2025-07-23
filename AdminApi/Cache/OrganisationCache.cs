using AdminApi.Entities;
using AdminApi.Repositories;

namespace AdminApi.Cache;

public interface IOrganisationCache
{
    IReadOnlyList<Organisation> GetAll();
    Organisation? GetById(int id);
    List<OrganisationSearchResult> Search(string query, int limit = 10);
}

public class OrganisationCache : IOrganisationCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private List<Organisation> _organisations = [];
    // ReSharper disable once NotAccessedField.Local
    private Timer? _timer;

    public OrganisationCache(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        LoadOrganisations().GetAwaiter().GetResult();
        _timer = new Timer(_ => Refresh(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public IReadOnlyList<Organisation> GetAll() => _organisations.AsReadOnly();
    public Organisation? GetById(int id) => _organisations.FirstOrDefault(o => o.Id == id);

    public List<OrganisationSearchResult> Search(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        return _organisations
            .Where(o => o.Name.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            .OrderBy(o => o.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(o => o.Name)
            .Take(limit)
            .Select(o => new OrganisationSearchResult { Id = o.Id, Name = o.Name })
            .ToList();
    }

    private async Task LoadOrganisations()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDatabaseRepository>();

        _organisations = (await db.GetAllOrganisationsAsync()).ToList();
    }
    
    private void Refresh()
    {
        try
        {
            LoadOrganisations().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing org cache: {ex.Message}");
        }
    }
}