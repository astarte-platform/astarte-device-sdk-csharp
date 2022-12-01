using AstarteDeviceSDKCSharp.Protocol;

namespace AstarteDeviceSDK.Protocol
{
    public interface IAstarteProtocol
    {
        Task SendIntrospection();
        Task SendIndividualValue(AstarteInterface astarteInterface, String path, Object value, DateTime timestamp);
    }
}
