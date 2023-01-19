using AstarteDeviceSDKCSharp.Crypto;
using AstarteDeviceSDKCSharp.Transport;
using System.Security.Cryptography.X509Certificates;

namespace AstarteDeviceSDKCSharp
{
    public class AstartePairingHandler
    {
        private readonly AstartePairingService _AstartePairingService;
        private readonly string _astarteRealm;
        private readonly string _deviceId;
        private readonly string _credentialSecret;
        readonly AstarteCryptoStore _cryptoStore;
        private List<AstarteTransport> _transports;
        private X509Certificate2 _certificate;

        public AstartePairingHandler(string pairingUrl, string astarteRealm, string deviceId, string credentialSecret, AstarteCryptoStore astarteCryptoStore)
        {
            _astarteRealm = astarteRealm;
            _deviceId = deviceId;
            _credentialSecret = credentialSecret;
            _cryptoStore = astarteCryptoStore;
            _AstartePairingService = new AstartePairingService(pairingUrl, astarteRealm);

            _certificate = _cryptoStore.GetCertificate();
            if (_certificate == null)
            {
                _ = _AstartePairingService.RequestNewCertificate(credentialSecret, _cryptoStore, deviceId).Result;
            }

        }

        public void Init()
        {
            ReloadTransports();
        }

        private void ReloadTransports()
        {
            _transports = _AstartePairingService.ReloadTransports(_credentialSecret, _cryptoStore, _deviceId).Result;
        }

        public List<AstarteTransport> GetTransports()
        {
            return _transports;
        }

    }
}
