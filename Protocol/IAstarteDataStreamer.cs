namespace AstarteDeviceSDKCSharp.Protocol
{
    public interface IAstarteDataStreamer
    {
        void StreamData(String path, Object payload);

        void StreamData(String path, Object payload, DateTime timestamp);
    }
}