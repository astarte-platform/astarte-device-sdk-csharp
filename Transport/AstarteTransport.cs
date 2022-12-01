using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;

namespace AstarteDeviceSDKCSharp.Transport
{
    public abstract class AstarteTransport : IAstarteProtocol
    {
        private readonly AstarteProtocolType astarteProtocolType;

        public AstarteDevice? Device { get; set; }

        public AstarteIntrospection AstarteIntrospection { get; set; }

        protected AstarteTransport(AstarteProtocolType type)
        {
            astarteProtocolType = type;
        }

        public abstract Task SendIntrospection();
        public abstract Task SendIndividualValue(AstarteInterface astarteInterface, string path, object value, DateTime timestamp);

        public void SetDevice(AstarteDevice astarteDevice)
        {
            Device = astarteDevice;
        }
        public AstarteProtocolType GetAstarteProtocolType()
        {
            return astarteProtocolType;
        }

        public abstract void Connect();

    }
}
