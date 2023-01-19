using AstarteDeviceSDK.Protocol;
using MQTTnet;
using MQTTnet.Client;

namespace AstarteDeviceSDKCSharp.Transport.MQTT
{
    public abstract class AstarteMqttTransport : AstarteTransport
    {
        protected IMqttClient? _client;
        private readonly IMqttConnectionInfo _connectionInfo;
        protected AstarteMqttTransport(AstarteProtocolType type, IMqttConnectionInfo connectionInfo) : base(type)
        {
            _connectionInfo = connectionInfo;
        }

        private void InitClient()
        {
            if (_client != null)
            {
                try
                {
                    _client.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.StackTrace);
                }
            }

            try
            {
                MqttFactory mqttFactory = new();
                _client = mqttFactory.CreateMqttClient();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public override void Connect()
        {
            try
            {
                if (_client != null)
                {
                    if (_client.IsConnected)
                    {
                        return;
                    }
                }
                else
                {
                    InitClient();
                }


                var result = _client.ConnectAsync(_connectionInfo.GetMqttConnectOptions(), CancellationToken.None).Result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }

        }

        public void Disconnect()
        {
            try
            {
                if (_client.IsConnected)
                {
                    _client.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool IsConnected()
        {
            if (_client == null)
            {
                return false;
            }
            return _client.IsConnected;
        }

        public IMqttConnectionInfo GetConnectionInfo()
        {
            return _connectionInfo;
        }
    }
}
