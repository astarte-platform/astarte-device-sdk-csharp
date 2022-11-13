using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace AstarteDeviceSDKCSharp
{
    public class PairingHandler
    {
        public async Task ObtainDeviceCertificate(string deviceId, string realm, string credentialsSecret, string pairingBaseURL, string cryptoStoreDir)
        {

            Crypto myCrypto = new();
            // Get a CSR
            string csr = myCrypto.GenerateCsr(realm, deviceId, cryptoStoreDir);

            // Prepare the Pairing API request
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentialsSecret);
            try
            {

                var json = JsonConvert.SerializeObject(new CertificateRequest() { Data = new CsrData() { Csr = csr } });
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(pairingBaseURL + $"/v1/{realm}/devices/{deviceId}/protocols/astarte_mqtt_v1/credentials", content);

                var certificate = JsonConvert.DeserializeObject<Certificate>(await response.Content.ReadAsStringAsync());

                // Save certificate from response
                if (certificate != null)
                {
                    myCrypto.ImportDeviceCertificate(certificate.Data.ClientCrt, cryptoStoreDir);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task<string> ObtainDeviceTransportInformationAsync(string deviceId, string realm, string credentialsSecret, string pairingBaseURL)
        {
            // Prepare the Pairing API request
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentialsSecret);
            try
            {
                var response = await client.GetAsync(pairingBaseURL + $"/v1/{realm}/devices/{deviceId}");

                return await response.Content.ReadAsStringAsync();

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return "";
            }

        }

        private class Certificate
        {
            [JsonProperty("data")]
            public Data Data { get; set; } = new Data();
        }
        private class Data
        {
            [JsonProperty("client_crt")]
            public string ClientCrt { get; set; } = string.Empty;
        }
        private class CertificateRequest
        {
            [JsonProperty("data")]
            public CsrData Data { get; set; } = new CsrData();
        }
        private class CsrData
        {
            [JsonProperty("csr")]
            public string Csr { get; set; } = string.Empty;
        }



    }
}

