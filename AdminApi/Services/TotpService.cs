using System.Security.Cryptography;

namespace AdminApi.Services;

public static class TotpService
{
    public static bool Verify(string base32Secret, string code, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(base32Secret) || string.IsNullOrWhiteSpace(code)) return false;

        string trimmed = code.Trim();
        if (trimmed.Length != 6) return false;
        if (!int.TryParse(trimmed, out int expected)) return false;

        byte[] key;
        try
        {
            key = Base32Decode(base32Secret);
        }
        catch
        {
            return false;
        }

        long counter = nowUtc.ToUnixTimeSeconds() / 30;

        for (long drift = -1; drift <= 1; drift++)
        {
            int actual = Hotp(key, counter + drift, 6);
            if (actual == expected) return true;
        }

        return false;
    }

    private static int Hotp(byte[] key, long counter, int digits)
    {
        Span<byte> msg = stackalloc byte[8];
        ulong c = (ulong)counter;
        msg[0] = (byte)(c >> 56);
        msg[1] = (byte)(c >> 48);
        msg[2] = (byte)(c >> 40);
        msg[3] = (byte)(c >> 32);
        msg[4] = (byte)(c >> 24);
        msg[5] = (byte)(c >> 16);
        msg[6] = (byte)(c >> 8);
        msg[7] = (byte)c;

        Span<byte> hash = stackalloc byte[20];
        using HMACSHA1 hmac = new(key);
        hmac.TryComputeHash(msg, hash, out int written);

        int offset = hash[written - 1] & 0x0f;

        int binary =
            ((hash[offset] & 0x7f) << 24) |
            ((hash[offset + 1] & 0xff) << 16) |
            ((hash[offset + 2] & 0xff) << 8) |
            (hash[offset + 3] & 0xff);

        int mod = 1;
        for (int i = 0; i < digits; i++) mod *= 10;

        return binary % mod;
    }

    private static byte[] Base32Decode(string input)
    {
        string s = input.Trim().Replace(" ", string.Empty).TrimEnd('=').ToUpperInvariant();

        if (s.Length == 0) return Array.Empty<byte>();

        int byteCount = s.Length * 5 / 8;
        byte[] bytes = new byte[byteCount];

        int buffer = 0;
        int bitsLeft = 0;
        int index = 0;

        for (int i = 0; i < s.Length; i++)
        {
            int val = CharToBase32(s[i]);
            buffer = (buffer << 5) | val;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bytes[index++] = (byte)(buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }
        }

        if (index != bytes.Length)
            Array.Resize(ref bytes, index);

        return bytes;
    }

    private static int CharToBase32(char c)
    {
        if (c >= 'A' && c <= 'Z') return c - 'A';
        if (c >= '2' && c <= '7') return 26 + (c - '2');
        throw new FormatException("Invalid base32 character.");
    }
}
