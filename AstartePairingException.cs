namespace AstarteDeviceSDKCSharp
{
    public class AstartePairingException: Exception
    {
        public AstartePairingException(string message) : base(message) { }

        public AstartePairingException(string message, Exception ex) : base(message, ex) { }
    }
}
