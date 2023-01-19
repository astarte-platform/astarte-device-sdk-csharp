namespace AstarteDeviceSDKCSharp.Transport
{
    public class AstarteTransportException : Exception
    {
        public AstarteTransportException(String message) : base(message) { }
        public AstarteTransportException(String message, Exception ex) : base(message, ex) { }
    }
}
