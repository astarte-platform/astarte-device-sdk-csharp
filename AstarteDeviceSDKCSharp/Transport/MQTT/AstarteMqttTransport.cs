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
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Utilities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.ManagedClient;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace AstarteDeviceSDKCSharp.Transport.MQTT
{
    public abstract class AstarteMqttTransport : AstarteTransport
    {
        protected IManagedMqttClient? _client;
        private readonly IMqttConnectionInfo _connectionInfo;
        public bool _resendingInProgress = false;

        protected AstarteMqttTransport(AstarteProtocolType type,
        IMqttConnectionInfo connectionInfo) : base(type)
        {
            _connectionInfo = connectionInfo;
        }

        private async Task<IManagedMqttClient> InitClientAsync()
        {
            if (_client != null)
            {
                try
                {
                    await _client.StopAsync();
                }
                catch (MqttCommunicationException ex)
                {
                    throw new AstarteTransportException(ex.Message, ex);
                }
            }

            MqttFactory mqttFactory = new();
            return mqttFactory.CreateManagedMqttClient();
        }

        private async Task CompleteAstarteConnection(bool IsSessionPresent)
        {

            if (_client is not null)
            {
                _client.ApplicationMessageReceivedAsync += e =>
                {
                    OnMessageReceive(e);
                    return Task.CompletedTask;
                };

                _client.ConnectedAsync += async e =>
                {
                    await OnConnectedAsync(e);
                };

                _client.ConnectingFailedAsync += e =>
                {
                    OnConnectingFailAsync(e);
                    return Task.CompletedTask;
                };

                _client.DisconnectedAsync += OnDisconnectAsync;
                _client.ApplicationMessageProcessedAsync += OnMessageProcessedAsync;

            }

            if (!IsSessionPresent || !_introspectionSent)
            {
                await SetupSubscriptions();
                await SendIntrospection();
                await SendEmptyCacheAsync();
                await ResendAllProperties();
                _introspectionSent = true;
            }

        }

        void OnConnectingFailAsync(ConnectingFailedEventArgs args)
        {

            if (_astarteTransportEventListener != null)
            {
                _astarteTransportEventListener.OnTransportConnectionInitializationError(args.Exception);
            }
            else
            {
                Trace.WriteLine("Transport Connecting failed");
            }

        }

        async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
        {
            if (!args.ConnectResult.IsSessionPresent)
            {
                await CompleteAstarteConnection(false);
            }
            if (_astarteTransportEventListener != null)
            {
                _astarteTransportEventListener.OnTransportConnected();
            }
            else
            {
                Trace.WriteLine("Transport Connected");
            }

        }

        async Task OnMessageProcessedAsync(ApplicationMessageProcessedEventArgs eventArgs)
        {

            if (eventArgs.Exception is null)
            {
                if (_failedMessageStorage is not null)
                {
                    if (_resendingInProgress)
                    {
                        await _failedMessageStorage.MarkAsProcessed(eventArgs.ApplicationMessage.Id);
                    }
                    else
                    {
                        await _failedMessageStorage.DeleteByGuidAsync(eventArgs.ApplicationMessage.Id);
                    }
                }
            }
            else
            {
                Trace.WriteLine(eventArgs.ApplicationMessage.Id + " " + eventArgs.Exception);
            }

            await Task.CompletedTask;
        }

        public override async Task Connect()
        {

            if (_client != null)
            {
                if (_client.IsConnected)
                {
                    return;
                }
            }
            else
            {
                _client = await InitClientAsync();
            }

            var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(_connectionInfo.GetMqttConnectOptions())
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithMaxPendingMessages(10000)
                .WithPendingMessagesOverflowStrategy(
                    MQTTnet.Server.MqttPendingMessagesOverflowStrategy.DropNewMessage)
                .Build();

            if (!_client.IsStarted)
            {
                await _client.StartAsync(managedMqttClientOptions);
                await CompleteAstarteConnection(true);
            }

        }

        public override async Task Disconnect()
        {
            if (_client != null)
            {
                if (_client.IsConnected)
                {
                    await _client.StopAsync();
                }
            }
        }

        public override bool IsConnected()
        {
            if (_client == null)
            {
                return false;
            }
            return _client.IsConnected;
        }

        public IMqttConnectionInfo GetConnectionInfo()
        {
            return _connectionInfo;
        }

        private async Task SetupSubscriptions()
        {

            await _client.SubscribeAsync(_connectionInfo.GetClientId() +
            "/control/consumer/properties",
            MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

            AstarteDevice? astarteDevice = GetDevice();

            if (astarteDevice == null)
            {
                throw new AstarteTransportException("Error setting up subscriptions." +
                    " Astarte device is null");
            }

            foreach (AstarteInterface astarteInterface in astarteDevice.GetAllInterfaces())
            {
                if ((astarteInterface.GetType()
                 == typeof(AstarteServerAggregateDatastreamInterface))
                || (astarteInterface.GetType() == typeof(AstarteServerDatastreamInterface))
                || (astarteInterface.GetType() == typeof(AstarteServerPropertyInterface)))
                {
                    await _client.SubscribeAsync(_connectionInfo.GetClientId()
                     + "/" + astarteInterface.GetInterfaceName() + "/#",
                        MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                }
            }
        }

        private async Task SendEmptyCacheAsync()
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                                  .WithTopic(_connectionInfo.GetClientId() + "/control/emptyCache")
                                  .WithPayload(Encoding.ASCII.GetBytes("1"))
                                  .WithQualityOfServiceLevel(MQTTnet.Protocol
                                  .MqttQualityOfServiceLevel.ExactlyOnce)
                                  .WithRetainFlag(false)
                                  .Build();

            if (_client is null)
            {
                return;
            }

            await _client.EnqueueAsync(applicationMessage);
        }

        async Task OnDisconnectAsync(MqttClientDisconnectedEventArgs e)
        {
            if (Device is not null && _client is not null)
            {
                if (!Device.GetAlwaysReconnect())
                {
                    await _client.StopAsync();
                }

            }

            if (_astarteTransportEventListener != null)
            {
                Trace.WriteLine("The Connection was lost.");
                _astarteTransportEventListener.OnTransportDisconnected();
            }
            else
            {
                Trace.WriteLine("The Connection was lost.");
            }

        }

        private void OnMessageReceive(MqttApplicationMessageReceivedEventArgs e)
        {
            object? payload = null;

            Trace.WriteLine("Incoming message: "
                + Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? new byte[0]));

            if (!e.ApplicationMessage.Topic.Contains(_connectionInfo.GetClientId())
            || _messageListener == null)
            {
                return;
            }

            AstarteDevice? astarteDevice = GetDevice();

            if (astarteDevice == null)
            {
                throw new AstarteTransportException("Unable to receive messages." +
                    " Astarte device is null.");
            }

            string path = e.ApplicationMessage.Topic
            .Replace(_connectionInfo.GetClientId() + "/", "");

            if (path.StartsWith("control"))
            {
                if (path == "control/consumer/properties" && e.ApplicationMessage.Payload != null)
                {
                    HandlePurgeProperties(e.ApplicationMessage.Payload, astarteDevice);
                }
                else
                {
                    Trace.WriteLine("Unhandled control message!" + path);
                }
                return;
            }

            string astarteInterface = path.Split("/")[0];
            string interfacePath = path.Replace(astarteInterface, "");

            if (!astarteDevice.HasInterface(astarteInterface))
            {
                Trace.WriteLine("Got an unexpected interface!" + astarteInterface);
                return;
            }

            DateTime? timestamp = DateTime.MinValue;
            DecodedMessage? decodedMessage;
            if (e.ApplicationMessage.Payload != null && e.ApplicationMessage.Payload.Length != 0)
            {

                decodedMessage = AstartePayload.Deserialize(e.ApplicationMessage.Payload);
                if (decodedMessage is null)
                {
                    Trace.WriteLine("Unable to get payload, decodedMessage was null");
                    return;
                }
                payload = decodedMessage.GetPayload();
                timestamp = decodedMessage.GetTimestamp();

            }

            AstarteInterface? targetInterface = astarteDevice.GetInterface(astarteInterface);
            if (targetInterface is null ||
            !typeof(IAstarteServerValueBuilder).IsAssignableFrom(targetInterface.GetType()))
            {
                return;
            }

            if (!DateTime.TryParse(timestamp.ToString(), out DateTime timeStampBuilder))
            {
                timeStampBuilder = DateTime.MinValue;
            }

            IAstarteServerValueBuilder astarteServerValueBuilder =
                (IAstarteServerValueBuilder)targetInterface;
            AstarteServerValue? astarteServerValue =
                astarteServerValueBuilder.Build(interfacePath, payload, timeStampBuilder);

            if (astarteServerValue == null)
            {
                Trace.WriteLine("Unable to get value, astarteServerValue was null");
                return;
            }

            if (targetInterface.GetType() == typeof(AstarteServerPropertyInterface) && _astartePropertyStorage != null)
            {
                try
                {
                    if (astarteServerValue.GetValue() != null)
                    {
                        _astartePropertyStorage
                        .SetStoredValue(astarteInterface,
                        interfacePath,
                        astarteServerValue.GetValue(),
                        targetInterface.GetMajorVersion());
                    }
                    else
                    {
                        _astartePropertyStorage
                        .RemoveStoredPath(astarteInterface,
                        interfacePath,
                        targetInterface.GetMajorVersion());
                    }

                }
                catch (AstartePropertyStorageException ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }

            if (!typeof(IAstarteServerValuePublisher).IsAssignableFrom(targetInterface.GetType()))
            {
                return;
            }

            IAstarteServerValuePublisher astarteServerValuePublisher =
                (IAstarteServerValuePublisher)targetInterface;
            astarteServerValuePublisher.Publish(astarteServerValue);

        }

        private void HandlePurgeProperties(byte[] payload, AstarteDevice? astarteDevice)
        {
            byte[] deflated = new byte[payload.Length - 4];
            Array.Copy(payload, 4, deflated, 0, payload.Length - 4);

            MemoryStream bais = new(deflated);
            ZLibStream iis = new(bais, CompressionMode.Decompress);

            StringBuilder result = new StringBuilder();
            byte[] buf = new byte[256];
            int rlen = 0;
            byte[] bufResult;

            if (astarteDevice == null)
            {
                throw new AstarteTransportException("Unable to purge properties." +
                    " Astarte device is null");
            }

            try
            {
                while ((rlen = iis.Read(buf)) != 0)
                {
                    bufResult = new byte[rlen];
                    Array.Copy(buf, bufResult, rlen);
                    result.Append(Encoding.UTF8.GetString(bufResult));
                }
            }
            catch (IOException e)
            {
                Trace.WriteLine(e.Message);
            }

            string purgePropertiesPayload = result.ToString();
            if (_astartePropertyStorage != null)
            {
                Dictionary<AstarteInterfaceHelper, List<String>> availableProperties = new();

                foreach (AstarteInterface astarteInterface in astarteDevice.GetAllInterfaces())
                {
                    if (astarteInterface is AstarteServerPropertyInterface)
                    {
                        availableProperties.Add(new AstarteInterfaceHelper
                        (astarteInterface.GetInterfaceName(),
                        astarteInterface.GetMajorVersion()),
                        new List<string>());
                    }
                }

                String[] allProperties = purgePropertiesPayload.Split(";");
                foreach (String property in allProperties)
                {
                    String[] propertyTokens = property.Split("/", 2);
                    if (propertyTokens.Length != 2)
                    {
                        continue;
                    }

                    List<string>? pathList =
                    availableProperties.Where(e => e.Key.InterfaceName ==
                    propertyTokens[0]).Select(e => e.Value).FirstOrDefault();

                    if (pathList is null)
                    {
                        continue;
                    }
                    pathList.Add("/" + propertyTokens[1]);
                }

                try
                {
                    _astartePropertyStorage.PurgeProperties(availableProperties);
                }
                catch (AstartePropertyStorageException e)
                {
                    throw new AstarteTransportException("Unable to purge properties", e);
                }
            }
        }

    }
}
