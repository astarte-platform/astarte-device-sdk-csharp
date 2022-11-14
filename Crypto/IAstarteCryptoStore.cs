
using MQTTnet.Client.Options;
using System.Security.Cryptography.X509Certificates;

namespace AstarteDeviceSDKCSharp.Crypto
{
    public interface IAstarteCryptoStore
    {
        void ClearKeyStore();
        X509Certificate2 GetCertificate();
        void SetAstarteCertificate(X509Certificate2 astarteCertificate);
        string GenerateCSR(string commonName);
        MqttClientOptionsBuilderTlsParameters GetMqttClientOptionsBuilderTlsParameters();
    }
}
