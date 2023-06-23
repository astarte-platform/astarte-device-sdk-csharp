# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
