using AdminApi.DTO;
using AdminApi.Repositories;
using CryptSharp.Core;

namespace AdminApi.Services;

public interface ISecurityService
{
    Task<bool> VerifyPasswordAsync(string email, string password);
}

public sealed class SecurityService(IDatabaseRepository db) : ISecurityService
{
    public async Task<bool> VerifyPasswordAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return false;

        MemberSecurityRecord? member = await db.GetMemberSecurityRecordByEmailAsync(email);
        if (member is null) return false;
        if (string.IsNullOrEmpty(member.HashedPassword)) return false;

        bool passwordCorrect = Crypter.CheckPassword(password, member.HashedPassword);
        bool loginNotExpired = DateTime.Now.Date <= member.LoginExpiry.Date;
        return passwordCorrect && loginNotExpired;
    }
}