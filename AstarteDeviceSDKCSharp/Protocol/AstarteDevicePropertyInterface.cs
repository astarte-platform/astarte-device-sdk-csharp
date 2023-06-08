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
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Transport;
using AstarteDeviceSDKCSharp.Utilities;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteDevicePropertyInterface : AstartePropertyInterface, IAstartePropertySetter
    {
        private readonly IAstartePropertyStorage propertyStorage;

        public AstarteDevicePropertyInterface(IAstartePropertyStorage propertyStorage)
        : base(propertyStorage)
        {
            this.propertyStorage = propertyStorage;
        }

        public void SetProperty(string path, object payload)
        {
            ValidatePayload(path, payload, null);

            AstarteTransport? transport = GetAstarteTransport();

            if (transport == null)
            {
                throw new AstarteTransportException("No available transport");
            }

            DecodedMessage? storedValue = null!;

            try
            {
                storedValue = propertyStorage.GetStoredValue(this, path, this.GetMajorVersion());
            }
            catch (AstartePropertyStorageException e)
            {
                Trace.WriteLine(e.Message);
            }

            if (storedValue == null)
            {
                try
                {
                    propertyStorage.SetStoredValue(this.GetInterfaceName(), path, payload,
                    this.GetMajorVersion());
                }
                catch (AstartePropertyStorageException e)
                {
                    throw new AstarteTransportException("Property storage failure", e);
                }
            }
            else
            {
                if (!storedValue.PayloadEquality(payload))
                {
                    transport.SendIndividualValue(this, path, payload);
                }
            }

        }

        public void UnsetProperty(string path)
        {
            AstarteTransport? transport = GetAstarteTransport();

            if (transport == null)
            {
                throw new AstarteTransportException("No available transport");
            }

            transport.SendIndividualValue(this, path, null);

            try
            {
                propertyStorage.RemoveStoredPath(GetInterfaceName(), path, GetMajorVersion());
            }
            catch (AstartePropertyStorageException e)
            {
                throw new AstarteTransportException("Property storage failure", e);
            }
        }
    }
}
