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
using AstarteDeviceSDKCSharp.Utilities;

namespace AstarteDeviceSDKCSharp.Data
{
    public interface IAstartePropertyStorage
    {
        public Dictionary<String, Object> GetStoredValuesForInterface
        (AstarteInterface astarteInterface);
        public List<string> GetStoredPathsForInterface(string interfaceName, int interfaceMajor);
        public DecodedMessage? GetStoredValue(AstarteInterface astarteInterface, String path,
        int interfaceMajor);
        public void SetStoredValue(String interfaceName, String path, Object? value,
        int interfaceMajor);
        public void RemoveStoredPath(String interfaceName, String path, int interfaceMajor);
        public void PurgeProperties(Dictionary<AstarteInterfaceHelper,
        List<String>> availableProperties);
    }
}
