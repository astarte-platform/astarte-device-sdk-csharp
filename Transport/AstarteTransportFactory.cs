using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Crypto;
using AstarteDeviceSDKCSharp.Transport.MQTT;
using System.Diagnostics;

namespace AstarteDeviceSDKCSharp.Transport
{
    internal class AstarteTransportFactory
    {

        public static AstarteTransport? CreateAstarteTransportFromPairing(AstarteProtocolType protocolType, string astarteRealm, string deviceId, dynamic protocolData, AstarteCryptoStore astarteCryptoStore)
        {

            switch (protocolType)
            {
                case AstarteProtocolType.ASTARTE_MQTT_V1:
                    try
                    {

                        Uri brokerUrl = new((string)protocolData.Value.broker_url);
                        return new AstarteMqttV1Transport(
                            new MutualSSLAuthenticationMqttConnectionInfo(brokerUrl,
                            astarteRealm,
                            deviceId,
                            astarteCryptoStore.GetMqttClientOptionsBuilderTlsParameters())
                            );

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        return null;
                    }
                default:
                    return null;
            }
        }
    }
}
