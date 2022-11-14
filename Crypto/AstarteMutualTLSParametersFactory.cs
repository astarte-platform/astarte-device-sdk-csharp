using MQTTnet.Client.Options;
using System.Security.Cryptography.X509Certificates;

namespace AstarteDeviceSDKCSharp.Crypto
{
    public class AstarteMutualTLSParametersFactory : MqttClientOptionsBuilderTlsParameters
    {

        private readonly MqttClientOptionsBuilderTlsParameters _tlsOptions;

        public AstarteMutualTLSParametersFactory(IAstarteCryptoStore cryptoStore):base()
        {
            _tlsOptions = new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                Certificates = new List<X509Certificate>
                    {
                      cryptoStore.GetCertificate()
                    },
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                AllowUntrustedCertificates = true
            };
        }

        public MqttClientOptionsBuilderTlsParameters Get() => _tlsOptions;

    }
}
