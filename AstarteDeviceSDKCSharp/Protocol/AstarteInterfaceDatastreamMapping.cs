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

using System.ComponentModel;
using AstarteDeviceSDK.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteInterfaceDatastreamMapping : AstarteInterfaceMapping
    {
        private bool explicitTimestamp;
        private MappingReliability reliability = MappingReliability.UNRELIABLE;
        private int expiry;

        public enum MappingReliability
        {
            [Description("unreliable")]
            UNRELIABLE,
            [Description("guaranteed")]
            GUARANTEED,
            [Description("unique")]
            UNIQUE
        }

        public enum MappingRetention
        {
            [Description("discard")]
            DISCARD,
            [Description("volatile")]
            VOLATILE,
            [Description("stored")]
            STORED
        }

        public MappingReliability GetReliability()
        {
            return reliability;
        }

        internal static AstarteInterfaceDatastreamMapping
        FromAstarteInterfaceMappingMaps(Mapping astarteMappingObject)
        {
            AstarteInterfaceDatastreamMapping astarteInterfaceDatastreamMapping = new();
            astarteInterfaceDatastreamMapping.ParseMappingFromAstarteInterface(astarteMappingObject);

            if (astarteMappingObject.ExplicitTimestamp != null)
            {
                astarteInterfaceDatastreamMapping.explicitTimestamp =
                (bool)astarteMappingObject.ExplicitTimestamp;
            }

            if (astarteMappingObject.Reliability != null)
            {
                if (astarteMappingObject.Reliability == "unreliable")
                {
                    astarteInterfaceDatastreamMapping.reliability = MappingReliability.UNRELIABLE;
                }
                else if (astarteMappingObject.Reliability == "guaranteed")
                {
                    astarteInterfaceDatastreamMapping.reliability = MappingReliability.GUARANTEED;
                }
                else if (astarteMappingObject.Reliability == "unique")
                {
                    astarteInterfaceDatastreamMapping.reliability = MappingReliability.UNIQUE;
                }

            }

            if (astarteMappingObject.Expiry != null)
            {
                astarteInterfaceDatastreamMapping.expiry = (int)astarteMappingObject.Expiry;
            }


            return astarteInterfaceDatastreamMapping;
        }

        public bool IsExplicitTimestamp()
        {
            return explicitTimestamp;
        }

        public override void ValidatePayload(Object payload, DateTime? timestamp)
        {
            ValidatePayload(payload);
            if (IsExplicitTimestamp() && timestamp == null)
            {
                throw new AstarteInvalidValueException(
                    "Timestamp cannot be null");
            }
        }
    }
}
