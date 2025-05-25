// Backend_Sistema_Central/Crypto/FixedIvGcm.cs
using System.Security.Cryptography;

namespace Backend_Sistema_Central.Crypto;

public interface IFixedIvGcm
{
    (byte[] cipher, byte[] tag) Encrypt(ReadOnlySpan<byte> plain, ReadOnlySpan<byte> key);
    byte[] Decrypt(ReadOnlySpan<byte> cipher, ReadOnlySpan<byte> tag, ReadOnlySpan<byte> key);
}

public sealed class FixedIvGcm : IFixedIvGcm
{
    private static readonly byte[] IV = new byte[12];   // 96-bit nonce fijo
    private const int TAG_BITS = 128;                   // 16 bytes

    public (byte[] cipher, byte[] tag) Encrypt(ReadOnlySpan<byte> plain, ReadOnlySpan<byte> key)
    {
        using var gcm = new AesGcm(key, TAG_BITS);
        byte[] cipher = new byte[plain.Length];
        byte[] tag    = new byte[TAG_BITS / 8];
        gcm.Encrypt(IV, plain, cipher, tag);
        return (cipher, tag);
    }

    public byte[] Decrypt(ReadOnlySpan<byte> cipher, ReadOnlySpan<byte> tag, ReadOnlySpan<byte> key)
    {
        using var gcm = new AesGcm(key, TAG_BITS);
        byte[] plain = new byte[cipher.Length];
        gcm.Decrypt(IV, cipher, tag, plain);
        return plain;
    }
}
