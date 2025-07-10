namespace AdminApi.Db;

public class DbField
{
    public string Name { get; set; }
    public string DbName { get; set; }
    public string Description { get; set; }
    public Type Type { get; set; }
    public string DbType { get; set; }
    public int? MaxLength { get; set; }
}