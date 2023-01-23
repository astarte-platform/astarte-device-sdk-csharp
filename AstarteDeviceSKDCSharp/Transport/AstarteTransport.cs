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
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;

namespace AstarteDeviceSDKCSharp.Transport
{
    public abstract class AstarteTransport : IAstarteProtocol
    {
        private readonly AstarteProtocolType astarteProtocolType;

        public AstarteDevice Device { get; set; }

        public AstarteIntrospection AstarteIntrospection { get; set; }
        protected AstarteTransport(AstarteProtocolType type)
        {
            astarteProtocolType = type;
        }

        public abstract Task SendIntrospection();
        public abstract Task SendIndividualValue(AstarteInterface astarteInterface, string path, object value, DateTime? timestamp);
        public abstract Task SendIndividualValue(AstarteInterface astarteInterface, string path, object value);


        public void SetDevice(AstarteDevice astarteDevice)
        {
            Device = astarteDevice;
        }
        public AstarteDevice GetDevice()
        {
            return Device;
        }

        public AstarteProtocolType GetAstarteProtocolType()
        {
            return astarteProtocolType;
        }

        public abstract void Connect();
        public abstract void Disconnect();
        public abstract bool IsConnected();

    }
}
