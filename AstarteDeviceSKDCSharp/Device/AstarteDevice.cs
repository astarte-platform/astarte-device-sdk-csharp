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
using AstarteDeviceSDKCSharp.Crypto;
using AstarteDeviceSDKCSharp.Protocol.AstarteExeption;
using AstarteDeviceSDKCSharp.Transport;

namespace AstarteDeviceSDKCSharp.Device
{
    public class AstarteDevice
    {
        private readonly Dictionary<string, AstarteInterface> _astarteInterfaces = new();
        private readonly AstartePairingHandler _pairingHandler;
        private AstarteTransport _astarteTransport;
        private bool _initialized;

        public AstarteDevice(
            string deviceId,
            string astarteRealm,
            string credentialSecret,
            IAstarteInterfaceProvider astarteInterfaceProvider,
            string pairingBaseUrl,
            string cryptoStoreDirectory)
        {

            _pairingHandler = new AstartePairingHandler(
             pairingBaseUrl,
             astarteRealm,
             deviceId,
             credentialSecret,
             new AstarteCryptoStore($@"{cryptoStoreDirectory}\{deviceId}\crypto\"));

            List<string> allInterfaces = astarteInterfaceProvider.LoadAllInterfaces();

            foreach (var item in allInterfaces)
            {
                AstarteInterface astarteInterface = AstarteInterface.FromString(item);
                _astarteInterfaces.Add(astarteInterface.GetInterfaceName(), astarteInterface);
            }

        }

        private void Init()
        {
            _pairingHandler.Init();

            // Get and configure the first available transport
            SetFirstTransportFromPairingHandler();
        }

        private void SetFirstTransportFromPairingHandler()
        {
            _astarteTransport = _pairingHandler.GetTransports().First();
            if (_astarteTransport == null)
            {
                throw new AstarteTransportException("No supported transports for the device !");
            }
            ConfigureTransport();
        }

        private void ConfigureTransport()
        {
            _astarteTransport.SetDevice(this);

            // Set transport on all interfaces
            foreach (AstarteInterface astarteInterface in GetAllInterfaces())
            {
                astarteInterface.SetAstarteTransport(_astarteTransport);
            }

        }

        public List<AstarteInterface> GetAllInterfaces()
        {
            return _astarteInterfaces.Values.ToList();
        }

        public void Connect()
        {

            if (!_initialized)
            {
                Init();
                _initialized = true;
            }

            _astarteTransport.Connect();

        }

        public void AddInterface(string astarteInterfaceObject)
        {
            AstarteInterface astarteInterface = AstarteInterface.FromString(astarteInterfaceObject);

            AstarteInterface formerInterface = GetInterface(astarteInterface.GetInterfaceName());
            if (formerInterface != null &&
            formerInterface.GetMajorVersion() == astarteInterface.GetMajorVersion())
            {
                if (formerInterface.GetMinorVersion() == astarteInterface.GetMinorVersion())
                {
                    throw new AstarteInterfaceAlreadyPresentException
                    ("Interface already present in mapping");
                }
                if (formerInterface.GetMinorVersion() > astarteInterface.GetMinorVersion())
                {
                    throw new AstarteInvalidInterfaceException
                    ("Can't downgrade an interface at runtime");
                }
            }

            _astarteInterfaces.Add(astarteInterface.GetInterfaceName(), astarteInterface);
        }

        public AstarteInterface? GetInterface(string interfaceName)
        {
            if (_astarteInterfaces.ContainsKey(interfaceName))
            {
                return _astarteInterfaces[interfaceName];
            }
            return null;
        }

        public void SetAstarteTransport(AstarteTransport astarteTransport)
        {
            _astarteTransport = astarteTransport;
            ConfigureTransport();
        }

    }
}
