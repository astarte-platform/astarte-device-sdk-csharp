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
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using AstarteDeviceSDKCSharp.Utilities;

namespace AstarteDeviceSDKCSharp.Data
{
    public class AstartePropertyStorage : IAstartePropertyStorage
    {

        private readonly AstarteDbContext _astarteDbContext;

        public AstartePropertyStorage()
        {
            this._astarteDbContext = new AstarteDbContext();
        }

        public List<string> GetStoredPathsForInterface(string interfaceName, int interfaceMajor)
        {
            var astartePropretisList = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == interfaceName
                            && x.InterfaceMajor == interfaceMajor)
                            .Select(x => x.Path)
                            .ToList();

            if (astartePropretisList.Count == 0)
            {
                return new List<string>();
            }

            return astartePropretisList;
        }

        public DecodedMessage? GetStoredValue(AstarteInterface astarteInterface, string path,
        int interfaceMajor)
        {
            var astarteProprety = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == astarteInterface.InterfaceName
                                   && x.Path.EndsWith(path) && x.InterfaceMajor == interfaceMajor)
                            .FirstOrDefault();

            if (astarteProprety == null)
            {
                return null;
            }

            return AstartePayload.Deserialize(astarteProprety.BsonValue);
        }

        public Dictionary<string, object> GetStoredValuesForInterface
        (AstarteInterface astarteInterface)
        {
            Dictionary<string, object> returnedValues = new();

            var astartePropretisList = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == astarteInterface.InterfaceName
                            && x.InterfaceMajor == astarteInterface.GetMajorVersion())
                            .ToList();

            foreach (var property in astartePropretisList)
            {
                DecodedMessage? decodedMessage = AstartePayload.Deserialize(property.BsonValue);

                if (decodedMessage is null)
                {
                    return returnedValues;
                }

                Object? value = decodedMessage.GetPayload();
                if (value is not null)
                {
                    returnedValues.Add(property.Path, value);
                }

            }

            return returnedValues;
        }

        public void PurgeProperties(Dictionary<AstarteInterfaceHelper,
        List<string>> availableProperties)
        {
            foreach (var entry in availableProperties)
            {
                foreach (String storedPath in GetStoredPathsForInterface(entry.Key.InterfaceName,
                entry.Key.InterfaceMajor))
                {
                    if (!entry.Value.Contains(storedPath))
                    {
                        RemoveStoredPath(entry.Key.InterfaceName, storedPath,
                        entry.Key.InterfaceMajor);
                    }
                }
            }
        }

        public void RemoveStoredPath(string interfaceName, string path, int interfaceMajor)
        {

            var astarteProperty = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.Id == interfaceName + "/" + path
                            && x.InterfaceMajor == interfaceMajor)
                            .SingleOrDefault();

            if (astarteProperty == null)
            {
                throw new AstartePropertyStorageException("Error local database is empty.");
            }
            _astarteDbContext.Remove(astarteProperty);
            _astarteDbContext.SaveChanges();
        }

        public void SetStoredValue(string interfaceName, string path, object? value,
        int interfaceMajor)
        {
            var astarteProp = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == interfaceName && x.Path == path
                            && x.InterfaceMajor == interfaceMajor)
                            .FirstOrDefault();

            byte[] bsonValue = AstartePayload.Serialize(value, null);

            if (astarteProp == null)
            {
                AstarteGenericPropertyEntry astarteProperty =
                    new AstarteGenericPropertyEntry
                    (interfaceName,
                    path,
                    bsonValue,
                    interfaceMajor);

                _astarteDbContext.AstarteGenericProperties.Add(astarteProperty);
            }
            else
            {
                astarteProp.BsonValue = bsonValue;
            }

            _astarteDbContext.SaveChanges();
        }
    }
}
