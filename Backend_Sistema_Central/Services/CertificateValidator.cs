using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography; 
namespace Backend_Sistema_Central.Services;

public interface ICertificateValidator
{
    bool IsSignedByRoot(string pem, out X509Certificate2 cert);
    bool VerifySignature(string serial, byte[] challenge, byte[] signature);
}

public class CertificateValidator(ApplicationDbContext db, IConfiguration cfg) : ICertificateValidator
{
    private readonly X509Certificate2 _root = new(
        Path.Combine(AppContext.BaseDirectory, "certs", "rootCA.pem"));

    public bool IsSignedByRoot(string pem, out X509Certificate2 cert)
    {
        cert = X509Certificate2.CreateFromPem(pem);
        return cert.Verify() && cert.Issuer == _root.Subject;
    }

    public bool VerifySignature(string serial, byte[] challenge, byte[] signature)
    {
        var usb = db.DispositivosUSB.FirstOrDefault(u => u.Serial == serial && !u.Revoked);
        if (usb is null) return false;

        using var cert = new X509Certificate2(Convert.FromHexString(usb.Thumbprint));
        using var rsa  = cert.GetRSAPublicKey()!;
        return rsa.VerifyData(challenge, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
