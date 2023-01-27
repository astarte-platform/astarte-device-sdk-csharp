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
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AstarteDeviceSDKCSharp
{
    public class AstartePairingService
    {
        private Uri _pairingUrl;
        private readonly string _astarteRealm;
        private readonly HttpClient _httpClient;

        public AstartePairingService(string pairingUrl, string astarteRealm)
        {
            _astarteRealm = astarteRealm;
            _pairingUrl = new Uri(pairingUrl);

            if (_pairingUrl == null)
            {
                throw new ArgumentNullException(nameof(_pairingUrl), "Pairing url is empty");
            }

            _pairingUrl = new Uri(_pairingUrl, "v1");
            _httpClient = new HttpClient();

        }


        public async Task<List<AstarteTransport>> ReloadTransports(string credentialSecret,
        AstarteCryptoStore astarteCryptoStore, string deviceId)
        {
            List<AstarteTransport> transports = new();
            // Prepare the Pairing API request
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            credentialSecret);

            try
            {

                var response = await client
                            .GetAsync(_pairingUrl + $"/{_astarteRealm}/devices/{deviceId}");

                var transportInfo = await response.Content.ReadAsStringAsync();

                if (transportInfo != null)
                {
                    dynamic? jsonInfo = JsonConvert.DeserializeObject<object>(transportInfo);

                    if (jsonInfo != null)
                    {
                        foreach (var item in jsonInfo.data.protocols)
                        {

                            AstarteProtocolType astarteProtocolType =
                                (AstarteProtocolType)System
                                .Enum.Parse(typeof(AstarteProtocolType), item.Name.ToUpper());

                            try
                            {
                                AstarteTransport supportedTransport =
                                   AstarteTransportFactory.CreateAstarteTransportFromPairing(
                                   astarteProtocolType,
                                   _astarteRealm,
                                   deviceId,
                                   item,
                                   astarteCryptoStore);

                                transports.Add(supportedTransport);
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex);
                            }
                        }

                        if (transports.Count == 0)
                        {
                            throw new AstartePairingException
                            ("Pairing did not return any supported Transport.");
                        }

                    }
                }
                return transports;

            }
            catch (Exception ex)
            {
                throw new AstartePairingException(ex.Message);
            }


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
            catch (Exception ex)
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
                catch (Exception ex)
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
                    throw new AstartePairingException(
                              "Request to Pairing API failed with "
                                  + response.StatusCode.ToString()
                                  + ". Returned body is "
                                  + response.Content.ToString());
                }

                var certificate = JsonConvert
                .DeserializeObject<Certificate>(await response.Content.ReadAsStringAsync());

                try
                {
                    X509Certificate2 newCertificate = new X509Certificate2
                    (Convert.FromBase64String(certificate.Data.ClientCrt
                     .Replace("-----BEGIN CERTIFICATE-----", "")
                     .Replace("-----END CERTIFICATE-----", "")
                     .Replace("\n", "")));

                    astarteCryptoStore.SaveCertificateIfNotExist(newCertificate);
                    return newCertificate;
                }
                catch (CryptographicException ex)
                {
                    throw new AstartePairingException("Could not generate X509 certificate", ex);
                }
            }
            catch (Exception ex)
            {
                throw new AstartePairingException("Failure in calling Pairing API", ex);
            }
        }
    }
}
