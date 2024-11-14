<!---
    Copyright 2024 SECO Mind Srl

    SPDX-License-Identifier: Apache-2.0
-->

# Get started with CSharp

Follow this guide to get started with the Astarte device SDK for the CSharp programming language.

## Generating a device ID

A device ID will be required to uniquely identify a device in an Astarte instance. Some of the Astarte device SDKs provide utilities to generate a deterministic or random device identifier, in some cases based on hardware information.

This step is only useful when registering a device using a JWT token and the provided Astarte device SDKs registration APIs. Registration of a device can also be performed outside the device in the Astarte instance. In such cases, the device ID should be obtained via astartectl, or the Astarte dashboard. The device ID should then be loaded manually on the device.

A device ID can be generated randomly:

```CSharp
string deviceId = AstarteDeviceIdUtils.GenerateId();
```

Or in a deterministic way:

```CSharp
Guid nameSpace = Guid.Parse("f79ad91f-c638-4889-ae74-9d001a3b4cf8");
string macAddress = "98:75:a8:0d:96:db";
deviceId = AstarteDeviceIdUtils.GenerateId(nameSpace, macAddress);
```

## Registering a device

First, generate JWT using astartectl:

```
astartectl utils gen-jwt pairing -k <private-key-file>.pem
```

Then use said token to register the device:

```CSharp
string credentialsSecret = await new AstartePairingService(pairingUrl, realm).RegisterDeviceWithJwtToken(deviceId, jwt);
```

## Instantiating and connecting a new device

```CSharp
// Device creation
// connectionSource allows to connect to a db for persistency
// The interfaces supported by the device are populated by ExampleInterfaceProvider
AstarteDevice myDevice = new(
                        deviceId,
                        realm,
                        credentialsSecret,
                        interfaceProvider,
                        pairingUrl,
                        cryptoStoreDir,
                        TimeSpan.FromMilliseconds(500));

// ExampleMessageListener listens for device connection, disconnection and failure.
myDevice.SetAstarteMessageListener(new ExampleMessageListener());

// Connect the device
await myDevice.Connect();
```

## Streaming data

All Astarte Device SDKs include primitives for sending data to a remote Astarte instance. Streaming of data could be performed for device-owned interfaces of individual or object aggregation type.

### Streaming individual data

In Astarte interfaces with individual aggregation, each mapping is treated as an independent value and is managed individually.

The snippet below shows how to send a value that will be inserted into the "/test0/value" datastream which is defined by the "/%{sensor_id}/value" parametric endpoint, that is part of the "org.astarte-platform.genericsensors.Values" datastream interface.

```CSharp
AstarteDeviceDatastreamInterface valuesInterface =
                (AstarteDeviceDatastreamInterface)myDevice.GetInterface(valuesInterfaceName);

while (true)
{
        double value = Random.Shared.NextDouble();
        Console.WriteLine("Streaming value: " + value);

        await valuesInterface.StreamData($"/{sensorUuid}/value", value, DateTime.Now);

        Thread.Sleep(1000);
}
```

### Streaming aggregated data

In Astarte interfaces with object aggregation, Astarte expects the owner to send all of the interface’s mappings at the same time, packed in a single message.

The following snippet shows how to send a value for an object aggregated interface. In this example, lat and long will be sent together and will be inserted into the "/coords" datastream which is defined by the "/coords" endpoint, that is part of the "com.example.GPS" datastream interface.

```CSharp
AstarteDeviceAggregateDatastreamInterface aggregateInterface =
        (AstarteDeviceAggregateDatastreamInterface)myDevice.GetInterface(geolocationInterfaceName);

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
                { "speed", Random.Shared.NextDouble() * 100 }
        };
        Console.WriteLine("Streaming object:" + JsonConvert.SerializeObject(gpsValues));
        await aggregateInterface.StreamData($"/{sensor_id}", gpsValues, DateTime.Now);
        Thread.Sleep(1000);
}
```

## Setting and unsetting properties

Interfaces of property type represent a persistent, stateful, synchronized state with no concept of history or timestamping. From a programming point of view, setting and unsetting properties of device-owned interface is rather similar to sending messages on datastream interfaces.

The following snippet shows how to set a value that will be inserted into the "/sensor0/name" property which is defined by the "/%{sensor_id}/name" parametric endpoint, that is part of the "org.astarte-platform.genericsensors.AvailableSensors" device-owned properties interface.

It should be noted how a property should be marked as unsettable in its interface definition to be able to use the unsetting method on it.

Set property:

```CSharp
AstarteDevicePropertyInterface availableSensorsInterface =
        (AstarteDevicePropertyInterface)myDevice.GetInterface(availableSensorsInterfaceName);

availableSensorsInterface.SetProperty($"/{sensorUuid}/name", "randomThermometer");
availableSensorsInterface.SetProperty($"/{sensorUuid}/unit", "°C");
```

Unset property:

```CSharp
AstarteDevicePropertyInterface availableSensorsInterface =
        (AstarteDevicePropertyInterface)myDevice.GetInterface(availableSensorsInterfaceName);

availableSensorsInterface.UnsetProperty("/myPath/name");
```

## Receive data on server owner interface

In Astarte interfaces with server owner interface, Astarte sends data to connected devices. If a device is offline when data is sent, it receives the messages as soon as it reconnects to Astarte.

This section provides instructions on how to receive data on the server’s owner interface using the Astarte Device SDK for C#.

```CSharp
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using Newtonsoft.Json;

namespace AstarteDeviceSDKExample
{
        public class ExampleGlobalEventListener : AstarteGlobalEventListener
        {

        }
}
```

### Receiving on server owned datastream interface with individual aggregation

```CSharp
public override void ValueReceived(AstarteDatastreamEvent e)
{
        Console.WriteLine(
        $"Received datastream value on interface {e.GetInterfaceName()}, " +
        $"path: {e.GetPath()}, " +
        $"value: {e.GetValue()}");
}
```

### Receiving on server owned datastream interface with object aggregation

```CSharp
public override void ValueReceived(AstarteAggregateDatastreamEvent e)
{
        Console.WriteLine(
        $"Received aggregate datastream object on interface {e.GetInterfaceName()}, " +
        $"value: {JsonConvert.SerializeObject(e.GetValues())}");
}
```

### Receiving set on a server owned properties interface

```CSharp
public override void PropertyReceived(AstartePropertyEvent e)
{
        Console.WriteLine(
        $"Received property on interface {e.GetInterfaceName()}, " +
        $"path: {e.GetPath()}, " +
        $"value: {e.GetValue()}");
}
```

### Receiving unset on a server owned properties interface

```CSharp
public override void PropertyUnset(AstartePropertyEvent e)
{
        Console.WriteLine(
        $"Received unset on interface {e.GetInterfaceName()}, " +
        $"path: {e.GetPath()}");
}
```
