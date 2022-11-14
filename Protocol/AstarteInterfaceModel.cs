using Newtonsoft.Json;

namespace AstarteDeviceSDK.Protocol
{
    public class AstarteInterfaceModel
    {
        [JsonProperty("interface_name")]
        public string InterfaceName { get; set; } = string.Empty;
        [JsonProperty("version_major")]
        public int MajorVersion { get; set; }
        [JsonProperty("version_minor")]
        public int MinorVersion { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("aggregation")]
        public string Aggregation { get; set; } = string.Empty;

        [JsonProperty("ownership")]
        public string Ownership { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("doc")]
        public string Doc { get; set; } = string.Empty;

        [JsonProperty("mappings")]
        public IList<Mapping> Mappings { get; set; } = new List<Mapping>();
    }

    public class Mapping
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("explicit_timestamp")]
        public bool? ExplicitTimestamp { get; set; } 

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("reliability")]
        public string Reliability { get; set; } = string.Empty;

        [JsonProperty("retention")]
        public string Retention { get; set; } = string.Empty;

        [JsonProperty("expiry")]
        public int? Expiry { get; set; } 

    }
}