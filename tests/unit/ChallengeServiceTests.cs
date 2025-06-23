using Backend_Sistema_Central.Services;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography.X509Certificates;
using Xunit;

public class ChallengeServiceTests
{
    /// <summary>Crea una instancia con un MemoryCache real pero aislado.</summary>
    private static ChallengeService CreateSut()
    {
        var mem = new MemoryCache(new MemoryCacheOptions());
        return new ChallengeService(mem);
    }

    [Fact]
    public void Create_ShouldReturn32RandomBytes_AndBeRetrievable()
    {
        // arrange
        var svc    = CreateSut();
        var serial = "USB-123";

        // act
        var challenge = svc.Create(serial);

        // assert
        challenge.Length.Should().Be(32);                 // 32 bytes aleatorios
        svc.Get(serial).Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public void TryGet_ShouldReturnCertAndInvalidateCache()
    {
        // arrange
        var svc    = CreateSut();
        var serial = "USB-XYZ";

        // ► crea un certificado autofirmado temporal
        using var rsa   = RSA.Create(2048);
        var req         = new CertificateRequest("CN=TempUSB", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert        = req.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));

        var challenge = svc.Create(serial, cert);

        // act
        var ok = svc.TryGet(serial, out var gotCert, out var challengeB64);

        // assert
        ok.Should().BeTrue();
        gotCert!.Thumbprint.Should().Be(cert.Thumbprint);
        challengeB64.Should().Be(Convert.ToBase64String(challenge));

        // y ahora debe estar invalidado
        svc.Get(serial).Should().BeEmpty();
    }
}
