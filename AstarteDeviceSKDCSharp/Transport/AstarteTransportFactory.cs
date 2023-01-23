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
using AstarteDeviceSDKCSharp.Transport.MQTT;
using System.Diagnostics;

namespace AstarteDeviceSDKCSharp.Transport
{
    internal class AstarteTransportFactory
    {

        public static AstarteTransport? CreateAstarteTransportFromPairing(AstarteProtocolType protocolType, string astarteRealm, string deviceId, dynamic protocolData, AstarteCryptoStore astarteCryptoStore)
        {

            switch (protocolType)
            {
                case AstarteProtocolType.ASTARTE_MQTT_V1:
                    try
                    {

                        Uri brokerUrl = new((string)protocolData.Value.broker_url);
                        return new AstarteMqttV1Transport(
                            new MutualSSLAuthenticationMqttConnectionInfo(brokerUrl,
                            astarteRealm,
                            deviceId,
                            astarteCryptoStore.GetMqttClientOptionsBuilderTlsParameters())
                            );

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        return null;
                    }
                default:
                    return null;
            }
        }
    }
}
