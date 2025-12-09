using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        Secret machineKey = secrets[ProdMachineKey];

        if (string.IsNullOrWhiteSpace(machineKey.EncryptionKey))
            throw new InvalidOperationException("ProdMachineKey.EncryptionKey is missing.");

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(machineKey.EncryptionKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("ProdMachineKey.EncryptionKey must be a base64 string.", ex);
        }

        if (keyBytes.Length < 32)
            throw new InvalidOperationException("ProdMachineKey.EncryptionKey must decode to at least 32 bytes for HS256.");

        return keyBytes;
    }
}
