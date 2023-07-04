/*
 * This file is part of Astarte.
 *
 * Copyright 2023 SECO Mind Srl
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 */

using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Crypto;
using AstarteDeviceSDKCSharp.Transport;
using AstarteDeviceSDKCSharp.Utilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AstarteDeviceSDKCSharp
{
    public class AstartePairingService
    {
        private readonly Uri _pairingUrl;
        private readonly string _astarteRealm;
        private readonly HttpClient _httpClient;

        public AstartePairingService(string pairingUrl, string astarteRealm)
        {
            _astarteRealm = astarteRealm;
            _pairingUrl = new Uri($"{pairingUrl.TrimEnd('/')}/v1");
            _httpClient = new HttpClient();

        }

        internal async Task<List<AstarteTransport>> ReloadTransports(string credentialSecret,
        AstarteCryptoStore astarteCryptoStore, string deviceId)
        {
            List<AstarteTransport> transports = new();
            // Prepare the Pairing API request
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            credentialSecret);

            var response = await client
                        .GetAsync(_pairingUrl + $"/{_astarteRealm}/devices/{deviceId}");

            if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                responseContent = responseContent.IsNullOrEmpty() ? "empty" : responseContent;

                throw new AstartePairingException(
                            "Request to Pairing API failed with "
                                + response.StatusCode.ToString()
                                + ". Returned body is "
                                + responseContent);
            }

            var transportInfo = await response.Content.ReadAsStringAsync();

            if (transportInfo != null)
            {

                dynamic? jsonInfo;
                try
                {
                    jsonInfo = JsonConvert.DeserializeObject<object>(transportInfo);
                }
                catch (JsonException ex)
                {
                    throw new AstartePairingException(ex.Message, ex);
                }

                if (jsonInfo != null)
                {
                    foreach (var item in jsonInfo.data.protocols)
                    {

                        AstarteProtocolType astarteProtocolType =
                            (AstarteProtocolType)System
                            .Enum.Parse(typeof(AstarteProtocolType), item.Name.ToUpper());

                        AstarteTransport supportedTransport =
                           AstarteTransportFactory.CreateAstarteTransportFromPairing(
                           astarteProtocolType,
                           _astarteRealm,
                           deviceId,
                           item,
                           astarteCryptoStore);

                        transports.Add(supportedTransport);
                    }

                    if (transports.Count == 0)
                    {
                        throw new AstartePairingException
                        ("Error in getting a valid transport from pairing");
                    }

                }
            }
            return transports;
        }

        public async Task<X509Certificate2> RequestNewCertificate
        (string credentialSecret, AstarteCryptoStore astarteCryptoStore, string deviceId)
        {
            string csr;
            // Get a CSR
            try
            {
                csr = astarteCryptoStore.GenerateCSR(_astarteRealm + "/" + deviceId);
            }
            catch (AstarteCryptoException ex)
            {
                throw new AstartePairingException("Could not generate a CSR", ex);
            }

            // Prepare the Pairing API request
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", credentialSecret);

            try
            {

                string json;
                try
                {
                    json = JsonConvert.SerializeObject
                    (new Utilities.CertificateRequest() { Data = new CsrData() { Csr = csr } });

                }
                catch (JsonException ex)
                {
                    throw new AstartePairingException
                    ("Could not generate the JSON Request Payload", ex);
                }

                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(_pairingUrl +
                $"/{_astarteRealm}/devices/{deviceId}/protocols/astarte_mqtt_v1/credentials",
                content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    responseContent = responseContent.IsNullOrEmpty() ? "empty" : responseContent;

                    throw new AstartePairingException(
                              "Request to Pairing API failed with "
                                  + response.StatusCode.ToString()
                                  + ". Returned body is "
                                  + responseContent);
                }

                var certificate = JsonConvert
                .DeserializeObject<Certificate>(await response.Content.ReadAsStringAsync());

                try
                {
                    if (certificate != null)
                    {
                        X509Certificate2 newCertificate = new X509Certificate2
                        (Convert.FromBase64String(certificate.Data.ClientCrt
                        .Replace("-----BEGIN CERTIFICATE-----", "")
                        .Replace("-----END CERTIFICATE-----", "")
                        .Replace("\n", "")));

                        astarteCryptoStore.SaveCertificateIfNotExist(newCertificate);
                        return newCertificate;
                    }
                    else
                    {
                        throw new AstartePairingException("Certificate is null");
                    }
                }
                catch (CryptographicException ex)
                {
                    throw new AstartePairingException("Could not generate X509 certificate", ex);
                }
            }
            catch (AstarteCryptoException ex)
            {
                throw new AstartePairingException("Failure in calling Pairing API", ex);
            }
        }

        /// <summary>
        /// Registers a Device against an Astarte instance/realm with a Private Key
        /// </summary>
        /// <param name="deviceId">The Device ID to register.</param>
        /// <param name="privateKeyFile">Path to the Private Key file for the Realm.
        /// It will be used to Authenticate against Pairing API.</param>
        /// <returns>Returns the Credentials secret for the Device</returns>
        public async Task<string> RegisterDeviceWithPrivateKey(string deviceId, string privateKeyFile)
        {
            return await RegisterDevice(deviceId, RegisterDeviceHeadersWithPrivateKey(privateKeyFile));
        }
        /// <summary>
        /// Registers a Device against an Astarte instance/realm with a JWT Token
        /// </summary>
        /// <param name="deviceId">The Device ID to register.</param>
        /// <param name="jwtToken">A JWT Token to Authenticate against Pairing API. 
        /// The token must have access to Pairing API and to the agent API paths.</param>
        /// <returns>Returns the Credentials secret for the Device</returns>
        public async Task<string> RegisterDeviceWithJwtToken(string deviceId, string jwtToken)
        {
            return await RegisterDevice(deviceId, RegisterDeviceHeadersWithJwtToken(jwtToken));
        }

        private AuthenticationHeaderValue RegisterDeviceHeadersWithPrivateKey(string privateKeyfile)
        {
            return new AuthenticationHeaderValue("Bearer", GenerateToken(privateKeyfile));
        }

        private AuthenticationHeaderValue RegisterDeviceHeadersWithJwtToken(string jwtToken)
        {
            return new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        private string GenerateToken(string privateKeyFile, int expiry = 30)
        {
            List<string> authPaths = new();
            authPaths.Add(".*::.*");

            string privateKey;
            try
            {
                privateKey = File.ReadAllText(
                privateKeyFile).ToString()
                .Replace("-----BEGIN EC PRIVATE KEY-----", "")
                .Replace("-----END EC PRIVATE KEY-----", "")
                .Replace("\n", "");
            }
            catch (DirectoryNotFoundException e)
            {
                throw new AstartePairingException("Private key file or directory not found", e);
            }
            catch (FileNotFoundException e)
            {
                throw new AstartePairingException("Private key file not found", e);
            }
            catch (IOException e)
            {
                throw new AstartePairingException("Unable to access the private key", e);
            }

            // creating the key 
            ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            try
            {
                ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);
            }
            catch (CryptographicException e)
            {
                throw new AstartePairingException("Error importing the private key", e);
            }

            ECDsaSecurityKey rsaSecurityKey = new(ecdsa);

            List<string> realAuthPaths = new();
            realAuthPaths.Add("JOIN::.*");
            realAuthPaths.Add("WATCH::.*");

            // Generating the token 
            var now = DateTime.UtcNow;
            var startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            JArray jRealAuthPaths = new(realAuthPaths);

            var claims = new[] {
                    new Claim( "iat", ((long)(now - startDate).TotalSeconds).ToString(), ClaimValueTypes.Integer64),
                    new Claim( "a_pa",jRealAuthPaths.ToString(),JsonClaimValueTypes.JsonArray),
                };

            var handler = new JwtSecurityTokenHandler();

            JwtSecurityToken token;
            try
            {
                token = new JwtSecurityToken(
                                   null,
                                   null,
                                   claims,
                                   null,
                                   now.AddSeconds(expiry),
                                   new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.EcdsaSha256)
                               );
            }
            catch (ArgumentException e)
            {
                throw new AstartePairingException("Token parameters error", e);
            }

            try
            {
                return handler.WriteToken(token);
            }
            catch (ArgumentNullException e)
            {
                throw new AstartePairingException("Token is null", e);
            }
            catch (ArgumentException e)
            {
                throw new AstartePairingException("Invalid token", e);
            }
            catch (SecurityTokenEncryptionFailedException e)
            {
                throw new AstartePairingException("Token serialization failed", e);
            }

        }

        private async Task<string> RegisterDevice(string deviceId, AuthenticationHeaderValue header)
        {
            string credentialSecret = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = header;

            JObject payLoad;
            try
            {
                payLoad = new(
                    new JProperty("data",
                      new JObject(
                        new JProperty("hw_id", deviceId)))
                );
            }
            catch (JsonException ex)
            {
                throw new AstartePairingException("Could not generate the JSON Request Payload", ex);
            }

            HttpContent content = new StringContent(payLoad.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(_pairingUrl + $"/{_astarteRealm}/agent/devices", content);

            if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                responseContent = responseContent.IsNullOrEmpty() ? "empty" : responseContent;

                throw new AstartePairingException(
                    "Request to device register API failed with "
                        + response.StatusCode
                        + ". Returned body is "
                        + responseContent);
            }

            dynamic? credential = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync());
            if (credential != null)
            {
                credentialSecret = credential.data.credentials_secret;
            }

            if (string.IsNullOrEmpty(credentialSecret))
            {
                throw new AstartePairingException("Failed to call the device registration API");
            }
            return credentialSecret;
        }

        public Uri PairingUrl()
        {
            return _pairingUrl;
        }
    }
}
