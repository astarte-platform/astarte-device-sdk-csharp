using MQTTnet.Client.Options;

namespace AstarteDeviceSDKCSharp.Transport.MQTT
{
    public class MutualSSLAuthenticationMqttConnectionInfo : IMqttConnectionInfo
    {

        private readonly Uri _brokerUrl;
        private readonly IMqttClientOptions _mqttConnectOptions;
        private readonly string _clientId = string.Empty;

        public MutualSSLAuthenticationMqttConnectionInfo(Uri brokerUrl, string astarteRealm, string deviceId, MqttClientOptionsBuilderTlsParameters tlsOptions)
        {
            _brokerUrl = brokerUrl;
            _mqttConnectOptions = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer(_brokerUrl.Host, _brokerUrl.Port)
            .WithTls(tlsOptions)
            .WithCleanSession(false)
            .WithCommunicationTimeout(TimeSpan.FromSeconds(60))
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
            .Build();

            _clientId = $"{astarteRealm}/{deviceId}";
        }

        public Uri GetBrokerUrl() => _brokerUrl;

        public string GetClientId() => _clientId;

        public IMqttClientOptions GetMqttConnectOptions() => _mqttConnectOptions;
    }
}
