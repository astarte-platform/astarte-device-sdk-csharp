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

using AstarteDeviceSDKCSharp;
using System.Reflection;

namespace AstarteDeviceSDKExample
{
    internal class ExampleInterfaceProvider : IAstarteInterfaceProvider
    {
        /// <summary>
        /// Load the interfaces from JSON files that are in the resources folder.
        /// </summary>
        /// <returns>All the interfaces supported by this device.</returns>
        public List<string> LoadAllInterfaces()
        {
            List<string> interfaces = new List<string>();
            List<string> interfaceNames = new List<string>
            {
                "org.astarte-platform.genericevents.DeviceEvents",
                "org.astarte-platform.genericsensors.Values",
                "org.astarte-platform.genericsensors.AvailableSensors",
                "org.astarte-platform.genericsensors.Geolocation"
            };

            foreach (string interfaceName in interfaceNames)
            {
                interfaces.Add(LoadInterface(interfaceName));
            }

            return interfaces;
        }

        /// <summary>
        /// Load the interface from JSON files that is in the resource folder.
        /// </summary>
        /// <param name="interfaceName">Interface name</param>
        /// <returns>The interface with the given interface name.</returns>
        public string LoadInterface(string interfaceName)
        {
            string text =
                File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                , "Resources", "standard-interfaces", interfaceName + ".json"));

            return text;
        }

    }
}
