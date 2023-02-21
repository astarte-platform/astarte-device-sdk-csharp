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
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Exceptions;

namespace AstarteDeviceSDKCSharp.Transport.MQTT
{
    public abstract class AstarteMqttTransport : AstarteTransport
    {
        protected IMqttClient? _client;
        private readonly IMqttConnectionInfo _connectionInfo;
        protected AstarteMqttTransport(AstarteProtocolType type,
        IMqttConnectionInfo connectionInfo) : base(type)
        {
            _connectionInfo = connectionInfo;
        }

        private void InitClient()
        {
            if (_client != null)
            {
                try
                {
                    _client.DisconnectAsync();
                }
                catch (MqttCommunicationException ex)
                {
                    throw new AstarteTransportException(ex.Message, ex);
                }
            }

            MqttFactory mqttFactory = new();
            _client = mqttFactory.CreateMqttClient();
        }

        public override void Connect()
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
                InitClient();
            }

            if (_client != null)
            {
                var result = _client.ConnectAsync(_connectionInfo.GetMqttConnectOptions(),
                        CancellationToken.None).Result;

                if (result.ResultCode != MqttClientConnectResultCode.Success)
                {
                    throw new AstarteTransportException
                    ($"Error connecting to MQTT. Code: {result.ResultCode}");
                }
            }

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
    }
}
