using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Generates self-signed TLS certificates for local development and non-production HTTPS.
/// </summary>
/// <remarks>
/// <para>
/// Self-signed certificates are NOT a substitute for certificates issued by a trusted
/// Certificate Authority in production. They are intended for development environments,
/// internal networks, and automated smoke tests where a public certificate is impractical.
/// For public production deployments, configure <c>Certificates:Path</c> to point at a
/// proper CA-issued certificate.
/// </para>
/// <para>
/// Generated certificates:
/// </para>
/// <list type="bullet">
/// <item><description>Use an ECDSA P-256 key pair (fast, small, modern)</description></item>
/// <item><description>Include a Subject Alternative Name (SAN) covering the configured host and <c>localhost</c></description></item>
/// <item><description>Are valid for the configured number of days (default 365)</description></item>
/// <item><description>Are exported as a password-protected PKCS#12 (PFX) file</description></item>
/// </list>
/// </remarks>
public static class CertificateGenerator
{
    /// <summary>
    /// Creates a self-signed certificate and persists it as a PFX file.
    /// </summary>
    /// <param name="outputPath">Absolute or relative path of the PFX file to write.</param>
    /// <param name="password">Optional password protecting the PFX file and its private key.</param>
    /// <param name="host">Primary DNS name / CN for the certificate (e.g. "localhost").</param>
    /// <param name="daysValid">Validity period in days. Must be between 1 and 825.</param>
    /// <returns>The loaded <see cref="X509Certificate2"/> ready to be used by Kestrel.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="outputPath"/> or <paramref name="host"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="daysValid"/> is outside the supported range.</exception>
    public static X509Certificate2 CreateSelfSignedCertificate(
        string outputPath,
        string? password,
        string host,
        int daysValid = 365
    )
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(host);

        if (daysValid is < 1 or > 825)
        {
            throw new ArgumentOutOfRangeException(
                nameof(daysValid),
                "Validity must be between 1 and 825 days."
            );
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest($"CN={host}", ecdsa, HashAlgorithmName.SHA256);

        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName(host);
        if (!string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            sanBuilder.AddDnsName("localhost");
        }

        request.CertificateExtensions.Add(sanBuilder.Build());
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: false
            )
        );
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") },
                critical: false
            )
        );

        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(daysValid)
        );

        var pfxBytes = certificate.Export(X509ContentType.Pfx, password);
        File.WriteAllBytes(outputPath, pfxBytes);

        return X509CertificateLoader.LoadPkcs12FromFile(outputPath, password);
    }

    /// <summary>
    /// Loads an existing certificate from a PFX file, or creates and persists a self-signed
    /// certificate when the file does not exist.
    /// </summary>
    /// <param name="path">Path of the certificate file.</param>
    /// <param name="password">Optional password protecting the PFX file.</param>
    /// <param name="host">Host name used only when a new certificate is generated.</param>
    /// <param name="daysValid">Validity period used only when a new certificate is generated.</param>
    /// <returns>The loaded or newly created <see cref="X509Certificate2"/>.</returns>
    public static X509Certificate2 LoadOrCreate(
        string path,
        string? password,
        string host,
        int daysValid = 365
    )
    {
        ArgumentNullException.ThrowIfNull(path);

        if (File.Exists(path))
        {
            return X509CertificateLoader.LoadPkcs12FromFile(path, password);
        }

        return CreateSelfSignedCertificate(path, password, host, daysValid);
    }
}
