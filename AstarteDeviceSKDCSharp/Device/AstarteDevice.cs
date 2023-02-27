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
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Protocol.AstarteExeption;
using AstarteDeviceSDKCSharp.Transport;
using Microsoft.EntityFrameworkCore;

namespace AstarteDeviceSDKCSharp.Device
{
    public class AstarteDevice
    {
        private readonly Dictionary<string, AstarteInterface> _astarteInterfaces = new();
        private readonly AstartePairingHandler _pairingHandler;
        private AstarteTransport? _astarteTransport;
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
             new AstarteCryptoStore(Path.Combine(cryptoStoreDirectory, deviceId, "crypto")));

            List<string> allInterfaces = astarteInterfaceProvider.LoadAllInterfaces();

            foreach (var item in allInterfaces)
            {
                AstarteInterface astarteInterface = AstarteInterface.FromString(item);
                _astarteInterfaces.Add(astarteInterface.GetInterfaceName(), astarteInterface);
            }

            using (var _context = new AstarteDbContext())
            {
                _context.Database.SetCommandTimeout(160);
                _context.Database.Migrate();
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
            ConfigureTransport(_astarteTransport);
        }

        private void ConfigureTransport(AstarteTransport astarteTransport)
        {
            astarteTransport.SetDevice(this);

            // Set transport on all interfaces
            foreach (AstarteInterface astarteInterface in GetAllInterfaces())
            {
                astarteInterface.SetAstarteTransport(astarteTransport);
            }

        }

        public List<AstarteInterface> GetAllInterfaces()
        {
            return _astarteInterfaces.Values.ToList();
        }

        public async Task Connect()
        {

            if (!_initialized)
            {
                Init();
                _initialized = true;
            }

            if (_astarteTransport == null)
            {
                throw new AstarteTransportException("Astarte transport is null");
            }

            await _astarteTransport.Connect();

        }

        public void AddInterface(string astarteInterfaceObject)
        {
            AstarteInterface astarteInterface = AstarteInterface.FromString(astarteInterfaceObject);

            AstarteInterface? formerInterface = GetInterface(astarteInterface.GetInterfaceName());

            if (formerInterface == null)
            {
                throw new AstarteInvalidInterfaceException("Astarte interface is null");
            }

            if (formerInterface.GetMajorVersion() == astarteInterface.GetMajorVersion())
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

            if (_astarteTransport == null)
            {
                throw new AstarteTransportException("Astarte transport is null");
            }

            _astarteInterfaces.Add(astarteInterface.GetInterfaceName(), astarteInterface);
            astarteInterface.SetAstarteTransport(_astarteTransport);
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
            ConfigureTransport(_astarteTransport);
        }

        public bool HasInterface(string interfaceName)
        {
            return _astarteInterfaces.ContainsKey(interfaceName);
        }

    }
}
