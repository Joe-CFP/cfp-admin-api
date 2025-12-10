using AdminApi.DTO;
using AdminApi.Repositories;
using CryptSharp.Core;

namespace AdminApi.Services;

public interface ISecurityService
{
    Task<MemberSecurityRecord?> AuthenticateAsync(string username, string password);
}

public sealed class SecurityService(IDatabaseRepository db) : ISecurityService
{
    public async Task<MemberSecurityRecord?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return null;

        MemberSecurityRecord? member = await db.GetMemberSecurityRecordByUsernameAsync(username);
        if (member is null) return null;
        if (string.IsNullOrEmpty(member.HashedPassword)) return null;

        bool passwordCorrect = Crypter.CheckPassword(password, member.HashedPassword);
        bool loginNotExpired = DateTime.Now.Date <= member.LoginExpiry.Date;
        return passwordCorrect && loginNotExpired ? member : null;
    }
}