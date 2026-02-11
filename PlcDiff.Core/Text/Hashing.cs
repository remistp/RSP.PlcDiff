using System.Security.Cryptography;
using System.Text;

namespace PlcDiff.Core.Text;

public static class Hashing
{
    public static string ComputeKey(string normalizedText, string? rockwellId)
    {
        if (!string.IsNullOrWhiteSpace(rockwellId))
        {
            return rockwellId;
        }

        var bytes = Encoding.UTF8.GetBytes(normalizedText ?? string.Empty);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
