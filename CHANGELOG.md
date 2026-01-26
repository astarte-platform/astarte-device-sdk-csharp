# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.7.2] - 2025-11-28
### Added
- Added support for NuGet package publishing.

## [0.7.1] - 2024-02-27
### Fixed
- Skip SSL certificate validation on HTTP requests if `ignoreSSLErrors` 
flag is enabled

## [0.7.0] - 2024-11-15
### Added
- Add MQTTNet ManagedClient extension version 4.1.4.563.
- `StreamData` change to `async StreamData` (BREAKING CHANGE!).
- `Disconnect` change to `async Disconnect` (BREAKING CHANGE!).
- Add timeout to the `AstartePairingService` constructor.
- Add device connection timeout to the `ÀstarteDevice` constructor.
- Update version of MQTTNet libary from 3.1.2 to 4.1.4.563.
- Add a fallout strategy for individual failed messages.
- Resend failed messages stored in the cache memory.

### Fixed
- Resend stored messages in the order in which they were streamed
by the user
- Regenerate client certificate after expiration

## [0.6.0] - 2023-12-18
### Added
- Add the capability to update the introspection dynamically.
- Handling sessionPresent flag from the broker.
- Add option to ignore SSL errors in the AstarteDevice constructor.
- Add a method to remove interface from device.

### Fixed
- Fix Connect method to handle certificate expiration.

## [0.5.4] - 2023-06-23
### Added
- Expose an interface path from Astarte aggregate datastream event.

### Fixed
- Fix a bug preventing sending of non-existing properties in local storage.

## [0.5.3] - 2023-06-07
### Fixed
- Fix payload validation on the aggregated object interface.
- Resolve the type inconsistency issue when deserializing BSON to a double array.
- Resolve array equality issue when setting the device property.
- Fix receiving unset property message.
- Fix parsing issue when receiving an aggregate object payload.

## [0.5.2] - 2023-05-26
### Fixed
- Fix a bug preventing sending of properties in the SendIndividualValue method.

## [0.5.1] - 2023-05-05
### Added
- Add a global event listener for listening incoming data from Astarte.

### Fixed
- Fix the construction of Pairing URL.
- Fix receiving properties from Astarte.

## [0.5.0] - 2023-04-07
### Added
- Initial Astarte Device C# SDK release.
