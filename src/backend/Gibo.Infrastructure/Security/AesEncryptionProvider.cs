using System.Security.Cryptography;
using System.Text;

namespace Gibo.Infrastructure.Security;

public interface IEncryptionProvider
{
    string Encrypt(string plainText);
    string Decrypt(string cipherTextBase64);
}

/// <summary>
/// Implements AES-256-GCM authenticated envelope encryption for sensitive fields at rest.
/// Adheres to Principle IV and UAE PDPL guidelines for protecting confidential health and personal preferences.
/// </summary>
public sealed class AesEncryptionProvider : IEncryptionProvider
{
    // In production, the key is securely loaded from Azure Key Vault or AWS KMS in the UAE region.
    private readonly byte[] _key;

    public AesEncryptionProvider(string? base64Key = null)
    {
        if (string.IsNullOrWhiteSpace(base64Key))
        {
            // Default 256-bit key for local development and test suite execution
            _key = SHA256.HashData(Encoding.UTF8.GetBytes("Gibo_AbuDhabi_2026_SecureDevMasterKey_MustRotateInProd!"));
        }
        else
        {
            _key = Convert.FromBase64String(base64Key);
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        // 12-byte Nonce standard for AES-GCM
        byte[] nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16-byte authentication tag

        using var aesGcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Payload format: [Nonce (12)] + [Tag (16)] + [Ciphertext (N)]
        byte[] payload = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherTextBase64)
    {
        if (string.IsNullOrEmpty(cipherTextBase64)) return cipherTextBase64;

        byte[] payload = Convert.FromBase64String(cipherTextBase64);
        int nonceSize = AesGcm.NonceByteSizes.MaxSize;
        int tagSize = AesGcm.TagByteSizes.MaxSize;

        if (payload.Length < nonceSize + tagSize)
            throw new CryptographicException("Ciphertext payload is truncated or corrupted.");

        byte[] nonce = new byte[nonceSize];
        byte[] tag = new byte[tagSize];
        byte[] cipherBytes = new byte[payload.Length - nonceSize - tagSize];

        Buffer.BlockCopy(payload, 0, nonce, 0, nonceSize);
        Buffer.BlockCopy(payload, nonceSize, tag, 0, tagSize);
        Buffer.BlockCopy(payload, nonceSize + tagSize, cipherBytes, 0, cipherBytes.Length);

        byte[] plainBytes = new byte[cipherBytes.Length];
        using var aesGcm = new AesGcm(_key, tagSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
