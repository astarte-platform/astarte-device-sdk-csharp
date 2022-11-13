using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AstarteDeviceSDKCSharp
{
    public class Crypto
    {

        public string GenerateCsr(string realm, string deviceId, string cryptoStoreDir)
        {

            ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            string fileName = "device.key";

            if (!File.Exists(Path.Combine(cryptoStoreDir, fileName)))
            {
                string privateKey = new(PemEncoding.Write("PRIVATE KEY", ecdsa.ExportECPrivateKey()).ToArray());
                File.WriteAllText(Path.Combine(cryptoStoreDir, fileName), privateKey);
            }
            else
            {
                string privateKey = File.ReadAllText(
                Path.Combine(cryptoStoreDir, fileName)).ToString()
               .Replace("-----BEGIN PRIVATE KEY-----", "")
               .Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
                ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);
            }

            var cert = new CertificateRequest($"O=Devices,CN={realm}/{deviceId}", ecdsa, HashAlgorithmName.SHA256);

            byte[] pkcs10 = cert.CreateSigningRequest();
            StringBuilder builder = new();

            builder.AppendLine("-----BEGIN CERTIFICATE REQUEST-----");

            string base64 = Convert.ToBase64String(pkcs10);

            int offset = 0;
            const int LineLength = 64;

            while (offset < base64.Length)
            {
                int lineEnd = Math.Min(offset + LineLength, base64.Length);
                builder.AppendLine(base64[offset..lineEnd]);
                offset = lineEnd;
            }

            builder.AppendLine("-----END CERTIFICATE REQUEST-----");
            return builder.ToString();
        }
        public void ImportDeviceCertificate(string clientCrt, string cryptoStoreDir)
        {
            var cert = new X509Certificate2(Convert.FromBase64String(clientCrt
                        .Replace("-----BEGIN CERTIFICATE-----", "")
                        .Replace("-----END CERTIFICATE-----", "")
                        .Replace("\n", "")));

            File.WriteAllText(Path.Combine(cryptoStoreDir, "device.crt"), 
                new(PemEncoding.Write("CERTIFICATE", cert.GetRawCertData()).ToArray()));
        }

    }
}



