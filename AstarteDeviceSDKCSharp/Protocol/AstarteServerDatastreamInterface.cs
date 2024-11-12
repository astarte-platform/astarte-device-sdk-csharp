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
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using System.Diagnostics;
using static AstarteDeviceSDKCSharp.Protocol.AstarteEvents.AstarteServerValue;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteServerDatastreamInterface : AstarteDatastreamInterface,
    IAstarteServerValueBuilder, IAstarteServerValuePublisher
    {

        private readonly List<IAstarteDatastreamEventListener> _listeners;

        public AstarteServerDatastreamInterface()
        {
            _listeners = new List<IAstarteDatastreamEventListener>();
        }

        public List<IAstarteDatastreamEventListener> GetAllListeners()
        {
            return _listeners;
        }

        public void AddListener(IAstarteDatastreamEventListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(IAstarteDatastreamEventListener listener)
        {
            _listeners.Remove(listener);
        }

        public AstarteServerValue? Build(string interfacePath, object? serverValue,
        DateTime timestamp)
        {
            AstarteInterfaceMapping? targetMapping = null;
            if (serverValue is null)
            {
                AstarteLogger.Error("Unable to build AstarteServerValue, astartePayload was empty", this.GetType().Name);
                return null;
            }

            AstarteServerValue? astarteServerValue =
            (new AstarteServerValueBuilder(serverValue)).Build();
            foreach (KeyValuePair<string, AstarteInterfaceMapping> entry in GetMappings())
            {
                if (AstarteInterface.IsPathCompatibleWithMapping(interfacePath, entry.Key))
                {
                    targetMapping = entry.Value;
                    break;
                }
            }
            if (targetMapping != null)
            {
                object astarteValue = serverValue;
                if (targetMapping.GetTypeMapping() == typeof(DateTime))
                {
                    astarteValue = new DateTime(((DateTime)serverValue).Ticks);
                }

                astarteServerValue = new AstarteServerValue.AstarteServerValueBuilder(astarteValue)
                                    .InterfacePath(interfacePath)
                                    .Timestamp(timestamp)
                                    .Build();
            }
            else
            {
                AstarteLogger.Warn($"Got an unexpected path {interfacePath}"
                + "for interface {GetInterfaceName()}", this.GetType().Name);

            }
            return astarteServerValue;
        }

        public void Publish(AstarteServerValue astarteServerValue)
        {
            AstarteDatastreamEvent e = new AstarteDatastreamEvent(
                GetInterfaceName(),
                astarteServerValue.GetInterfacePath(),
                astarteServerValue.GetValue(),
                astarteServerValue.GetTimestamp());

            foreach (IAstarteDatastreamEventListener listener in _listeners)
            {
                listener.ValueReceived(e);
            }
        }
    }
}
