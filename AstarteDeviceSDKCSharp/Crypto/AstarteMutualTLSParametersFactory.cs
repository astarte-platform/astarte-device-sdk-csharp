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

using MQTTnet.Client;
using System.Security.Cryptography.X509Certificates;

namespace AstarteDeviceSDKCSharp.Crypto
{
    public class AstarteMutualTLSParametersFactory
    {

        private readonly MqttClientOptionsBuilderTlsParameters _tlsOptions;

        public AstarteMutualTLSParametersFactory(IAstarteCryptoStore cryptoStore) : base()
        {
            _tlsOptions = new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                Certificates = new List<X509Certificate2?>
                {
                    cryptoStore.GetCertificate()
                },
                IgnoreCertificateChainErrors = cryptoStore.IgnoreSSLErrors,
                IgnoreCertificateRevocationErrors = cryptoStore.IgnoreSSLErrors,
                SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                AllowUntrustedCertificates = cryptoStore.IgnoreSSLErrors,
                CertificateValidationHandler = eventArgs =>
                {
                    eventArgs.Certificate = cryptoStore.GetCertificate();
                    return true;
                }
            };
        }

        public MqttClientOptionsBuilderTlsParameters Get() => _tlsOptions;

    }
}
