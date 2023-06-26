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
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Utilities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using MQTTnet.Exceptions;
using System.Text;
using static AstarteDeviceSDKCSharp.Protocol.AstarteInterfaceDatastreamMapping;

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
        string path, object? value, DateTime? timestamp)
        {
            AstarteInterfaceDatastreamMapping mapping;
            int qos = 2;

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
                qos = QosFromReliability(mapping);
            }

            string topic = _baseTopic + "/" + astarteInterface.InterfaceName + path;
            byte[] payload = AstartePayload.Serialize(value, timestamp);

            await DoSendMqttMessage(topic, payload, qos);
        }

        private async Task DoSendMqttMessage(string topic, byte[] payload, int qos)
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
                ($"Error publishing on MQTT. Code: {result.ReasonCode}");
            }
        }

        public override async Task SendIntrospection()
        {
            StringBuilder introspectionStringBuilder = new();
            AstarteDevice? astarteDevice = GetDevice();

            if (astarteDevice == null)
            {
                throw new AstarteTransportException("Error sending introspection." +
                    " Astarte device is null");
            }

            foreach (AstarteInterface astarteInterface in
            astarteDevice.GetAllInterfaces())
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

            await DoSendMqttMessage(_baseTopic, Encoding.ASCII.GetBytes(introspection), 2);
        }

        public override async Task SendIndividualValue(AstarteInterface astarteInterface,
        string path, object? value)
        {
            await SendIndividualValue(astarteInterface, path, value, null);
        }

        public override async Task SendAggregate(AstarteAggregateDatastreamInterface astarteInterface,
        string path, Dictionary<string, object> value, DateTime? timeStamp)
        {
            AstarteInterfaceDatastreamMapping mapping =
            (AstarteInterfaceDatastreamMapping)astarteInterface.GetMappings().Values.ToArray()[0];

            if (mapping is null)
            {
                throw new AstarteTransportException("Mapping not found");
            }

            int qos = QosFromReliability(mapping);

            string topic = _baseTopic + "/" + astarteInterface.InterfaceName + path;
            byte[] payload = AstartePayload.Serialize(value, timeStamp);

            try
            {
                await DoSendMqttMessage(topic, payload, qos);
            }
            catch (MqttCommunicationException e)
            {

                HandleDatastreamFailedPublish(e, mapping, topic, payload, qos);
            }
        }

        public override void RetryFailedMessages()
        {
            if (_failedMessageStorage is null)
            {
                return;
            }

            while (!_failedMessageStorage.IsEmpty())
            {
                IAstarteFailedMessage? failedMessage = _failedMessageStorage.PeekFirst();
                if (failedMessage is null)
                {
                    return;
                }

                if (failedMessage.IsExpired())
                {
                    // No need to send this anymore, drop it
                    _failedMessageStorage.RejectFirst();
                    continue;
                }

                try
                {
                    Task.Run(async () => await DoSendMessage(failedMessage));
                }
                catch (MqttCommunicationException e)
                {
                    throw new AstarteTransportException(e.Message);
                }
                _failedMessageStorage.AckFirst();
            }
        }

        private async Task DoSendMessage(IAstarteFailedMessage failedMessage)
        {
            await DoSendMqttMessage(failedMessage.GetTopic(), failedMessage.GetPayload(),
            failedMessage.GetQos());
        }

        private int QosFromReliability(AstarteInterfaceDatastreamMapping mapping)
        {
            switch (mapping.GetReliability())
            {
                case AstarteInterfaceDatastreamMapping.MappingReliability.UNIQUE:
                    return 2;
                case AstarteInterfaceDatastreamMapping.MappingReliability.GUARANTEED:
                    return 1;
                case AstarteInterfaceDatastreamMapping.MappingReliability.UNRELIABLE:
                    return 0;
                default:
                    return 0;
            }
        }

        public override async Task ResendAllProperties()
        {

            if (_astartePropertyStorage == null)
            {
                return;
            }

            AstarteDevice? astarteDevice = GetDevice() ??
                throw new AstarteTransportException("Error sending properties." +
                " Astarte device is null");

            foreach (AstarteInterface astarteInterface in astarteDevice.GetAllInterfaces())
            {
                if (astarteInterface is AstarteDevicePropertyInterface)
                {
                    Dictionary<string, object> storedPaths;
                    try
                    {
                        storedPaths = _astartePropertyStorage
                            .GetStoredValuesForInterface(astarteInterface);
                    }
                    catch (AstartePropertyStorageException e)
                    {
                        throw new AstarteTransportException("Error sending properties", e);
                    }

                    if (storedPaths != null)
                    {
                        foreach (var entry in storedPaths)
                        {
                            await SendIndividualValue(astarteInterface, entry.Key, entry.Value);
                        }
                    }
                }
            }
        }
        private void HandlePropertiesFailedPublish(MqttCommunicationException e, string topic,
        byte[] payload, int qos)
        {
            if (_failedMessageStorage is null)
            {
                return;
            }

            // Properties are always stored and never expire
            _failedMessageStorage.InsertStored(topic, payload, qos);
        }

        private async void DoSendMqttMessage(IAstarteFailedMessage failedMessage)
        {
            await DoSendMqttMessage(failedMessage.GetTopic(),
            failedMessage.GetPayload(),
            failedMessage.GetQos());
        }

        private void HandleDatastreamFailedPublish(MqttCommunicationException e,
        AstarteInterfaceDatastreamMapping mapping, string topic, byte[] payload, int qos)
        {
            int expiry = mapping.GetExpiry();
            switch (mapping.GetRetention())
            {
                case MappingRetention.DISCARD:
                    throw new AstarteTransportException("Cannot send value", e);

                case MappingRetention.VOLATILE:
                    {
                        if (expiry > 0)
                        {
                            _failedMessageStorage?.InsertVolatile(topic, payload, qos, expiry);
                        }
                        else
                        {
                            _failedMessageStorage?.InsertVolatile(topic, payload, qos);
                        }
                        break;
                    }

                case MappingRetention.STORED:
                    {
                        if (expiry > 0)
                        {
                            _failedMessageStorage?.InsertStored(topic, payload, qos, expiry);
                        }
                        else
                        {
                            _failedMessageStorage?.InsertStored(topic, payload, qos);
                        }
                        break;
                    }
                default:
                    throw new AstarteTransportException("Invalid retention value", e);
            }
        }

    }
}
