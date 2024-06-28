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
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Utilities;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System.Diagnostics;
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
        string path, object? value, DateTime? timestamp)
        {
            AstarteInterfaceDatastreamMapping mapping = new();
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

            await DoSendMqttMessage(topic, payload, (MqttQualityOfServiceLevel)qos, mapping);

        }

        private async Task DoSendMqttMessage(string topic, byte[] payload, MqttQualityOfServiceLevel qos,
        AstarteInterfaceDatastreamMapping? mapping = null)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                                .WithTopic(topic)
                                .WithPayload(payload)
                                .WithQualityOfServiceLevel(qos)
                                .WithRetainFlag(false);

            if (mapping is not null)
            {
                applicationMessage
                .WithMessageExpiryInterval((uint)mapping.GetExpiry())
                .WithUserProperty("Retention", mapping.GetRetention().ToString());
            }

            var managedApplicationMessage = new ManagedMqttApplicationMessage
            {
                ApplicationMessage = applicationMessage.Build(),
                Id = Guid.NewGuid()
            };

            if (_client is not null)
            {
                if (_failedMessageStorage is not null)
                {
                    await _failedMessageStorage.SaveQueuedMessageAsync(managedApplicationMessage);
                }

                if (!_resendingInProgress ||
                mapping is null ||
                mapping.GetRetention() == AstarteInterfaceDatastreamMapping.MappingRetention.DISCARD)
                {
                    await _client.EnqueueAsync(managedApplicationMessage);
                }
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

            await DoSendMqttMessage(_baseTopic, Encoding.ASCII.GetBytes(introspection), MqttQualityOfServiceLevel.ExactlyOnce);
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


            await DoSendMqttMessage(topic, payload, (MqttQualityOfServiceLevel)qos, mapping);

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

        public override void StartResenderTask()
        {

            CancellationTokenSource _resenderCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _resenderCancellationTokenSource.Token;

            Task.Run(async () =>
           {
               if (!_resendingInProgress)
               {
                   await ResendFailedMessages(cancellationToken);
               }
           });
        }

        private async Task ResendFailedMessages(CancellationToken cancellationToken = default)
        {
            Trace.WriteLine("Resending stored messages in progress.");
            _resendingInProgress = true;

            if (_client == null || _failedMessageStorage == null)
            {
                Trace.WriteLine("Client or failed message storage is null.");
                _resendingInProgress = false;
                return;
            }

            try
            {
                await WaitForPendingMessagesToClear(_client);

                var storedMessages = await _failedMessageStorage.LoadQueuedMessagesAsync();
                if (storedMessages.Count == 0)
                {
                    Trace.WriteLine("No more stored messages to resend.");
                    _resendingInProgress = false;
                    return;
                }

                foreach (var message in storedMessages)
                {
                    await _client.EnqueueAsync(message);
                }

                await WaitForPendingMessagesToClear(_client);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("Resending stored messages was canceled.");
            }
            finally
            {
                _resendingInProgress = false;
                Trace.WriteLine("Resending stored messages finished.");
            }

            await ResendFailedMessages(cancellationToken);
        }

        private static async Task WaitForPendingMessagesToClear(IManagedMqttClient _client)
        {
            var timeout = _client.Options.ClientOptions.Timeout * 10000;
            await Task.Run(() => SpinWait.SpinUntil(() => _client.PendingApplicationMessagesCount == 0, timeout));

            if (_client.PendingApplicationMessagesCount > 0)
            {
                throw new AstarteTransportException("Timeout while resending stored messages.");
            }
        }

    }
}
