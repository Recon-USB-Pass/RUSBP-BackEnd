using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Backend_Sistema_Central.Services;

/// <summary>Genera y recuerda desafíos (challenge) firmados por cada USB.</summary>
public interface IChallengeService
{
    /// <summary>Crea un challenge aleatorio y lo asocia al <paramref name="serial"/>.</summary>
    byte[] Create(string serial);

    /// <summary>
    /// Crea un challenge y recuerda TAMBIÉN el certificado del USB para su posterior verificación.
    /// </summary>
    byte[] Create(string serial, X509Certificate2 cert);

    /// <summary>Obtiene el challenge almacenado (vacío si expiró) sin tocar el certificado.</summary>
    byte[] Get(string serial);

    /// <summary>
    /// Obtiene challenge + certificado y los REMUEVE de caché (single-use).  
    /// Devuelve <c>false</c> si ya expiró.
    /// </summary>
    bool TryGet(string serial,
                out X509Certificate2? cert,
                out string? challengeB64);
}

public sealed class ChallengeService(IMemoryCache cache) : IChallengeService
{
    private static readonly TimeSpan TTL = TimeSpan.FromMinutes(2);

    /* ─────────────────────────────── 1) Sólo challenge ─────────────────────────────── */
    public byte[] Create(string serial)
    {
        var challenge = RandomNumberGenerator.GetBytes(32);
        cache.Set(serial + ":c", challenge, TTL);          // “:c” = challenge
        return challenge;
    }

    public byte[] Get(string serial) =>
        cache.TryGetValue(serial + ":c", out byte[]? c) ? c! : [];

    /* ─────────────────────────────── 2) Challenge + Cert ───────────────────────────── */
    public byte[] Create(string serial, X509Certificate2 cert)
    {
        var challenge = Create(serial);                    // reutiliza el anterior
        cache.Set(serial + ":crt", cert, TTL);             // “:crt” = certificado
        return challenge;
    }

    public bool TryGet(string serial,
                       out X509Certificate2? cert,
                       out string? challengeB64)
    {
        bool okC = cache.TryGetValue(serial + ":c",   out byte[]? challenge);
        bool okP = cache.TryGetValue(serial + ":crt", out cert);

        if (okC && okP && challenge is not null)
        {
            // eliminar para evitar re-uso
            cache.Remove(serial + ":c");
            cache.Remove(serial + ":crt");
            challengeB64 = Convert.ToBase64String(challenge);
            return true;
        }

        cert = null;
        challengeB64 = null;
        return false;
    }
}
