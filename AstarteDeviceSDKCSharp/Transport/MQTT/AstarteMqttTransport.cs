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

using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Utilities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Publishing;
using MQTTnet.Exceptions;

namespace AstarteDeviceSDKCSharp.Transport.MQTT
{
    public abstract class AstarteMqttTransport : AstarteTransport
    {
        protected IMqttClient? _client;
        private readonly IMqttConnectionInfo _connectionInfo;
        private readonly IAstartePropertyStorage _astartePropertyStorage;
        protected AstarteMqttTransport(AstarteProtocolType type,
        IMqttConnectionInfo connectionInfo) : base(type)
        {
            _connectionInfo = connectionInfo;
            _astartePropertyStorage = new AstartePropertyStorage();
        }

        private async Task<IMqttClient> InitClientAsync()
        {
            if (_client != null)
            {
                try
                {
                    await _client.DisconnectAsync();
                }
                catch (MqttCommunicationException ex)
                {
                    throw new AstarteTransportException(ex.Message, ex);
                }
            }

            MqttFactory mqttFactory = new();
            return mqttFactory.CreateMqttClient();
        }

        private async Task CompleteAstarteConnection()
        {
            if (!_introspectionSent)
            {
                await SetupSubscriptions();
                await SendIntrospection();
                await SendEmptyCacheAsync();
                _introspectionSent = true;
            }

            if (_astarteTransportEventListener != null)
            {
                _astarteTransportEventListener.OnTransportConnected();
            }
            else
            {
                Trace.WriteLine("Transport Connected");
            }

            _client.UseApplicationMessageReceivedHandler(OnMessageReceive);
            _client.UseDisconnectedHandler(OnDisconnectAsync);

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

            var result = await _client.ConnectAsync(_connectionInfo.GetMqttConnectOptions(),
                    CancellationToken.None);

            await CompleteAstarteConnection();

        }

        public override void Disconnect()
        {
            if (_client != null)
            {
                if (_client.IsConnected)
                {
                    _client.DisconnectAsync();
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

            MqttClientPublishResult result = await _client.PublishAsync(applicationMessage);
            if (result.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                throw new AstarteTransportException($"Error publishing on MQTT. Code: " +
                "{" + result.ReasonCode + "}");
            }
        }

        private Task OnDisconnectAsync(MqttClientDisconnectedEventArgs e)
        {
            if (_astarteTransportEventListener != null)
            {
                _astarteTransportEventListener.OnTransportDisconnected();
            }
            else
            {
                Trace.WriteLine("The Connection was lost.");
            }

            return Task.CompletedTask;
        }

        private void OnMessageReceive(MqttApplicationMessageReceivedEventArgs e)
        {
            Trace.WriteLine("Incoming message: "
            + Encoding.UTF8.GetString(e.ApplicationMessage.Payload));

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
                if (path == "control/consumer/properties")
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

            object? payload;
            DateTime? timestamp = DateTime.MinValue;
            DecodedMessage? decodedMessage;
            if (e.ApplicationMessage.Payload.Length == 0)
            {
                payload = null;
            }
            else
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

            if (targetInterface.GetType() == typeof(AstarteServerPropertyInterface))
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
