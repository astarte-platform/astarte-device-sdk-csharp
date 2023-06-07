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
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteServerAggregateDatastreamInterface : AstarteAggregateDatastreamInterface,
    IAstarteServerValueBuilder, IAstarteServerValuePublisher
    {
        private readonly ICollection<IAstarteAggregateDatastreamEventListener> _listeners;
        public AstarteServerAggregateDatastreamInterface()
        {
            _listeners = new HashSet<IAstarteAggregateDatastreamEventListener>();
        }

        public void AddListener(IAstarteAggregateDatastreamEventListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(IAstarteAggregateDatastreamEventListener listener)
        {
            _listeners.Remove(listener);
        }

        public AstarteServerValue? Build(string interfacePath, object? serverValue,
        DateTime timestamp)
        {
            if (serverValue is null)
            {
                Trace.WriteLine("Unable to build AstarteServerValue, serverValue was empty");
                return null;
            }

            Dictionary<string, object>? astartePayload = new();
            Dictionary<string, object> astarteAggregate = new();

            astartePayload = JsonConvert.DeserializeObject
            <Dictionary<string, object>>(JsonConvert.SerializeObject(serverValue));

            if (astartePayload is null)
            {
                Trace.WriteLine("Unable to build AstarteServerValue, astartePayload was empty");
                return null;
            }

            foreach (KeyValuePair<string, object> entry in astartePayload)
            {
                foreach (KeyValuePair<string, AstarteInterfaceMapping> m in GetMappings())
                {
                    if (AstarteInterface.IsPathCompatibleWithMapping(interfacePath + "/"
                    + entry.Key, m.Value.GetPath()))
                    {
                        if (m.Value.GetType() == typeof(DateTime))
                        {
                            astarteAggregate[entry.Key] =
                            new DateTime(((DateTime)astartePayload[entry.Key]).Ticks);
                        }
                        else
                        {
                            astarteAggregate[entry.Key] = astartePayload[entry.Key];
                        }
                    }
                }
            }

            return new AstarteServerValue.AstarteServerValueBuilder(astarteAggregate)
                .InterfacePath(interfacePath)
                .Build();
        }

        public void Publish(AstarteServerValue astarteServerValue)
        {
            AstarteAggregateDatastreamEvent e = new AstarteAggregateDatastreamEvent(
                GetInterfaceName(), astarteServerValue.GetMapValue(),
                astarteServerValue.GetTimestamp());

            foreach (IAstarteAggregateDatastreamEventListener listener in _listeners)
            {
                listener.ValueReceived(e);
            }
        }
    }
}
