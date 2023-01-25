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
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteExeption;
using AstarteDeviceSDKCSharp.Utilities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using System.Text;

namespace AstarteDeviceSDKCSharp.Transport.MQTT
{
    public class AstarteMqttV1Transport : AstarteMqttTransport
    {
        private readonly string _baseTopic;
        public AstarteMqttV1Transport(MutualSSLAuthenticationMqttConnectionInfo connectionInfo)
        : base(AstarteProtocolType.ASTARTE_MQTT_V1, connectionInfo)
        {
            _baseTopic = connectionInfo.GetClientId();

        }

        public override async Task SendIndividualValue(AstarteInterface astarteInterface,
        string path, object value, DateTime? timestamp)
        {
            int qos;
            AstarteInterfaceDatastreamMapping mapping;

            if (astarteInterface.GetType() == (typeof(AstarteDeviceDatastreamInterface)))
            {
                try
                {
                    // Find a matching mapping
                    mapping = (AstarteInterfaceDatastreamMapping)astarteInterface
                    .FindMappingInInterface(path);
                }
                catch (AstarteInterfaceMappingNotFoundException e)
                {
                    throw new AstarteTransportException("Mapping not found", e);
                }
                qos = 0;

                string topic = _baseTopic + "/" + astarteInterface.InterfaceName + path;
                byte[] payload = AstartePayload.Serialize(value, timestamp);

                try
                {
                    await DoSendMqttMessage(topic, payload, qos);
                }
                catch (Exception ex)
                {
                    if (astarteInterface.GetType()
                    .IsInstanceOfType(typeof(AstarteDatastreamInterface)))
                    {
                        throw new AstarteTransportException("Mapping not found", ex);
                    }
                    else
                    {
                        throw new AstarteTransportException("Mapping not found", ex);
                    }
                }
            }
        }

        private async Task DoSendMqttMessage(string topic, byte[] payload, int qos)
        {
            try
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                                    .WithTopic(topic)
                                    .WithPayload(payload)
                                    .WithQualityOfServiceLevel(qos)
                                    .WithRetainFlag(false)
                                    .Build();

                MqttClientPublishResult result = await _client.PublishAsync(applicationMessage);

                if (result.ReasonCode != MqttClientPublishReasonCode.Success)
                {
                    throw new AstarteTransportException
                    ($"MQTT raised an exception with a reason code: {result.ReasonCode}");
                }
            }
            catch (Exception ex)
            {
                //we cannot implement error handling at the moment 
                throw new AstarteTransportException(ex.Message, ex);
            }
        }

        public override async Task SendIntrospection()
        {
            StringBuilder introspectionStringBuilder = new();

            foreach (AstarteInterface astarteInterface in
            AstarteIntrospection.GetAllAstarteInterfaces())
            {
                introspectionStringBuilder.Append(astarteInterface.InterfaceName);
                introspectionStringBuilder.Append(':');
                introspectionStringBuilder.Append(astarteInterface.MajorVersion);
                introspectionStringBuilder.Append(':');
                introspectionStringBuilder.Append(astarteInterface.MinorVersion);
                introspectionStringBuilder.Append(';');
            }

            // Remove last ;
            introspectionStringBuilder = introspectionStringBuilder
            .Remove(introspectionStringBuilder.Length - 1, 1);
            string introspection = introspectionStringBuilder.ToString();

            try
            {
                await DoSendMqttMessage(_baseTopic, Encoding.ASCII.GetBytes(introspection), 2);
            }
            catch (Exception ex)
            {
                throw new AstarteTransportException(ex.Message, ex);
            }
        }

        public override async Task SendIndividualValue(AstarteInterface astarteInterface,
        string path, object value)
        {
            await SendIndividualValue(astarteInterface, path, value, null);
        }
    }
}
