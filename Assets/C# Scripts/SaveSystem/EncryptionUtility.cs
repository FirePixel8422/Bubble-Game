using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public static class EncryptionUtility
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("cpw98^wnlsourb!L"); // 16 bytes
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("u)'T#xu*wm)c^mw9");  // 16 bytes

    public static async Task<string> EncryptAsync(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream memoryStream = new MemoryStream();
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter writer = new StreamWriter(cryptoStream);

        await writer.WriteAsync(plainText);
        await writer.FlushAsync();
        cryptoStream.FlushFinalBlock();

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static async Task<string> DecryptAsync(string encryptedText)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream memoryStream = new MemoryStream(encryptedBytes);
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader reader = new StreamReader(cryptoStream);

        return await reader.ReadToEndAsync();
    }
}
