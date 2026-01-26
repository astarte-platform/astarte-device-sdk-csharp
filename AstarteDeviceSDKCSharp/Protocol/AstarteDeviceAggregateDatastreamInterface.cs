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
using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Transport;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteDeviceAggregateDatastreamInterface : AstarteAggregateDatastreamInterface, IAstarteAggregateDataStreamer
    {
        public async Task StreamData(string path, Dictionary<string, object> payload)
        {
            await StreamData(path, payload, null);
        }

        public async Task StreamData(string path, Dictionary<string, object> payload, DateTime? timestamp)
        {
            ValidatePayload(path, payload, timestamp);

            AstarteTransport? transport = GetAstarteTransport();
            if (transport == null)
            {
                throw new AstarteTransportException("No available transport");
            }

            await transport.SendAggregate(this, path, payload, timestamp);
        }

        public void ValidatePayload(string path, Dictionary<string, object> payload, DateTime? timestamp)
        {
            Dictionary<string, AstarteInterfaceMapping> mappings = GetMappings();

            string? firstMappingPath = mappings.Values.First().GetPath();
            string prefix = string.Empty;

            if (firstMappingPath != null)
            {
                prefix = firstMappingPath[..firstMappingPath.LastIndexOf('/')];
            }

            string[] splitMappingPath = prefix.Split('/');
            string[] splitPath = path.Split("/");

            if (splitPath.Length != splitMappingPath.Length)
            {
                throw new AstarteInterfaceMappingNotFoundException(
                      $"{path} not found in interface");
            }

            for (int i = 0; i < splitPath.Length; i++)
            {
                if (!splitPath[i].Equals(splitMappingPath[i]) &&
                    !splitMappingPath[i].StartsWith("%{"))
                {
                    throw new AstarteInterfaceMappingNotFoundException(
                        $"{path} not found in interface");
                }
            }

            string formattedPath = prefix + "/";

            foreach (var interfaceMappingEntry in mappings)
            {
                AstarteInterfaceMapping astarteInterfaceMapping = interfaceMappingEntry.Value;

                if (astarteInterfaceMapping.Path is null)
                {
                    AstarteLogger.Warn("Astarte mapping path " +
                    "{" + astarteInterfaceMapping.Path + " } "
                    + " is null.", this.GetType().Name);
                    continue;
                }
            }

            foreach (var data in payload)
            {
                string mappingKey = formattedPath + data.Key;

                if (mappings.ContainsKey(mappingKey))
                {
                    FindMappingInInterface(mappingKey)
                        .ValidatePayload(data.Value, timestamp);
                }
                else
                {
                    throw new AstarteInterfaceMappingNotFoundException(
                        $"{mappingKey} not found in interface");
                }
            }
        }
    }
}
