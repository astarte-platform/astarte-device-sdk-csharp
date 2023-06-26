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
using AstarteDeviceSDKCSharp.Crypto;
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Transport;
using Microsoft.EntityFrameworkCore;

namespace AstarteDeviceSDKCSharp.Device
{
    public class AstarteDevice : IAstarteTransportEventListener
    {
        private readonly Dictionary<string, AstarteInterface> _astarteInterfaces = new();
        private readonly AstartePairingHandler _pairingHandler;
        private AstarteTransport? _astarteTransport;
        private IAstarteMessageListener? _astarteMessagelistener;
        private IAstartePropertyStorage astartePropertyStorage;
        private AstarteFailedMessageStorage _astarteFailedMessageStorage;
        private bool _initialized;
        private const string _cryptoSubDir = "crypto";
        private bool _alwaysReconnect = false;
        private bool _explicitDisconnectionRequest;
        private static int MIN_INCREMENT_INTERVAL = 5000;
        private static int MAX_INCREMENT_INTERVAL = 60000;

        public AstarteDevice(
            string deviceId,
            string astarteRealm,
            string credentialSecret,
            IAstarteInterfaceProvider astarteInterfaceProvider,
            string pairingBaseUrl,
            string cryptoStoreDirectory,
            bool ignoreSSLErrors = false)
        {
            if (!Directory.Exists(cryptoStoreDirectory))
            {
                throw new DirectoryNotFoundException($"Unable to found {cryptoStoreDirectory}");
            }

            string fullCryptoDirPath = Path.Combine(cryptoStoreDirectory, deviceId, _cryptoSubDir);
            if (!Directory.Exists(fullCryptoDirPath))
            {
                Directory.CreateDirectory(fullCryptoDirPath);
            }

            AstarteCryptoStore astarteCryptoStore = new AstarteCryptoStore(fullCryptoDirPath);
            astarteCryptoStore.IgnoreSSLErrors = ignoreSSLErrors;

            _pairingHandler = new AstartePairingHandler(
             pairingBaseUrl,
             astarteRealm,
             deviceId,
             credentialSecret,
             astarteCryptoStore);

            astartePropertyStorage = new AstartePropertyStorage(fullCryptoDirPath);

            List<string> allInterfaces = astarteInterfaceProvider.LoadAllInterfaces();

            foreach (var item in allInterfaces)
            {
                AstarteInterface astarteInterface = AstarteInterface.FromString(item, astartePropertyStorage);
                _astarteInterfaces.Add(astarteInterface.GetInterfaceName(), astarteInterface);
            }

            using (var _context = new AstarteDbContext(fullCryptoDirPath))
            {
                _context.Database.SetCommandTimeout(160);
                _context.Database.Migrate();
            }

            _astarteFailedMessageStorage = new(fullCryptoDirPath);
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
            astarteTransport.SetFailedMessageStorage(_astarteFailedMessageStorage);

            if (_astarteTransport != null)
            {
                _astarteTransport.SetAstarteTransportEventListener(this);
                _astarteTransport.SetPropertyStorage(astartePropertyStorage);
                if (_astarteMessagelistener != null)
                {
                    _astarteTransport.SetMessageListener(_astarteMessagelistener);
                }
            }

            // Set transport on all interfaces
            foreach (AstarteInterface astarteInterface in GetAllInterfaces())
            {
                astarteInterface.SetAstarteTransport(astarteTransport);
            }

        }

        private bool EventualyReconnect()
        {
            if (_astarteTransport is null)
            {
                return false;
            }
            lock (this)
            {
                int x = 1;
                int interval = 0;
                while (_alwaysReconnect && !IsConnected())
                {

                    if (interval < MAX_INCREMENT_INTERVAL)
                    {
                        interval = MIN_INCREMENT_INTERVAL * x;
                        x++;
                    }
                    else
                    {
                        interval = MAX_INCREMENT_INTERVAL;
                    }
                    Task.Run(async () => await Connect()).Wait(interval);
                }
            }

            _explicitDisconnectionRequest = false;
            return false;
        }

        public List<AstarteInterface> GetAllInterfaces()
        {
            return _astarteInterfaces.Values.ToList();
        }

        public void SetAlwaysReconnect(bool alwaysReconnect)
        {
            _alwaysReconnect = alwaysReconnect;
        }

        public async Task Connect()
        {

            if (!_initialized)
            {

                Init();
                _initialized = true;
            }

            if (IsConnected())
            {
                return;
            }

            try
            {
                if (_astarteTransport is null)
                {
                    return;
                }
                await _astarteTransport.Connect();
            }
            catch (AstarteCryptoException)
            {
                Trace.WriteLine("Regenerating the cert");
                try
                {
                    await _pairingHandler.RequestNewCertificate();
                    _initialized = false;
                }
                catch (AstartePairingException ex)
                {
                    OnTransportConnectionError(ex);
                    return;
                }
            }
        }

        public bool IsConnected()
        {
            return _astarteTransport?.IsConnected() ?? false;
        }

