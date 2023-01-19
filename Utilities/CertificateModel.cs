using Newtonsoft.Json;

namespace AstarteDeviceSDKCSharp.Utilities
{
    internal class Certificate
    {
        [JsonProperty("data")]
        public Data Data { get; set; } = new Data();
    }
    internal class Data
    {
        [JsonProperty("client_crt")]
        public string ClientCrt { get; set; } = string.Empty;
    }
}
