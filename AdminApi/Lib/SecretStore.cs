using System.Text.Json;
using System.Xml;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace AdminApi.Lib;

public class Secret
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string Apikey { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string EncryptionKey { get; init; } = string.Empty;
}

public enum SecretName
{
    ProdEmailBot, ProdDatabase, CoreElastic, AzureOpenAi, AzureOpenAiSw, ProdEcsUser, ProdTranslateUser,
    ProdStripe, TestStripe, ProdTnOpensearch, ProdBedrockUser, ProdMachineKey, ProdCoreOpensearch,
}

public interface ISecretStore
{
    Secret this[SecretName name] { get; }
}

public class SecretStore : ISecretStore
{
    private readonly Dictionary<SecretName, Secret> _secrets;

    private static readonly Dictionary<SecretName, string> AwsSecretNames = new()
    {
        [SecretName.ProdEmailBot] = "prod-emailbot",
        [SecretName.ProdDatabase] = "prod-database",
        [SecretName.CoreElastic] = "core-elastic",
        [SecretName.AzureOpenAi] = "azure-openai",
        [SecretName.AzureOpenAiSw] = "azure-openaisw",
        [SecretName.ProdEcsUser] = "prod-ecsuser",
        [SecretName.ProdTranslateUser] = "prod-translateuser",
        [SecretName.ProdStripe] = "prod-stripe",
        [SecretName.TestStripe] = "test-stripe",
        [SecretName.ProdTnOpensearch] = "prod-tnopensearch",
        [SecretName.ProdBedrockUser] = "prod-bedrockuser",
        [SecretName.ProdMachineKey] = "prod-machinekey",
        [SecretName.ProdCoreOpensearch] = "prod-coreopensearch"
    };

    private SecretStore(Dictionary<SecretName, Secret> secrets)
    {
        _secrets = secrets;
    }

    public static async Task<SecretStore> CreateAsync(IEnumerable<SecretName> requestedSecrets, string configPath, string region)
    {
        if (!File.Exists(configPath))
            throw new($"Missing config at {configPath}");

        XmlDocument doc = new();
        doc.Load(configPath);

        string? accessKey = doc.SelectSingleNode("config/accesskey")?.InnerText;
        string? secretKey = doc.SelectSingleNode("config/secretkey")?.InnerText;

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
            throw new("Access key or secret key missing from config file.");

        AmazonSecretsManagerClient client = new(accessKey, secretKey, RegionEndpoint.GetBySystemName(region));

        Dictionary<SecretName, Secret> secrets = new Dictionary<SecretName, Secret>();

        foreach (SecretName name in requestedSecrets)
        {
            if (!AwsSecretNames.TryGetValue(name, out string? awsName))
                throw new($"No AWS secret name mapping for {name}");

            Secret secret = await FetchSecretAsync(client, awsName);
            secrets[name] = secret;
        }

        return new(secrets);
    }

    public Secret this[SecretName name]
    {
        get
        {
            if (!_secrets.TryGetValue(name, out Secret? value))
                throw new($"Requested secret '${name}' wasn't included in the list specified at creation.");
            return value;
        }
    }

    private static async Task<Secret> FetchSecretAsync(AmazonSecretsManagerClient client, string awsName)
    {
        GetSecretValueRequest request = new()
        {
            SecretId = awsName,
            VersionStage = "AWSCURRENT"
        };

        GetSecretValueResponse response = await client.GetSecretValueAsync(request);

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        Secret? parsed = JsonSerializer.Deserialize<Secret>(response.SecretString, options);
        if (parsed == null)
            throw new($"Failed to deserialize secret: {awsName}");

        return parsed;
    }
}