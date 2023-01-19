using MQTTnet.Client.Options;


namespace AstarteDeviceSDKCSharp.Transport.MQTT
{
    public interface IMqttConnectionInfo
    {
        Uri GetBrokerUrl();

        string GetClientId();

        IMqttClientOptions GetMqttConnectOptions();
    }
}
