using System.Security.Cryptography;              // ⬅️  Faltaba
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Backend_Sistema_Central.Services
{
    public interface ICertificateValidator
    {
        bool IsSignedByRoot(string pem, out X509Certificate2 cert);
        bool VerifySignature(X509Certificate2 cert, string challengeB64, string signatureB64);
    }

    public class CertificateValidator : ICertificateValidator
    {
        private readonly X509Certificate2 _rootCert;

        public CertificateValidator(IConfiguration cfg)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "certs", "rootCA.pem");
            _rootCert = new X509Certificate2(path);
        }

        public bool IsSignedByRoot(string pem, out X509Certificate2 cert)
        {
            try
            {
                cert = X509Certificate2.CreateFromPem(pem);

                using var chain = new X509Chain();
                chain.ChainPolicy.ExtraStore.Add(_rootCert);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain.ChainPolicy.RevocationMode    = X509RevocationMode.NoCheck;

                return chain.Build(cert);
            }
            catch
            {
                cert = null!;
                return false;
            }
        }

        /* NUEVO — validación de firma */
        public bool VerifySignature(X509Certificate2 cert,
                                    string challengeB64,
                                    string signatureB64)
        {
            try
            {
                byte[] challenge = Convert.FromBase64String(challengeB64);
                byte[] signature = Convert.FromBase64String(signatureB64);

                using var rsa = cert.GetRSAPublicKey();
                return rsa!.VerifyData(
                    challenge,
                    signature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }
    }
}
