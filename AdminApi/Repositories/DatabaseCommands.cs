using System.Reflection;
using AdminApi.Lib;
namespace AdminApi.Repositories;

public interface IDatabaseCommands
{
    Task CreateMemberAsync(string username, string firstName, string lastName);
    Task UpdateMemberAsync(int id, string firstName, string lastName);
}

public class DatabaseCommands : IDatabaseCommands
{
    private readonly string _connectionString;

    public DatabaseCommands(ISecretStore secrets)
    {
        Secret db = secrets[SecretName.ProdDatabase];
        _connectionString =
            $"Server={db.Endpoint};User={db.Username};Password={db.Password};" +
            $"Database=login;Convert Zero Datetime=true;Connect Timeout=30;Pooling=true;";
    }

    public async Task CreateMemberAsync(string username, string firstName, string lastName)
    {
        const string sql = """
                               INSERT INTO members (username, email, firstname, lastname, verified, mod_timestamp)
                               VALUES (@Username, @Email, @FirstName, @LastName, 0, NOW());
                           """;

        var parameters = new
        {
            Username = username,
            Email = username,
            FirstName = firstName,
            LastName = lastName
        };

        PrintSql(sql, parameters);
        await Task.CompletedTask;
    }

    public async Task UpdateMemberAsync(int id, string firstName, string lastName)
    {
        const string sql = """
                               UPDATE members
                               SET firstname = @FirstName,
                                   lastname = @LastName,
                                   mod_timestamp = NOW()
                               WHERE id = @Id;
                           """;

        var parameters = new
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName
        };

        PrintSql(sql, parameters);
        await Task.CompletedTask;
    }

    private static void PrintSql(string sql, object parameters)
    {
        Console.WriteLine("---- SQL COMMAND (not executed) ----");
        Console.WriteLine(sql);
        Console.WriteLine("PARAMS:");
        foreach (PropertyInfo prop in parameters.GetType().GetProperties())
            Console.WriteLine($"  {prop.Name} = {prop.GetValue(parameters) ?? "NULL"}");
        Console.WriteLine("-----------------------------------");
    }
}