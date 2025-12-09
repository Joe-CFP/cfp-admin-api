using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using AdminApi.Lib;
using Microsoft.IdentityModel.Tokens;
using static AdminApi.Lib.SecretName;

namespace AdminApi.Services;

public sealed class JwtTokenService
{
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(ISecretStore secrets)
    {
        byte[] keyBytes = GetSigningKeyBytes(secrets);
        _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTimeOffset ExpiresUtc) CreateAdminToken(string email)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = now.AddHours(1);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, "admin")
        ];

        JwtSecurityToken jwt = new JwtSecurityToken(
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: _signingCredentials);

        string token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expires);
    }

    public static byte[] GetSigningKeyBytes(ISecretStore secrets)
    {
        string secret = GetSecretString(secrets, ProdMachineKey);

        if (TryDecodeBase64(secret, out byte[] bytes))
            return bytes;

        using JsonDocument doc = JsonDocument.Parse(secret);
        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("encryptionkey", out JsonElement ek))
        {
            string? ekString = ek.GetString();
            if (!string.IsNullOrWhiteSpace(ekString) && TryDecodeBase64(ekString, out bytes))
                return bytes;
        }

        throw new InvalidOperationException("ProdMachineKey was not a base64 string and did not contain an 'encryptionkey' field.");
    }

    private static bool TryDecodeBase64(string value, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            bytes = Convert.FromBase64String(value);
            return bytes.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    private static string GetSecretString(ISecretStore store, SecretName name)
    {
        Type t = store.GetType();

        PropertyInfo? indexer = t.GetProperties()
            .FirstOrDefault(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(SecretName));

        if (indexer is not null)
        {
            object? value = indexer.GetValue(store, new object[] { name });
            if (value is string s) return s;
            if (value is not null) return value.ToString() ?? string.Empty;
        }

        MethodInfo? method = t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m =>
            {
                ParameterInfo[] ps = m.GetParameters();
                return ps.Length == 1 && ps[0].ParameterType == typeof(SecretName);
            });

        if (method is not null)
        {
            object? value = method.Invoke(store, new object[] { name });
            if (value is string s) return s;
            if (value is not null) return value.ToString() ?? string.Empty;
        }

        PropertyInfo? prop = t.GetProperty("MachineKey") ?? t.GetProperty("ProdMachineKey");
        if (prop is not null)
        {
            object? value = prop.GetValue(store);
            if (value is string s) return s;

            if (value is not null)
            {
                PropertyInfo? ek = value.GetType().GetProperty("encryptionkey") ?? value.GetType().GetProperty("EncryptionKey");
                if (ek is not null)
                {
                    object? ekValue = ek.GetValue(value);
                    if (ekValue is string es) return es;
                    if (ekValue is not null) return ekValue.ToString() ?? string.Empty;
                }

                return value.ToString() ?? string.Empty;
            }
        }

        throw new InvalidOperationException($"Unable to read secret '{name}' from ISecretStore.");
    }
}
