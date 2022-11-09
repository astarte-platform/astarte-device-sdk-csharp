using System.Diagnostics;
using System.Net.Http.Headers;


namespace AstarteDeviceSDKCSharp
{
    public class PairingHandler
    {
        public async Task ObtainDeviceCertificate(string deviceId, string realm, string credentialsSecret, string pairingBaseURL, string cryptoStoreDir)
        {

            var myCrypto = new Crypto();
            var csr = myCrypto.GenerateCsr(realm, deviceId, cryptoStoreDir);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentialsSecret);
            try
            {
                HttpContent content = new StringContent("{'data': {'csr': " + csr + "}}");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(pairingBaseURL + $"/v1/{realm}/devices/{deviceId}/protocols/astarte_mqtt_v1/credentials", content);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }


        }

        public async Task RegisterDevice(string deviceId, string realm, string credentialsSecret, string pairingBaseURL)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credentialsSecret);
            try
            {
                HttpContent content = new StringContent("{'data': {'hw_id': " + deviceId + "}}");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(pairingBaseURL + $"/v1/{realm}/agent/devices", content);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }



    }
}
