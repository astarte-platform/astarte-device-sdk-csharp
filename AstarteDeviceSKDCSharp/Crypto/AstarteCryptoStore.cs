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

using MQTTnet.Client.Options;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AstarteDeviceSDKCSharp.Crypto
{
    public class AstarteCryptoStore : IAstarteCryptoStore
    {
        private X509Certificate2? _certificate;
        private AstarteMutualTLSParametersFactory? _parametersFactory;
        private readonly string _cryptoStoreDir = string.Empty;

        public AstarteCryptoStore(string cryptoStoreDir)
        {
            _cryptoStoreDir = cryptoStoreDir;
            LoadNewCertificate();

        }

        private void LoadNewCertificate()
        {

            if (File.Exists(_cryptoStoreDir + @"\device.crt"))
            {
                ECDsa ecdsa;
                ecdsa = GenerateKey();

                //Set certificate
                X509Certificate2 caCert = new(File.ReadAllBytes(_cryptoStoreDir + @"\device.crt"));
                X509Certificate2 caCertWithKey = caCert.CopyWithPrivateKey(ecdsa);
                _certificate = new(caCertWithKey.Export(X509ContentType.Pfx));
            }

        }

        public void SaveCertificateIfNotExist(X509Certificate2 x509Certificate)
        {
            //Save certificate to file
            try
            {
                File.WriteAllText(Path.Combine(_cryptoStoreDir, "device.crt"),
                new(PemEncoding.Write("CERTIFICATE", x509Certificate.GetRawCertData()).ToArray()));
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new AstarteCryptoException("Failed to write certificate. Directory not found", ex);
            }
            catch (IOException ex)
            {
                throw new AstarteCryptoException("Failed to open .crt file", ex);
            }

            LoadNewCertificate();
        }

        private ECDsa GenerateKey()
        {
            ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            string fileName = "device.key";
            // Save key if not exist
            if (!File.Exists(Path.Combine(_cryptoStoreDir, fileName)))
            {
                string newKey = new(PemEncoding.Write("PRIVATE KEY", ecdsa.ExportECPrivateKey())
                .ToArray());
                try
                {
                    File.WriteAllText(Path.Combine(_cryptoStoreDir, fileName), newKey);
                }
                catch (DirectoryNotFoundException ex)
                {
                    throw new AstarteCryptoException("Failed to write private key. Directory not found", ex);
                }
                catch (IOException ex)
                {
                    throw new AstarteCryptoException("Failed to open private key file", ex);
                }

            }
            else
            {
                string privateKey;
                try
                {
                    privateKey = File.ReadAllText(
                                    Path.Combine(_cryptoStoreDir, fileName)).ToString()
                                   .Replace("-----BEGIN PRIVATE KEY-----", "")
                                   .Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
                    try
                    {
                        ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);
                    }
                    catch (CryptographicException e)
                    {
                        throw new AstarteCryptoException("Error importing the private key", e);
                    }

                }
                catch (Exception e) when (e is DirectoryNotFoundException ||
                               e is FileNotFoundException)
                {
                    throw new AstarteCryptoException("Private key file or directory not found", e);
                }
                catch (IOException e)
                {
                    throw new AstarteCryptoException("Unable to access the private key", e);
                }

            }
            return ecdsa;
        }

        public void ClearKeyStore()
        {
            if (_certificate != null)
            {
                _certificate.Dispose();
            }
        }

        public string GenerateCSR(string commonName)
        {
            ECDsa ecdsa;
            ecdsa = GenerateKey();

            var cert = new CertificateRequest
            ($"O=Devices,CN={commonName}", ecdsa, HashAlgorithmName.SHA256);

            byte[] pkcs10 = cert.CreateSigningRequest();
            StringBuilder builder = new();

            builder.AppendLine("-----BEGIN CERTIFICATE REQUEST-----");

            string base64 = Convert.ToBase64String(pkcs10);

            int offset = 0;
            const int LineLength = 64;

            while (offset < base64.Length)
            {
                int lineEnd = Math.Min(offset + LineLength, base64.Length);
                builder.AppendLine(base64[offset..lineEnd]);
                offset = lineEnd;
            }

            builder.AppendLine("-----END CERTIFICATE REQUEST-----");
            return builder.ToString();
        }

        public X509Certificate2? GetCertificate()
        {
            return _certificate;
        }

        public MqttClientOptionsBuilderTlsParameters GetMqttClientOptionsBuilderTlsParameters()
        {
            if (_parametersFactory == null)
            {
                _parametersFactory = new AstarteMutualTLSParametersFactory(this);
            }
            return _parametersFactory.Get();
        }

        public void SetAstarteCertificate(X509Certificate2 astarteCertificate)
        {
            _certificate = astarteCertificate;
        }
    }
}
