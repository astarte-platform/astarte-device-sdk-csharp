using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;

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

                var json = JsonConvert.SerializeObject(new CertificateRequest() { data = new csrData() { csr = csr } });
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(pairingBaseURL + $"/v1/{realm}/devices/{deviceId}/protocols/astarte_mqtt_v1/credentials", content);

                var certificate = JsonConvert.DeserializeObject<Certificate>(await response.Content.ReadAsStringAsync());
                
                // Save certificate from response
                if (certificate != null)
                {
                    myCrypto.ImportDeviceCertificate(certificate.data.client_crt, cryptoStoreDir);
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

        public async Task<string> RegisterDeviceWithPrivateKey(string deviceId, string realm, string privateKeyFile, string pairingBaseURL)
        {

            return await RegisterDevice(deviceId,realm, RegisterDeviceHeadersWithPrivateKey(privateKeyFile),pairingBaseURL);
        }

        public async Task<string> RegisterDeviceWithJwtToken(string deviceId, string realm, string jwtToken, string pairingBaseURL)
        {
            return await RegisterDevice(deviceId, realm, RegisterDeviceHeadersWithJwtToken(jwtToken), pairingBaseURL);
        }

        private AuthenticationHeaderValue RegisterDeviceHeadersWithPrivateKey(string privateKeyfile)
        {
            var headers = new AuthenticationHeaderValue("Bearer", GenerateToken(privateKeyfile,"pairing"));
            return headers;
        }
        private AuthenticationHeaderValue RegisterDeviceHeadersWithJwtToken(string jwtToken)
        {
            var headers = new AuthenticationHeaderValue("Bearer", jwtToken);
            return headers;
        }
        private string GenerateToken(string privateKeyFile, string type = "appengine", List<string>? authPaths = null, int expiry = 30)
        {

            try
            {
                if (authPaths == null)
                {
                    authPaths.Add(".*::.*");
                }

                // reading the content of a private key PEM file, PKCS8 encoded 
                string privateKeyPem = File.ReadAllText(privateKeyFile);

                // keeping only the payload of the key 
                privateKeyPem = privateKeyPem.Replace("-----BEGIN PRIVATE KEY-----", "");
                privateKeyPem = privateKeyPem.Replace("-----END PRIVATE KEY-----", "");

                byte[] privateKeyRaw = Convert.FromBase64String(privateKeyPem);

                // creating the RSA key 
                RSACryptoServiceProvider provider = new ();
                provider.ImportPkcs8PrivateKey(new ReadOnlySpan<byte>(privateKeyRaw), out _);
                RsaSecurityKey rsaSecurityKey = new (provider);


                List<string> realAuthPaths = new();

                if (type== "channels" && authPaths == new List<string>() {".*::.*"})
                {
                    realAuthPaths.Add("JOIN::.*");
                    realAuthPaths.Add("WATCH::.*");
                }
                else
                {
                    realAuthPaths.Add(".*::.*");
                }

                // Generating the token 
                var now = DateTime.UtcNow;

                var claims = new[] {
                    new Claim( "appengine",  "a_aea"),
                    new Claim( "realm",  "a_rma"),
                    new Claim( "housekeeping",  "a_ha"),
                    new Claim( "channels",  "a_ch"),
                    new Claim( "pairing",  "a_pa"),
                };

                var handler = new JwtSecurityTokenHandler();

                var token = new JwtSecurityToken
                (   null,
                    null,
                    claims,
                    now.AddMilliseconds(-30),
                    now.AddMinutes(60),
                    new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
                );

                // handler.WriteToken(token) returns the token!
                 return handler.WriteToken(token);

            }

            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return "";
            }
        }

        private async Task<string> RegisterDevice(string deviceId, string realm, AuthenticationHeaderValue header, string pairingBaseURL)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = header;
            try
            {
                HttpContent content = new StringContent("{'data': {'hw_id': " + deviceId + "}}");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(pairingBaseURL + $"/v1/{realm}/agent/devices", content);

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else if (response.StatusCode == HttpStatusCode.Created)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }

                dynamic credential = await response.Content.ReadAsStringAsync();

                return credential.credential_secret;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return "";
            }
        }
        public class Certificate
        {

            public Data data { get; set; } = new Data();
        }
        public class Data
        {
            public string client_crt { get; set; } = string.Empty;
        }
        public class CertificateRequest
        {
            public csrData data { get; set; } = new csrData();
        }
        public class csrData
        {
            public string csr { get; set; } = string.Empty;
        }



    }
}

