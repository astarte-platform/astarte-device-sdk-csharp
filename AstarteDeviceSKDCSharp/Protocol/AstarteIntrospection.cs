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
using AstarteDeviceSDKCSharp.Protocol.AstarteExeption;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteIntrospection : IAstarteIntrospection
    {

        private readonly Dictionary<string, AstarteInterface> astarteInterfaces = new();

        public void AddAstarteInterface(string astarteInterfaceObject)
        {
            AstarteInterface newInterface =
        AstarteInterface.FromString(astarteInterfaceObject);

            AstarteInterface formerInterface = GetAstarteInterface(newInterface.InterfaceName);

            if (formerInterface != null
                && formerInterface.MajorVersion == newInterface.MajorVersion)
            {
                if (formerInterface.MinorVersion == newInterface.MinorVersion)
                {
                    throw new AstarteInterfaceAlreadyPresentException("Interface already present in mapping");
                }
                if (formerInterface.MinorVersion > newInterface.MinorVersion)
                {
                    throw new AstarteInvalidInterfaceException("Can't downgrade an interface at runtime");
                }
            }
            astarteInterfaces.Add(newInterface.InterfaceName, newInterface);
        }

        public List<AstarteInterface> GetAllAstarteInterfaces()
        {
            return astarteInterfaces.Values.ToList();
        }

        public AstarteInterface? GetAstarteInterface(string astarteInterfaceObject)
        {
            foreach (var astarteInterface in astarteInterfaces)
            {
                if (astarteInterface.Key == astarteInterfaceObject)
                {
                    return astarteInterface.Value;
                }
            }
            return null;
        }

        public void RemoveAstarteInterface()
        {
            throw new NotImplementedException();
        }
    }
}
