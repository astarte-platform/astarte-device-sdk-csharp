using MQTTnet.Client.Options;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AstarteDeviceSDKCSharp.Crypto
{
    public class AstarteCryptoStore : IAstarteCryptoStore
    {
        private X509Certificate2 _certificate ;
        private AstarteMutualTLSParametersFactory? _parametersFactory;
        private readonly string _cryptoStoreDir = string.Empty;

        public AstarteCryptoStore(string cryptoStoreDir)
        {
            _cryptoStoreDir = cryptoStoreDir;
            LoadNewCertificate();
            
        }

        private void LoadNewCertificate()
        {
           
            if (File.Exists(_cryptoStoreDir + @"\device.crt"))
            {
                ECDsa ecdsa = GenerateKey();

                //Set certificate
                X509Certificate2 caCert = new(File.ReadAllBytes(_cryptoStoreDir + @"\device.crt"));
                X509Certificate2 caCertWithKey = caCert.CopyWithPrivateKey(ecdsa);
                _certificate = new(caCertWithKey.Export(X509ContentType.Pfx));
            }
                
        }

        public void SaveCertificateIfNotExist(X509Certificate2 x509Certificate)
        {
            //Save certificate to file
            File.WriteAllText(Path.Combine(_cryptoStoreDir, "device.crt"),
            new(PemEncoding.Write("CERTIFICATE", x509Certificate.GetRawCertData()).ToArray()));

            LoadNewCertificate();
        }

        private ECDsa GenerateKey()
        {
            ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            string fileName = "device.key";
            // Save key if not exist
            if (!File.Exists(Path.Combine(_cryptoStoreDir, fileName)))
            {
                string newKey = new(PemEncoding.Write("PRIVATE KEY", ecdsa.ExportECPrivateKey()).ToArray());
                File.WriteAllText(Path.Combine(_cryptoStoreDir, fileName), newKey);
            }
            else
            {
                string privateKey = File.ReadAllText(
                Path.Combine(_cryptoStoreDir, fileName)).ToString()
               .Replace("-----BEGIN PRIVATE KEY-----", "")
               .Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
                ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);
            }
            return ecdsa;
        }

        public void ClearKeyStore()
        {
            _certificate.Dispose();
        }

        public string GenerateCSR(string commonName)
        {
            ECDsa ecdsa = GenerateKey();

            var cert = new CertificateRequest($"O=Devices,CN={commonName}", ecdsa, HashAlgorithmName.SHA256);

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

        public X509Certificate2 GetCertificate()
        {
            return _certificate;
        }

        public MqttClientOptionsBuilderTlsParameters GetMqttClientOptionsBuilderTlsParameters()
        {
            if (_parametersFactory == null)
            {
                _parametersFactory = new AstarteMutualTLSParametersFactory(this);
            }
            return _parametersFactory.Get();
        }

        public void SetAstarteCertificate(X509Certificate2 astarteCertificate)
        {
            _certificate = astarteCertificate;
        }
    }
}
