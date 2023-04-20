using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace ViteCommerce.Api;

static class Utilities
{
    public static SymmetricSecurityKey GetKeyFromConfig(string secret)
    {
        ReadOnlySpan<byte> secretSpan = Encoding.UTF8.GetBytes(secret);
        var key = new byte[32];
        Span<byte> keySpan = new Span<byte>(key);
        HKDF.DeriveKey(HashAlgorithmName.SHA256, secretSpan, keySpan, ""u8, "NextAuth.js Generated Encryption Key"u8);
        var ssk = new SymmetricSecurityKey(key);
        return ssk;
    }

}
