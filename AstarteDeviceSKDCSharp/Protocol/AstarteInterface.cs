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

using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteExeption;
using AstarteDeviceSDKCSharp.Transport;
using Newtonsoft.Json;

namespace AstarteDeviceSDK.Protocol
{
    public abstract class AstarteInterface
    {
        public string InterfaceName { get; set; } = string.Empty;
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        private readonly Dictionary<string, AstarteInterfaceMapping> Mappings = new();

        private AstarteTransport _astarteTransport;

        public AstarteTransport GetAstarteTransport()
        {
            return _astarteTransport;
        }

        public string GetInterfaceName()
        {
            return InterfaceName;
        }

        public int GetMajorVersion()
        {
            return MajorVersion;
        }

        public int GetMinorVersion()
        {
            return MinorVersion;
        }

        public void SetAstarteTransport(AstarteTransport astarteTransport)
        {
            _astarteTransport = astarteTransport;
        }

        public static AstarteInterface FromString(string astarteInterfaceObject)
        {
            AstarteInterfaceModel astarteInterfaceModel = JsonConvert.DeserializeObject<AstarteInterfaceModel>(astarteInterfaceObject);
            string astarteInterfaceOwnership = astarteInterfaceModel.Ownership;
            string astarteInterfaceType = astarteInterfaceModel.Type;
            string astarteInterfaceAggregation;

            if (!string.IsNullOrEmpty(astarteInterfaceModel.Aggregation))
            {
                astarteInterfaceAggregation = astarteInterfaceModel.Aggregation;
            }
            else
            {
                astarteInterfaceAggregation = "individual";
            }

            bool? astarteInterfaceExplicitTimestamp;

            if (astarteInterfaceModel.Mappings.Any(x => x.ExplicitTimestamp != null))
            {
                astarteInterfaceExplicitTimestamp = astarteInterfaceModel.Mappings.Select(x => x.ExplicitTimestamp)
                                                                            .FirstOrDefault();
            }
            else
            {
                astarteInterfaceExplicitTimestamp = false;
            }

            AstarteInterface astarteInterface = null;

            if (astarteInterfaceModel.Type.Equals("datastream"))
            {
                if (astarteInterfaceAggregation.Equals("individual"))
                {
                    astarteInterface = new AstarteDeviceDatastreamInterface();
                }
            }

            astarteInterface.InterfaceName = astarteInterfaceModel.InterfaceName;
            astarteInterface.MajorVersion = astarteInterfaceModel.MajorVersion;
            astarteInterface.MinorVersion = astarteInterfaceModel.MinorVersion;

            if (astarteInterface.MajorVersion == 0 && astarteInterface.MinorVersion == 0)
            {
                throw new AstarteInvalidInterfaceException(
                        $"Both Major and Minor version are 0 on interface {astarteInterface.InterfaceName}"
                        );
            }

            foreach (var mapping in astarteInterfaceModel.Mappings)
            {
                if (Object.Equals(astarteInterfaceType, "datastream"))
                {
                    AstarteInterfaceDatastreamMapping astarteInterfaceDatastreamMapping =
                     AstarteInterfaceDatastreamMapping.FromAstarteInterfaceMappingMaps(mapping);
                    astarteInterface.Mappings.Add(
                        mapping.Endpoint,
                        astarteInterfaceDatastreamMapping);
                }
                else
                {
                    AstarteInterfaceMapping astarteInterfaceMapping = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
                    astarteInterface.Mappings.Add(
                        mapping.Endpoint,
                        astarteInterfaceMapping);
                }
            }

            return astarteInterface;
        }

        public void ValidatePayload(string path, object payload, DateTime timestamp)
        {
            FindMappingInInterface(path).ValidatePayload(payload, timestamp);
        }

        public static bool IsPathCompatibleWithMapping(string path, string mapping)
        {
            // Tokenize and handle paths, to ensure we match parametric interfaces.
            string[] mappingTokens = mapping.Split("/");
            string[] pathTokens = path.Split("/");
            if (mappingTokens.Length != pathTokens.Length)
            {
                return false;
            }

            bool matches = true;
            for (int k = 0; k < mappingTokens.Length; k++)
            {
                if (!mappingTokens[k].Contains("%{"))
                {
                    if (!Object.Equals(mappingTokens[k], pathTokens[k]))
                    {
                        matches = false;
                        break;
                    }
                }
            }

            return matches;
        }

        public AstarteInterfaceMapping FindMappingInInterface(string path)
        {

            foreach (var mappingEntry in Mappings)
            {
                if (IsPathCompatibleWithMapping(path, mappingEntry.Key))
                {
                    return mappingEntry.Value;
                }
            }

            throw new AstarteInterfaceMappingNotFoundException(
                "Mapping " + path + " not found in interface " + this);
        }

        public Dictionary<string, AstarteInterfaceMapping> GetMappings()
        {
            return Mappings;
        }

    }
}
