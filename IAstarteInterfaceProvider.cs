namespace AstarteDeviceSDKCSharp
{
    public interface IAstarteInterfaceProvider
    {
        List<string> LoadAllInterfaces();

        public string LoadInterface (string interfaceName);
    }
}
