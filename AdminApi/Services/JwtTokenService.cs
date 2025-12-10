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

    public (string Token, DateTimeOffset ExpiresUtc) CreateAdminToken(int memberId, string username, string firstName, string lastName)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = now.AddMinutes(1);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, memberId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, memberId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.GivenName, firstName),
            new Claim(ClaimTypes.Surname, lastName),
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
