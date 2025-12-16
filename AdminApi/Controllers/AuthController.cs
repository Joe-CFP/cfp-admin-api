using AdminApi.DTO;
using AdminApi.Repositories;
using AdminApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public class AuthController(JwtTokenService tokens, ISecurityService security, IDatabaseRepository db, ILogger<AuthController> log) : ControllerBase
{
    private const int MfaMaxAttempts = 5;

    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        return request.grant_type switch {
            "password" => HandlePasswordGrant(request, request.username),
            "mfa_totp" => HandleMfaTotpGrant(request, request.username),
            "refresh_token" => HandleRefreshGrant(request, request.username),
            _ => Task.FromResult(Reject("unsupported_grant_type", "Unsupported grant_type.", request.username))
        };
    }

    private async Task<IActionResult> HandlePasswordGrant(TokenRequest request, string? attemptedUsername)
    {
        if (string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
            return Reject("missing_username_or_password", "Username and password are required.", attemptedUsername);

        MemberSecurityRecord? member = await security.AuthenticateAsync(request.username, request.password);
        if (member is null)
            return Deny("password_auth_failed", attemptedUsername);

        if (!member.IsAdmin)
            return Deny("not_admin", attemptedUsername);

        if (string.IsNullOrWhiteSpace(member.TotpSecret))
            return Deny("missing_totp_secret", attemptedUsername);

        string loginIp = GetServerObservedIp();

        (string mfaToken, DateTimeOffset expiresUtc) =
            await MfaChallengeService.CreateAsync(db, member.Id, loginIp, HttpContext.RequestAborted);

        int expiresIn = (int)Math.Max(0, (expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);
        return Ok(new { mfa_required = true, mfa_token = mfaToken, expires_in = expiresIn });
    }

    private async Task<IActionResult> HandleMfaTotpGrant(TokenRequest request, string? attemptedUsername)
    {
        if (string.IsNullOrWhiteSpace(request.mfa_token) || string.IsNullOrWhiteSpace(request.totp))
            return Reject("missing_mfa_fields", "MFA token and code are required.", attemptedUsername);

        string mfaTokenHash = MfaChallengeService.ComputeHash(request.mfa_token);

        MfaChallengeRecord? challenge = await db.GetMfaChallengeByHashAsync(mfaTokenHash, HttpContext.RequestAborted);
        if (challenge is null)
            return Deny("mfa_challenge_not_found", attemptedUsername);

        if (challenge.VerifiedUtc.HasValue)
            return Deny("mfa_challenge_already_verified", attemptedUsername);

        if (challenge.ExpiresUtc <= DateTime.UtcNow)
            return Deny("mfa_challenge_expired", attemptedUsername);

        if (challenge.FailedAttempts >= MfaMaxAttempts)
            return Deny("mfa_too_many_attempts", attemptedUsername);

        MemberSecurityRecord? member = await db.GetMemberSecurityRecordByIdAsync(challenge.MemberId);
        if (member is null)
            return Deny("member_not_found_for_mfa_challenge", attemptedUsername);

        if (!member.IsAdmin)
            return Deny("not_admin", attemptedUsername);

        if (string.IsNullOrWhiteSpace(member.TotpSecret))
            return Deny("missing_totp_secret", attemptedUsername);

        bool ok = TotpService.Verify(member.TotpSecret, request.totp, DateTimeOffset.UtcNow);

        if (!ok)
        {
            try
            {
                await db.IncrementMfaChallengeFailedAttemptsAsync(challenge.Id, DateTime.UtcNow, HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "IncrementMfaChallengeFailedAttempts failed for {Username}", SanitizeIdentifier(attemptedUsername));
            }

            return Deny("totp_invalid", attemptedUsername);
        }

        try
        {
            await db.MarkMfaChallengeVerifiedAsync(challenge.Id, DateTime.UtcNow, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            return Deny("mark_mfa_verified_failed", attemptedUsername, ex);
        }

        string loginIp = challenge.LoginIp ?? string.Empty;
        string loginUrl = Request.Headers.Referer.ToString();
        const string referCode = "";

        try
        {
            await db.InsertLoginHistoryAsync(member.Id, loginIp, loginUrl, referCode, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "InsertLoginHistory failed for {Username}", SanitizeIdentifier(attemptedUsername));
        }

        string refreshToken = await RefreshTokenService.IssueAsync(db, member.Id, HttpContext.RequestAborted);

        (string token, DateTimeOffset expiresUtc) = tokens.CreateAdminToken(member.Id, member.Username, member.FirstName, member.LastName);
        int expiresIn = (int)Math.Max(0, (expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);

        return Ok(new { access_token = token, refresh_token = refreshToken, token_type = "Bearer", expires_in = expiresIn });
    }

    private async Task<IActionResult> HandleRefreshGrant(TokenRequest request, string? attemptedUsername)
    {
        if (string.IsNullOrWhiteSpace(request.refresh_token))
            return Reject("missing_refresh_token", "Refresh token is required.", attemptedUsername);

        (int MemberId, string RefreshToken)? rotated = await RefreshTokenService.RotateAsync(db, request.refresh_token, HttpContext.RequestAborted);
        if (rotated is null)
            return Deny("refresh_token_invalid_or_expired", attemptedUsername);

        MemberSecurityRecord? member = await db.GetMemberSecurityRecordByIdAsync(rotated.Value.MemberId);
        if (member is null)
            return Deny("member_not_found_for_refresh", attemptedUsername);

        if (!member.IsAdmin)
            return Deny("not_admin", attemptedUsername);

        (string token, DateTimeOffset expiresUtc) = tokens.CreateAdminToken(member.Id, member.Username, member.FirstName, member.LastName);
        int expiresIn = (int)Math.Max(0, (expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);

        return Ok(new { access_token = token, refresh_token = rotated.Value.RefreshToken, token_type = "Bearer", expires_in = expiresIn });
    }

    private IActionResult Deny(string reason, string? username = null, Exception? ex = null)
    {
        string traceId = HttpContext.TraceIdentifier;
        string safeUsername = SanitizeIdentifier(username);

        if (ex is not null)
            log.LogError(ex, "Auth denied {Reason} for {Username} TraceId={TraceId}", reason, safeUsername, traceId);
        else
            log.LogWarning("Auth denied {Reason} for {Username} TraceId={TraceId}", reason, safeUsername, traceId);

        return Unauthorized("Invalid credentials.");
    }

    private IActionResult Reject(string reason, string message, string? username = null)
    {
        string traceId = HttpContext.TraceIdentifier;
        string safeUsername = SanitizeIdentifier(username);

        log.LogWarning("Auth bad request {Reason} for {Username} TraceId={TraceId}", reason, safeUsername, traceId);
        return BadRequest(message);
    }

    private static string SanitizeIdentifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        string trimmed = value.Trim();
        int len = Math.Min(trimmed.Length, 128);

        char[] buffer = new char[len];
        int j = 0;

        for (int i = 0; i < len; i++)
        {
            char c = trimmed[i];
            if (char.IsControl(c)) continue;
            buffer[j++] = c;
        }

        return j == 0 ? string.Empty : new string(buffer, 0, j);
    }

    private string GetServerObservedIp()
    {
        string forwarded = Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            string first = forwarded.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(first)) return first;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    public sealed record TokenRequest(string? username, string? password, string? grant_type, string? refresh_token, string? mfa_token, string? totp);
}
