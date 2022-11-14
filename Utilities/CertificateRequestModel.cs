using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstarteDeviceSDKCSharp.Utilities
{
    internal class CertificateRequest
    {
        [JsonProperty("data")]
        public CsrData Data { get; set; } = new CsrData();
    }
    internal class CsrData
    {
        [JsonProperty("csr")]
        public string Csr { get; set; } = string.Empty;
    }
}
