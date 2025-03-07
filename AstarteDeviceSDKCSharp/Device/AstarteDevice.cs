/*
 * This file is part of Astarte.
 *
 * Copyright 2025 SECO Mind Srl
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
        private bool _initialized = false;
        private const string _cryptoSubDir = "crypto";
        private bool _alwaysReconnect = false;

        /// <summary>
        /// Basic class defining an Astarte device.
        /// Used for managing the device lifecycle and data. 
        /// Users can instantiate a device by providing a set of credentials 
        /// and connect it to the Astarte instance. 
        /// </summary>
        /// <param name="deviceId">The device ID for this device. 
        /// It has to be a valid Astarte device ID.
        /// </param>
        /// <param name="astarteRealm">The realm this device will be connecting to.</param>
        /// <param name="credentialSecret">The credentials secret for this device. 
        ///  This class assumes your device has
        ///  already been registered. If that is not the case register your device using either
        ///  :py:func:`register_device_with_jwt_token` or
        ///  :py:func:`register_device_with_private_key`.
        /// </param>
        /// <param name="astarteInterfaceProvider">Class for loading Astarte interfaces</param>
        /// <param name="pairingBaseUrl">The base URL of the pairing API of the 
        /// Astarte instance the device will connect to.</param>
        /// <param name="cryptoStoreDirectory">Path to an existing directory which will be used
        /// to store the persistent data of this device. (i.e. certificates, caching, and more). 
        /// It can be a shared directory for multiple devices, a subdirectory for the given device 
        /// ID will be created.
        /// </param>
        /// <param name="timeOut">The timeout duration for the connection.</param>
        public AstarteDevice(
            string deviceId,
            string astarteRealm,
            string credentialSecret,
            IAstarteInterfaceProvider astarteInterfaceProvider,
            string pairingBaseUrl,
            string cryptoStoreDirectory,
            TimeSpan? timeOut,
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
             astarteCryptoStore,
             (TimeSpan)(timeOut is null ? TimeSpan.FromSeconds(5) : timeOut),
             ignoreSSLErrors);

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

        private async Task Init()
        {
            await _pairingHandler.Init();

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

        /// <summary>
        /// Method for getting a list of interfaces for the device
        /// </summary>
        /// <returns>List of available interfaces</returns>
        public List<AstarteInterface> GetAllInterfaces()
        {
            return _astarteInterfaces.Values.ToList();
        }

        /// <summary>
        /// Enable/Disable automatic reconnection
        /// </summary>
        /// <param name="alwaysReconnect"></param>
        public void SetAlwaysReconnect(bool alwaysReconnect)
        {
            _alwaysReconnect = alwaysReconnect;
        }

        public bool GetAlwaysReconnect()
        {
            return _alwaysReconnect;
        }

        /// <summary>
        /// Establishes a connection to the Astarte asynchronously.
        /// </summary>
        /// <remarks>
        /// If the transport is not initialized, the method returns without performing any action. 
        /// If a crypto exception occurs during the connection attempt, 
        /// the method regenerates the certificate by requesting a new one from the pairing handler.
        /// If an exception occurs during the pairing process, the method raises the transport
        /// connection error event.<seealso cref="OnTransportConnectionError"/>
        /// </remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="AstartePairingException"></exception>
        public async Task Connect()
        {

            if (!_pairingHandler.IsCertificateAvailable())
            {
                await _pairingHandler.RequestNewCertificate();
                _initialized = false;
            }

            if (!_initialized)
            {

                await Init();
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
                AstarteLogger.Debug("Regenerating the cert", this.GetType().Name);
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

        /// <summary>
        /// Check if the device is currently connected.
        /// </summary>
        /// <returns>If device currently connected return true</returns>
        public bool IsConnected()
        {
            return _astarteTransport?.IsConnected() ?? false;
        }

        /// <summary>
        /// Disconnect device from Astarte
        /// </summary>
        public async Task Disconnect()
        {

            if (!IsConnected() || _astarteTransport is null)
            {
                return;
            }

            await _astarteTransport.Disconnect();

        }

        /// <summary>
        /// Adds an Interface to the Device.
        /// </summary>
        /// <param name="astarteInterfaceObject"></param>
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

        /// <summary>
        /// Getter function for an interface.
        /// </summary>
        /// <param name="interfaceName">Name of specific interface</param>
        /// <returns>AstarteInterface with matching name when present, null otherwise.</returns>
        public AstarteInterface? GetInterface(string interfaceName)
        {
            if (_astarteInterfaces.ContainsKey(interfaceName))
            {
                return _astarteInterfaces[interfaceName];
            }
            return null;
        }

        /// <summary>
        /// Setting up Astarte Transport
        /// </summary>
        /// <param><seealso cref="AstarteTransport"/>set by the user</param>
        public void SetAstarteTransport(AstarteTransport astarteTransport)
        {
            _astarteTransport = astarteTransport;
            ConfigureTransport(_astarteTransport);
        }

        /// <summary>
        /// Verify whether the interface has been added to the device.
        /// </summary>
        /// <param name="interfaceName">Name of specific interface</param>
        /// <returns>If device contains interface returns true</returns>
        public bool HasInterface(string interfaceName)
        {
            return _astarteInterfaces.ContainsKey(interfaceName);
        }

        /// <summary>
        /// Method for getting message listener
        /// </summary>
        /// <returns><seealso cref="IAstarteMessageListener"/> set by the user</returns>
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
                _astarteTransport?.StartResenderTask();
            }
        }

        public void OnTransportConnectionError(Exception ex)
        {
            lock (this)
            {
                if (ex is AstarteCryptoException)
                {
                    AstarteLogger.Debug("Regenerating the cert", this.GetType().Name);
                    try
                    {
                        Task.Run(() => _pairingHandler.RequestNewCertificate());
                    }
                    catch (AstartePairingException e)
                    {
                        _astarteMessagelistener?
                        .OnFailure(new AstarteMessageException(e.Message, e));
                        Trace.WriteLine(e);

                        return;
                    }

                }
                else
                {
                    _astarteMessagelistener?
                    .OnFailure(new AstarteMessageException(ex.Message, ex));
                }
            }
        }

        public void OnTransportDisconnected()
        {
            lock (this)
            {
                _astarteMessagelistener?
                .OnDisconnected(new AstarteMessageException("Connection lost"));
            }
        }
        public void OnTransportConnectionInitializationError(Exception ex)
        {
            _astarteMessagelistener?.OnFailure(new AstarteMessageException(ex.Message, ex));

            if (!_pairingHandler.IsCertificateAvailable())
            {
                _ = _astarteTransport?.Disconnect();
                _ = Connect();
            }

        }

        /// <summary>
        /// Add events for every interface on device
        /// </summary>
        /// <param name="eventListener">AstarteGlobalEventListener</param>
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

        /// <summary>
        /// Remove events for every interface on device
        /// </summary>
        /// <param name="eventListener">AstarteGlobalEventListener</param>
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
