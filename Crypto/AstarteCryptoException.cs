namespace AstarteDeviceSDKCSharp.Crypto
{
    public class AstarteCryptoException : Exception
    {
        public AstarteCryptoException(string message) : base(message) { }

        public AstarteCryptoException(string message, Exception ex): base(message, ex) { }
    }
}
