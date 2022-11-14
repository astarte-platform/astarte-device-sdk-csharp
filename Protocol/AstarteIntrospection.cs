using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteExeption;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteIntrospection : IAstarteIntrospection
    {

        private readonly Dictionary<string, AstarteInterface> astarteInterfaces = new();

        public void AddAstarteInterface(string astarteInterfaceObject)
        {
            AstarteInterface newInterface =
        AstarteInterface.FromString(astarteInterfaceObject);

            AstarteInterface formerInterface = GetAstarteInterface(newInterface.InterfaceName);

            if (formerInterface != null
                && formerInterface.MajorVersion == newInterface.MajorVersion)
            {
                if (formerInterface.MinorVersion == newInterface.MinorVersion)
                {
                    throw new AstarteInterfaceAlreadyPresentException("Interface already present in mapping");
                }
                if (formerInterface.MinorVersion > newInterface.MinorVersion)
                {
                    throw new AstarteInvalidInterfaceException("Can't downgrade an interface at runtime");
                }
            }
            astarteInterfaces.Add(newInterface.InterfaceName, newInterface);
        }

        public List<AstarteInterface> GetAllAstarteInterfaces()
        {
            return astarteInterfaces.Values.ToList();
        }

        public AstarteInterface GetAstarteInterface(string astarteInterfaceObject)
        {
            foreach (var astarteInterface in astarteInterfaces)
            {
                if (astarteInterface.Key == astarteInterfaceObject)
                {
                    return astarteInterface.Value;
                }
            }
            return null;
        }

        public void RemoveAstarteInterface()
        {
            throw new NotImplementedException();
        }
    }
}