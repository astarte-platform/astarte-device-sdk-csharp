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
using Microsoft.EntityFrameworkCore;

namespace AstarteDeviceSDKCSharp.Data
{
    public class AstartePropertyStorage : IAstartePropertyStorage
    {

        private readonly AstarteDbContext _astarteDbContext;

        public AstartePropertyStorage()
        {
            this._astarteDbContext = new AstarteDbContext();
        }

        public List<string> GetStoredPathsForInterface(string interfaceName)
        {
            var astartePropretisList = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == interfaceName)
                            .Select(x => x.Path)
                            .ToList();

            if (astartePropretisList.Count == 0)
            {
                throw new AstartePropertyStorageException("Error local database is empty.");
            }

            return astartePropretisList;
        }

        public DecodedMessage GetStoredValue(AstarteInterface astarteInterface, string path)
        {
            var astarteProprety = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == astarteInterface.InterfaceName
                                   && x.Path.EndsWith(path))
                            .FirstOrDefault();

            if (astarteProprety == null)
            {
                throw new AstartePropertyStorageException("Error local database is empty.");
            }

            return AstartePayload.Deserialize(astarteProprety.BsonValue);
        }

        public Dictionary<string, object> GetStoredValuesForInterface(AstarteInterface astarteInterface)
        {
            Dictionary<string, object> returnedValues = new();

            var astartePropretisList = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == astarteInterface.InterfaceName)
                            .ToList();

            if (astartePropretisList.Count == 0)
            {
                throw new AstartePropertyStorageException("Error local database is empty.");
            }

            foreach (var property in astartePropretisList)
            {
                DecodedMessage decodedMessage = AstartePayload.Deserialize(property.BsonValue);

                Object value = decodedMessage.GetPayload();
                returnedValues.Add(property.Path, value);
            }

            return returnedValues;
        }

        public void PurgeProperties(Dictionary<string, List<string>> availableProperties)
        {
            foreach (var entry in availableProperties)
            {
                foreach (String storedPath in GetStoredPathsForInterface(entry.Key))
                {
                    if (!entry.Value.Contains(storedPath))
                    {
                        RemoveStoredPath(entry.Key, storedPath);
                    }
                }
            }
        }

        public void RemoveStoredPath(string interfaceName, string path)
        {

            var astarteProperty = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.Id == interfaceName + "/" + path)
                            .SingleOrDefault();

            if (astarteProperty == null)
            {
                throw new AstartePropertyStorageException("Error local database is empty.");
            }
            _astarteDbContext.Remove(astarteProperty);

            _astarteDbContext.SaveChanges();
        }

        public void SetStoredValue(string interfaceName, string path, object value)
        {
            var astarteProp = _astarteDbContext.AstarteGenericProperties
                            .Where(x => x.InterfaceName == interfaceName && x.Path == path)
                            .FirstOrDefault();

            if (astarteProp == null)
            {
                byte[] bsonValue = AstartePayload.Serialize(value, null);


                AstarteGenericPropertyEntry astarteProperty =
                    new AstarteGenericPropertyEntry(interfaceName, path, bsonValue);

                _astarteDbContext.AstarteGenericProperties.Add(astarteProperty);

            }
            else
            {
                if (!astarteProp.InterfaceName.Equals(interfaceName))
                {
                    astarteProp.InterfaceName = interfaceName;
                }
                if (!astarteProp.Path.Equals(path))
                {
                    astarteProp.Path = path;
                }
            }

            _astarteDbContext.SaveChanges();
        }
    }
}
