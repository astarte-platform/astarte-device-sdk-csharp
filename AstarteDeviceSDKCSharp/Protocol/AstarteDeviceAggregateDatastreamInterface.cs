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
        public void StreamData(string path, Dictionary<string, object> payload)
        {
            StreamData(path, payload, null);
        }

        public void StreamData(string path, Dictionary<string, object> payload, DateTime? timestamp)
        {
            ValidatePayload(path, payload, timestamp);

            AstarteTransport? transport = GetAstarteTransport();
            if (transport == null)
            {
                throw new AstarteTransportException("No available transport");
            }

            transport.SendAggregate(this, path, payload, timestamp);
        }

        public void ValidatePayload(string path, Dictionary<string, Object> payload, DateTime? timestamp)
        {
            string fomattedPath = path + "/";
            Dictionary<string, AstarteInterfaceMapping> mappings = GetMappings();

            foreach (var interfaceMappingEntry in mappings)
            {
                AstarteInterfaceMapping astarteInterfaceMapping = interfaceMappingEntry.Value;

                if (astarteInterfaceMapping.Path is null)
                {
                    Trace.WriteLine("Astarte mapping path " +
                    "{" + astarteInterfaceMapping.Path + " } "
                    + " is null.");
                    continue;
                }

                if (!payload.Any(x =>
                    x.Key == astarteInterfaceMapping.Path.Substring(fomattedPath.Length)))
                {
                    throw new AstarteInvalidValueException(
                        $"Value not found for {astarteInterfaceMapping.Path}");
                }
            }

            foreach (var data in payload)
            {
                if (mappings.Any(x => x.Key == fomattedPath + data.Key))
                {
                    FindMappingInInterface(fomattedPath + data.Key)
                        .ValidatePayload(data.Value, timestamp);
                }
                else
                {
                    throw new AstarteInterfaceMappingNotFoundException(
                        $"{fomattedPath + data.Key} not found in interface");
                }
            }
        }
    }
}
