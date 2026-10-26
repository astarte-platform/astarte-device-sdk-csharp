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

        private readonly MqttClientTlsOptions _tlsOptions;

        public AstarteMutualTLSParametersFactory(IAstarteCryptoStore cryptoStore) : base()
        {
            X509Certificate2? cert = cryptoStore.GetCertificate();
            List<X509Certificate2> certs = cert is not null ? new List<X509Certificate2> { cert } : new List<X509Certificate2>();

            _tlsOptions = new MqttClientTlsOptionsBuilder()
                .UseTls(true)
                .WithClientCertificates(certs)
                .WithIgnoreCertificateChainErrors(cryptoStore.IgnoreSSLErrors)
                .WithIgnoreCertificateRevocationErrors(cryptoStore.IgnoreSSLErrors)
                .WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12)
                .WithAllowUntrustedCertificates(cryptoStore.IgnoreSSLErrors)
                .WithCertificateValidationHandler(eventArgs =>
                {
                    return true;
                })
                .Build();
        }

        public MqttClientTlsOptions Get() => _tlsOptions;

    }
}
