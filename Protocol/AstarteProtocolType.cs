using System.ComponentModel;

namespace AstarteDeviceSDK.Protocol
{
    public enum AstarteProtocolType
    {
        [Description("")]
        UNKNOWN_PROTOCOL,
        [Description("astarte_mqtt_v1")]
        ASTARTE_MQTT_V1
    }
}