/*
 * This file is part of Astarte.
 *
 * Copyright 2024 SECO Mind Srl
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

using System.Text;
using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Transport.MQTT;
using AstarteDeviceSDKCSharp.Utilities;

namespace AstarteDeviceSDKCSharp.Transport.Offline
{
    public class AstarteTransportOffline : AstarteMqttV1Transport
    {
        private AstarteFailedMessageStorage _astarteFailedMessageStorage;
        private readonly string _baseTopic;

        public AstarteTransportOffline(MutualSSLAuthenticationMqttConnectionInfo connectionInfo,
                AstarteFailedMessageStorage astarteFailedMessageStorage)
         : base(connectionInfo)
        {
            _astarteFailedMessageStorage = astarteFailedMessageStorage;
            _baseTopic = connectionInfo.GetClientId();
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

            AstarteFailedMessageEntry astarteFailedMessageEntry = new(
                qos,
                payload,
                topic,
                Guid.NewGuid()
            );

            await SaveMessageToDatabase(astarteFailedMessageEntry);
        }

        public override async Task SendIndividualValue(AstarteInterface astarteInterface, string path, object? value)
        {
            await SendIndividualValue(astarteInterface, path, value, null);
        }

        public override async Task SendIndividualValue(AstarteInterface astarteInterface, string path, object? value, DateTime? timestamp)
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

            AstarteFailedMessageEntry astarteFailedMessageEntry = new(
                qos,
                payload,
                topic,
                Guid.NewGuid()
            );
            await SaveMessageToDatabase(astarteFailedMessageEntry);
        }

        private async Task SaveMessageToDatabase(AstarteFailedMessageEntry astarteFailedMessageEntry)
        {

            _astarteFailedMessageStorage?.InsertStored(astarteFailedMessageEntry.Topic,
            astarteFailedMessageEntry.Payload,
            astarteFailedMessageEntry.Qos,
            astarteFailedMessageEntry.Guid);

            if (Device is not null)
            {
                await Device.Connect();
            }

        }

        public override async Task SendIntrospection()
        {
            StringBuilder introspectionStringBuilder = new();
            AstarteDevice? astarteDevice = GetDevice();

            if (Device == null)
            {
                throw new AstarteTransportException("Error sending introspection." +
                    " Astarte device is null");
            }

            foreach (AstarteInterface astarteInterface in
            Device.GetAllInterfaces())
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

            AstarteFailedMessageEntry astarteFailedMessageEntry = new(
                0,
                Encoding.ASCII.GetBytes(introspection),
                _baseTopic,
                Guid.NewGuid()
            );

            await SaveMessageToDatabase(astarteFailedMessageEntry);
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
    }
}
