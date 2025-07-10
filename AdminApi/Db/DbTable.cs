namespace AdminApi.Db;

public class DbTable
{
    public string Name { get; set; }
    public string DbName { get; set; }
    public Type Type { get; set; }
    public List<DbField> Fields { get; set; }
}