using System.Security.Cryptography;
using System.Text;

namespace WebChatApplication.Helpers;

public static class SecurityHelper
{
    public static string GenerateSaltedHash(string content, string salt)
    {
        var contentBytes = Encoding.Unicode.GetBytes(content);
        var saltBytes = Encoding.Unicode.GetBytes(salt);

        var contentWithSaltBytes = new byte[contentBytes.Length + saltBytes.Length];
        contentBytes.CopyTo(contentWithSaltBytes, 0);
        saltBytes.CopyTo(contentWithSaltBytes, contentBytes.Length);
        return Convert.ToBase64String(SHA256.HashData(contentWithSaltBytes));
    }
    public static string GenerateTokenFromEmail(string email)
    {
        return GenerateSaltedHash(email, DateTime.UtcNow.Ticks.ToString());
    }

    public static bool HashMatch(string content, string salt, string hash)
    {
        return hash == GenerateSaltedHash(content, salt);
    }
}