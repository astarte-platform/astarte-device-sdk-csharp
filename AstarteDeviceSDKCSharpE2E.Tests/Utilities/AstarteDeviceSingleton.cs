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

using System;
using System.Threading;
using System.Threading.Tasks;
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharpE2E.Tests.AstarteMockData;

namespace AstarteDeviceSDKCSharpE2E.Tests.Utilities
{
    public sealed class AstarteDeviceSingleton
    {
        private static AstarteDevice astarteDevice = null;

        private AstarteDeviceSingleton()
        {

        }

        public static AstarteDevice Instance
        {
            get
            {
                if (astarteDevice == null)
                {
                    AstarteMockDevice astarteMockData = new();

                    if (astarteMockData is null)
                    {
                        return null;
                    }

                    string cryptoStoreDir = string.Empty;

                    if (String.IsNullOrEmpty(cryptoStoreDir))
                    {
                        cryptoStoreDir = AppDomain.CurrentDomain.BaseDirectory;
                    }

                    astarteDevice = new AstarteDevice(
                        astarteMockData.DeviceId,
                        astarteMockData.Realm,
                        astarteMockData.CredentialsSecret,
                        new MockInterfaceProvider(),
                        astarteMockData.PairingUrl,
                        cryptoStoreDir,
                        TimeSpan.FromMilliseconds(5000),
                        true
                    );
                    astarteDevice.SetAlwaysReconnect(true);
                    astarteDevice.SetAstarteMessageListener(new MessageListener());

                    return astarteDevice;
                }

                return astarteDevice;
            }
        }
    }

}