        public void Disconnect()
        {
            lock (this)
            {
                if (!IsConnected())
                {
                    return;
                }
                _explicitDisconnectionRequest = true;
                _astarteTransport?.Disconnect();
            }
        }

        public void AddInterface(string astarteInterfaceObject)
        {
            AstarteInterface astarteInterface = AstarteInterface.FromString(astarteInterfaceObject, astartePropertyStorage);

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

            if (IsConnected())
            {
                astarteInterface.SetAstarteTransport(_astarteTransport);
                _astarteTransport.SendIntrospection();
            }
        }

        /// <summary>
        /// Remove interface from device 
        /// </summary>
        /// <param name="interfaceName">Interface name</param>
        public void RemoveInterface(string interfaceName)
        {
            AstarteInterface? astarteInterface = GetInterface(interfaceName);

            if (astarteInterface == null)
            {
                throw new AstarteInterfaceException("Interface " + interfaceName + " not found");
            }

            _astarteInterfaces.Remove(astarteInterface.GetInterfaceName());

            if (IsConnected())
            {
                _astarteTransport?.SendIntrospection();
            }
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

        public IAstarteMessageListener? GetAstarteMessageListener()
        {
            return _astarteMessagelistener == null ? null : _astarteMessagelistener;
        }

        public void SetAstarteMessageListener(IAstarteMessageListener astarteMessageListener)
        {
            _astarteMessagelistener = astarteMessageListener;
            if (_astarteTransport != null && _astarteMessagelistener != null)
            {
                _astarteTransport.SetMessageListener(astarteMessageListener);
            }
        }

        public void OnTransportConnected()
        {
            lock (this)
            {
                _astarteMessagelistener?.OnConnected();
            }
        }

        public void OnTransportConnectionInitializationError(Exception ex)
        {
            lock (this)
            {

                _astarteMessagelistener?.OnFailure(new AstarteMessageException(ex.Message, ex));

                new Thread(delegate ()
                {
                    try
                    {
                        Disconnect();

                        _astarteMessagelistener?
                        .OnDisconnected(new AstarteMessageException(ex.Message, ex));

                    }
                    catch (AstarteTransportException e)
                    {
                        Trace.WriteLine(e.Message);
                    }
                    EventualyReconnect();
                }).Start();

            }
        }

        public void OnTransportConnectionError(Exception ex)
        {
            lock (this)
            {
                if (ex is AstarteCryptoException)
                {
                    Trace.WriteLine("Regenerating the cert");
                    try
                    {
                        Task.Run(() => _pairingHandler.RequestNewCertificate());
                    }
                    catch (AstartePairingException e)
                    {

                        if (!EventualyReconnect())
                        {

                            _astarteMessagelistener?
                            .OnFailure(new AstarteMessageException(e.Message, e));
                            Trace.WriteLine(e);
                        }
                        return;
                    }

                    try
                    {
                        _astarteTransport?.Connect();
                    }
                    catch (AstarteTransportException e)
                    {

                        _astarteMessagelistener?
                        .OnFailure(new AstarteMessageException(e.Message, e));

                    }
                }
                else
                {
                    if (!EventualyReconnect())
                    {

                        _astarteMessagelistener?
                        .OnFailure(new AstarteMessageException(ex.Message, ex));

                    }
                }
            }
        }

        public void OnTransportDisconnected()
        {
            lock (this)
            {

                _astarteMessagelistener?
                .OnDisconnected(new AstarteMessageException("Connection lost"));

                if (_alwaysReconnect && !_explicitDisconnectionRequest)
                {
                    EventualyReconnect();
                }
                _explicitDisconnectionRequest = false;

            }
        }

        public void AddGlobalEventListener(AstarteGlobalEventListener eventListener)
        {
            foreach (AstarteInterface astarteInterface in _astarteInterfaces.Values)
            {
                if (astarteInterface is AstarteServerPropertyInterface astarteServerPropertyInterface)
                {
                    astarteServerPropertyInterface.AddListener(eventListener);
                }
                else if (astarteInterface is AstarteServerDatastreamInterface astarteServerDatastreamInterface)
                {
                    astarteServerDatastreamInterface.AddListener(eventListener);
                }
                else if (astarteInterface is AstarteServerAggregateDatastreamInterface astarteServerAggregateDatastreamInterface)
                {
                    astarteServerAggregateDatastreamInterface.AddListener(eventListener);
                }
            }
        }

        public void RemoveGlobalEventListener(AstarteGlobalEventListener eventListener)
        {
            foreach (AstarteInterface astarteInterface in _astarteInterfaces.Values)
            {
                if (astarteInterface is AstarteServerPropertyInterface astarteServerPropertyInterface)
                {
                    astarteServerPropertyInterface.RemoveListener(eventListener);
                }
                else if (astarteInterface is AstarteServerDatastreamInterface astarteServerDatastreamInterface)
                {
                    astarteServerDatastreamInterface.RemoveListener(eventListener);
                }
                else if (astarteInterface is AstarteServerAggregateDatastreamInterface astarteServerAggregateDatastreamInterface)
                {
                    astarteServerAggregateDatastreamInterface.RemoveListener(eventListener);
                }
            }
        }

    }
}
