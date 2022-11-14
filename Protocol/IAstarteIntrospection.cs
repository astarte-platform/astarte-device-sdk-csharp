using AstarteDeviceSDK.Protocol;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public interface IAstarteIntrospection
    {
        public void AddAstarteInterface(String astarteInterfaceObject);

        public void RemoveAstarteInterface();

        public AstarteInterface GetAstarteInterface(String astarteInterfaceObject);

        public List<AstarteInterface> GetAllAstarteInterfaces();

    }
}