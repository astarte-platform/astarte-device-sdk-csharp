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
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteServerPropertyInterface : AstartePropertyInterface,
    IAstarteServerValueBuilder, IAstarteServerValuePublisher
    {
        private readonly IAstartePropertyStorage _propertyStorage;
        private readonly List<IAstartePropertyEventListener> _listeners;

        public AstarteServerPropertyInterface(IAstartePropertyStorage propertyStorage)
        : base(propertyStorage)
        {
            _propertyStorage = propertyStorage;
            _listeners = new();
        }

        public void AddListener(IAstartePropertyEventListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(IAstartePropertyEventListener listener)
        {
            _listeners.Remove(listener);
        }

        public AstarteServerValue? Build(string interfacePath, object? serverValue,
        DateTime timestamp)
        {
            AstarteServerValue? astarteServerValue = null;
            foreach (var entry in GetMappings())
            {
                if (AstarteInterface.IsPathCompatibleWithMapping(interfacePath, entry.Key))
                {
                    AstarteInterfaceMapping targetMapping = entry.Value;
                    object? astarteValue = serverValue;
                    if (targetMapping.GetType() == typeof(DateTime))
                    {
                        astarteValue = Convert.ToDateTime(serverValue);
                    }
                    astarteServerValue =
                            new AstarteServerValue.AstarteServerValueBuilder(astarteValue)
                           .InterfacePath(interfacePath)
                           .Build();
                    break;
                }
            }
            return astarteServerValue;
        }

        public void Publish(AstarteServerValue astarteServerValue)
        {
            AstartePropertyEvent e = new AstartePropertyEvent(
               GetInterfaceName(),
               astarteServerValue.GetInterfacePath(),
               astarteServerValue.GetValue());

            if (astarteServerValue.GetValue() == null)
            {
                foreach (IAstartePropertyEventListener listener in _listeners)
                {
                    listener.PropertyUnset(e);
                }
            }
            else
            {
                foreach (IAstartePropertyEventListener listener in _listeners)
                {
                    listener.PropertyReceived(e);
                }
            }
        }
    }
}
