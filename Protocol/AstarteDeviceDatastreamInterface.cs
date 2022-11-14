using AstarteDeviceSDKCSharp.Transport;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteDeviceDatastreamInterface : AstarteDatastreamInterface, IAstarteDataStreamer
    {
        public void StreamData(string path, object payload)
        {
            StreamData(path, payload, new DateTime());
        }

        public void StreamData(string path, object payload, DateTime timestamp)
        {
            AstarteTransport transport = getAstarteTransport();
            if (transport == null)
            {
                 throw new AstarteTransportException("No available transport");
            }

            transport.SendIndividualValue(this, path, payload, timestamp);
        }
    }
}