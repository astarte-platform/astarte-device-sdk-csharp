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
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Utilities;
using CommandLine;
using Newtonsoft.Json;

namespace AstarteDeviceSDKExample
{
    class Program
    {
        private static readonly string valuesInterfaceName =
            "org.astarte-platform.genericsensors.Values";
        private static readonly string availableSensorsInterfaceName =
            "org.astarte-platform.genericsensors.AvailableSensors";
        private static readonly string geolocationInterfaceName =
            "org.astarte-platform.genericsensors.Geolocation";
        private static readonly string sensorUuid =
            "b2c5a6ed-ebe4-4c5c-9d8a-6d2f114fc6e5";

        static async Task<int> Main(string[] args)
        {
            string realm = string.Empty;
            string pairingUrl = string.Empty;
            string deviceId = string.Empty;
            string credentialsSecret = string.Empty;
            string jwt = string.Empty;
            string cryptoStoreDir = string.Empty;

            var result = Parser.Default.ParseArguments<Options>(args)
                     .WithParsed(o =>
                     {
                         realm = o.Realm;
                         pairingUrl = o.PairingUrl;
                         deviceId = o.DeviceId;
                         credentialsSecret = o.CredentialsSecret;
                         jwt = o.Jwt;
                         cryptoStoreDir = o.CryptoStoreDir;
                     });

            if (result.Errors.Any())
            {
                return 1;
            }

            /// <summary>
            /// Astarte device id creation
            /// </summary>
            if (String.IsNullOrEmpty(deviceId))
            {
                Guid nameSpace = Guid.NewGuid();
                string macAdress = "0099112233";
                deviceId = AstarteDeviceIdUtils.GenerateId(nameSpace, macAdress);
                credentialsSecret = await new AstartePairingService(
                    pairingUrl,
                    realm,
                    TimeSpan.FromSeconds(1))
                    .RegisterDeviceWithJwtToken(deviceId, jwt);
            }

            if (String.IsNullOrEmpty(cryptoStoreDir))
            {
                cryptoStoreDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            /// <summary>
            /// Astarte device creation
            /// 
            /// The interfaces supported by the device are populated by ExampleInterfaceProvider,
            /// see that class for more details
            /// </summary>
            var interfaceProvider = new ExampleInterfaceProvider();
            AstarteDevice myDevice = new(
                deviceId,
                realm,
                credentialsSecret,
                interfaceProvider,
                pairingUrl,
                cryptoStoreDir,
                TimeSpan.FromMilliseconds(500),
                true);

            myDevice.SetAlwaysReconnect(true);
            myDevice.SetAstarteMessageListener(new ExampleMessageListener());
            myDevice.AddGlobalEventListener(new ExampleGlobalEventListener());

            /// <summary>
            /// Start the connection
            /// </summary>
            await myDevice.Connect();

            while (!myDevice.IsConnected())
            {
                Thread.Sleep(1000);
            }

            /// <summary>
            /// Publish on a properties interface
            /// Retrieve the interface from the device and call SetProperty and UnsetPropery on it.
            /// </summary>
            AstarteDevicePropertyInterface availableSensorsInterface =
                (AstarteDevicePropertyInterface)myDevice
                .GetInterface(availableSensorsInterfaceName);

            availableSensorsInterface.SetProperty(
                $"/{sensorUuid}/name", "randomThermometer");
            availableSensorsInterface.SetProperty($"/{sensorUuid}/unit", "°C");

            // Unset property
            availableSensorsInterface.SetProperty(
                "/myPath/name", "randomThermometer");
            availableSensorsInterface.UnsetProperty("/myPath/name");

            /// <summary>
            /// Individual datastream interface creation
            /// </summary>
            AstarteDeviceDatastreamInterface valuesInterface =
                (AstarteDeviceDatastreamInterface)myDevice
                .GetInterface(valuesInterfaceName);

            /// <summary>
            /// Aggregate datastream interface creation
            /// </summary>
            AstarteDeviceAggregateDatastreamInterface aggregateInterface =
                (AstarteDeviceAggregateDatastreamInterface)myDevice
                .GetInterface(geolocationInterfaceName);

            while (true)
            {
                double value = Random.Shared.NextDouble();
                Console.WriteLine("Streaming value: " + value);

                //Stream individual value
                valuesInterface.StreamData($"/{sensorUuid}/value", value, DateTime.Now);

                Dictionary<string, object> gpsValues = new()
                {
                    { "latitude", Random.Shared.NextDouble() * 50 },
                    { "longitude", Random.Shared.NextDouble() * 50 },
                    { "altitude", Random.Shared.NextDouble() },
                    { "accuracy", Random.Shared.NextDouble() },
                    { "altitudeAccuracy", Random.Shared.NextDouble() },
                    { "heading", Random.Shared.NextDouble() },
                    { "speed", Random.Shared.NextDouble() * 100}
                };

                Console.WriteLine("Streaming object:" + JsonConvert.SerializeObject(gpsValues));

                //Stream aggregate object
                aggregateInterface.StreamData($"/{sensorUuid}", gpsValues, DateTime.Now);

                Thread.Sleep(1000);

            }

        }

        public class Options
        {
            [Option('r', "realm", Required = true, HelpText = "The target Astarte realm")]
            public string Realm { get; set; }

            [Option('t', "jwt", SetName = "UseToken", Required = true,
                HelpText = "The jwt for the Astarte Register Device. " +
                "Generate jwt using astartectl:astartectl utils gen-jwt pairing -k test_private.pem")]
            public string Jwt { get; set; }

            [Option('d', "device-id", Required = false,
                HelpText = "The device id for the Astarte Device")]
            public string DeviceId { get; set; }

            [Option('c', "credentials-secret", SetName = "UseCredentials", Required = true,
                HelpText = "The credentials secret for the Astarte Device")]
            public string CredentialsSecret { get; set; }

            [Option('p', "pairing-url", Required = true,
                HelpText = "The URL to reach Pairing API in the target Astarte instance")]
            public string PairingUrl { get; set; }

            [Option('s', "crypto-store", Required = false,
                HelpText = "The existing path for storing certificates")]
            public string CryptoStoreDir { get; set; }
        }
    }
}
