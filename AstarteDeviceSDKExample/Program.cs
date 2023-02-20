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
using AstarteDeviceSDKExample;

string valuesInterfaceName = "org.astarte-platform.genericsensors.Values";
string pairingUrl = "http://localhost:4003";  //replace with your pairing url
string realm = "test"; //replace with your realm name
string cryptoStoreDir = AppDomain.CurrentDomain.BaseDirectory; // direcory to save certificates
string deviceId = ""; // replace with your device ID or leave empty to get random device ID
string credentialsSecret = ""; // replace with your credential or leave empty
var sensorUuid = "b2c5a6ed-ebe4-4c5c-9d8a-6d2f114fc6e5";
// generate jwt using astartectl: astartectl utils gen-jwt pairing -k test_private.pem
string jwt = "";


/// <summary>
/// Astarte device id creation
/// </summary>
if (String.IsNullOrEmpty(deviceId))
{
    Guid nameSpace = Guid.Parse("f79ad91f-c638-4889-ae74-9d001a3b4ca4");
    string macAdress = "0099112233";
    deviceId = AstarteDeviceIdUtils.GenerateId(nameSpace, macAdress);
    credentialsSecret = await new AstartePairingService(pairingUrl, realm)
        .RegisterDeviceWithJwtToken(deviceId, jwt);
}

//Path validation will be implemented on device creation
#region check path 
if (!Directory.Exists(cryptoStoreDir))
{
    throw new FileNotFoundException(cryptoStoreDir + " is not directory");
}

if (!Directory.Exists(Path.Join(cryptoStoreDir, deviceId)))
{
    Directory.CreateDirectory(Path.Join(cryptoStoreDir, deviceId));
}

if (!Directory.Exists(Path.Join(cryptoStoreDir, deviceId, "crypto")))
{
    Directory.CreateDirectory(Path.Join(cryptoStoreDir, deviceId, "crypto"));
}
#endregion

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
    cryptoStoreDir);

/// <summary>
/// Start the connection
/// </summary>
myDevice.Connect();

AstarteDeviceDatastreamInterface valuesInterface =
    (AstarteDeviceDatastreamInterface)myDevice.GetInterface(valuesInterfaceName);

while (true)
{
    double value = Random.Shared.NextDouble();
    Console.WriteLine("Streaming value: " + value);
    valuesInterface.StreamData($"/{sensorUuid}/value", value, DateTime.Now);

    Thread.Sleep(1000);
}
