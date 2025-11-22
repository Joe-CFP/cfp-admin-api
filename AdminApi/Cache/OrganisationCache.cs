using AdminApi.Entities;
using AdminApi.Repositories;

namespace AdminApi.Cache;

public interface IOrganisationCache
{
    IReadOnlyList<OrganisationPreview> GetAll();
    List<OrganisationPreview> Search(string query, int limit = 10);
}

public class OrganisationCache : IOrganisationCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private List<OrganisationPreview> _organisations = [];
    private Timer? _timer;

    public OrganisationCache(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        LoadOrganisations().GetAwaiter().GetResult();
        _timer = new(_ => Refresh(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public IReadOnlyList<OrganisationPreview> GetAll() => _organisations.AsReadOnly();

    public List<OrganisationPreview> Search(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        return _organisations
            .Where(o => o.Name.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            .OrderBy(o => o.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(o => o.Name)
            .Take(limit)
            .ToList();
    }

    private async Task LoadOrganisations()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IDatabaseRepository db = scope.ServiceProvider.GetRequiredService<IDatabaseRepository>();

        _organisations = (await db.GetAllOrganisationPreviewsAsync()).ToList();
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