using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;   
namespace Backend_Sistema_Central.Services;

public interface IChallengeService
{
    byte[] Create(string serial);
    byte[] Get(string serial);
}

public class ChallengeService(IMemoryCache cache) : IChallengeService
{
    private static readonly TimeSpan TTL = TimeSpan.FromMinutes(2);

    public byte[] Create(string serial)
    {
        var challenge = RandomNumberGenerator.GetBytes(32);
        cache.Set(serial, challenge, TTL);
        return challenge;
    }

    public byte[] Get(string serial) =>
        cache.TryGetValue(serial, out byte[]? c) ? c : [];
}
