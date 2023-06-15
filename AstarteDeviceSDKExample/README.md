<!-- This file is part of Astarte.

Copyright 2023 SECO Mind Srl

SPDX-License-Identifier: Apache-2.0 -->

# Astarte Device SDK C# Example

This directory contains a minimal example that shows how to use the `AstarteDevice`.

The code provided in this sample includes comments that explicitly outline the key actions 
to be taken for setting up an Astarte device and sending data to Astarte.

## Usage

### 1. Device registration and credentials secret emission

*If the device is already registered, skip this section.*

The device must be registered beforehand to obtain its `credentials-secret`.
To obtain it there are three ways:
- Using the astartectl command [`astartectl`](https://github.com/astarte-platform/astartectl).
- Using the [`Astarte Dashboard`](https://docs.astarte-platform.org/snapshot/015-astarte_dashboard.html), which is located at `https://dashboard.<your-astarte-domain>`.
- Using `AstartePairingService` class contained in sdk (see following paragraph for details).

#### 1.1 Programmatically Generate an Astarte Device ID

The Astarte `device id` can be generated either randomly or using a GUID namespace and some unique data.

##### 1.1.1 Random

``` CSharp
String deviceId = AstarteDeviceIdUtils.GenerateId();
```

##### 1.1.2 UUIDv5

``` CSharp
Guid nameSpace = Guid.Parse("f79ad91f-c638-4889-ae74-9d001a3b4cf8");
String macAddress = "98:75:a8:0d:96:db";
deviceId = AstarteDeviceIdUtils.GenerateId(nameSpace, macAddress);
```

#### 1.2. Device Registration

The device can be registered either with JWT or with a private key file

#### 1.2.1 Register a device using JWT

In order to register a device, and obtain its credentials secret, a call to the registration API with a valid JWT is needed.

First generate JWT using astartectl:
```
astartectl utils gen-jwt pairing -k <private-key-file>.pem
```
Then use said token to register the device
```CSharp
String credentialsSecret = await new AstartePairingService(pairingUrl, realm).RegisterDeviceWithJwtToken(deviceId, jwt);
```

#### 1.2.2 Register device using private key file

In order to register a device, and obtain its credentials secret, a call to the registration API with the use of the private key is needed.

``` CSharp
String credentialsSecret = await new AstartePairingService(pairingUrl, realm).RegisterDeviceWithPrivateKey(deviceId, privateKeyFile);
```

### 2. Astarte device creation

``` CSharp
AstarteDevice myDevice = new(
            deviceId,
            realm,
            credentialsSecret,
            interfaceProvider,
            pairingUrl,
            cryptoStoreDir);
```

### 4. Start the connection

``` CSharp
await myDevice.Connect();
```

In order to operate with the device object safely, it is necessary to wait for the connection to be completed. This can be handled asynchronously in the Message Listener.

``` CSharp
while (!device.IsConnected()) {
    Thread.sleep(100);
}
```

### 5. Publish

#### 5.1 Publishing on a datastream interface with individual aggregation

Retrieve the interface from the device and call `StreamData` on it.

``` CSharp 
AstarteDeviceDatastreamInterface valuesInterface =
            (AstarteDeviceDatastreamInterface)myDevice.GetInterface(valuesInterfaceName);

while (true)
{
    double value = Random.Shared.NextDouble();
    Console.WriteLine("Streaming value: " + value);

    valuesInterface.StreamData($"/{sensorUuid}/value", value, DateTime.Now);

    Thread.Sleep(1000);
}   
```

#### 5.2 Publishing on a datastream interface with object aggregation

Retrieve the object aggregated interface and call `Stream data` on it.

``` CSharp
 AstarteDeviceAggregateDatastreamInterface aggregateInterface =
                (AstarteDeviceAggregateDatastreamInterface)myDevice
                .GetInterface(geolocationInterfaceName)

            while (true)
            {

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

                aggregateInterface.StreamData($"/{sensor_id}", gpsValues, DateTime.Now);

                Thread.Sleep(1000);

            }
```

#### 5.3 Setting a device properties

Retrieve the properties interface and call `SetProperty` on it.

``` CSharp
AstarteDevicePropertyInterface availableSensorsInterface =
                (AstarteDevicePropertyInterface)myDevice
                .GetInterface(availableSensorsInterfaceName);

            availableSensorsInterface.SetProperty(
                $"/{sensorUuid}/name", "randomThermometer");
            availableSensorsInterface.SetProperty($"/{sensorUuid}/unit", "°C")
```

#### 5.4 Unsetting a device properties

Retrieve the properties interface and call `UnsetProperty` on it.

``` CSharp
AstarteDevicePropertyInterface availableSensorsInterface =
                (AstarteDevicePropertyInterface)myDevice
                .GetInterface(availableSensorsInterfaceName);

availableSensorsInterface.UnsetProperty("/myPath/name");
```

### 6. Receive

The SDK offers an abstract class called `AstarteGlobalEventListener`, which includes several interfaces for obtaining datastreams and properties from Astarte. Inheriting from `AstarteGlobalEventListener` is essential to implement all of its methods.

#### 6.1 Receiving on server owned datastream interface

#### 6.1.1 Receiving on server owned datastream interface with individual aggregation

``` CSharp
public override void ValueReceived(AstarteDatastreamEvent e)
    {
        Console.WriteLine(
            $"Received datastream value on interface { e.GetInterfaceName()}, " +
            $"path: { e.GetPath()}, " +
            $"value:{ e.GetValue()}");
    }
```

#### 6.1.2 Receiving on server owned datastream interface with object aggregation

``` CSharp
public override void ValueReceived(AstarteAggregateDatastreamEvent e)
    {
        Console.WriteLine(
            $"Received aggregate datastream object on interface {e.GetInterfaceName()}, " +
            $"value:{JsonConvert.SerializeObject(e.GetValues())}");
    }
```

#### 6.2 Receiving on server owned properties interface

#### 6.2.1 Receiving data on server owned properties interface

``` CSharp
public override void PropertyReceived(AstartePropertyEvent e)
        {
            Console.WriteLine(
                $"Received property on interface { e.GetInterfaceName() }, " +
                $"path: { e.GetPath() }, " +
                $"value:{ e.GetValue() }");
        }
```

#### 6.2.2 Receiving unset on a server owned properties interface

``` CSharp
public override void PropertyUnset(AstartePropertyEvent e)
        {
            Console.WriteLine(
                $"Received unset on interface { e.GetInterfaceName() }, " +
                $"path: { e.GetPath() }");
        }
```

### 7. Run the example

#### 7.1 Using an unregistered device

Run the code from the root of the example project with

```
dotnet run AstarteDeviceSDKExample.csproj --r "<realm>" --p "<pairing-url>" --t "<jwt>"
```

where `pairing-url` is the URL to reach Pairing API in your Astarte instance, usually `https://api.<your-astarte-domain>/pairing`.

#### 7.2 Using a registered device

Run the code from the root of the example project with

```
dotnet run AstarteDeviceSDKExample.csproj --r "<realm>"  --p "<pairing-url>" --d "<device-id>" --c "<credential-secret>"
```

where `pairing-url` is the URL to reach Pairing API in your Astarte instance, usually `https://api.<your-astarte-domain>/pairing`.
