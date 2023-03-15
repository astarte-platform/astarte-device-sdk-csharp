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

namespace AstarteDeviceSDKCSharp.Protocol.AstarteEvents
{
    public abstract class AstarteGenericIndividualEvent
    {
        private readonly String _interfaceName;
        private readonly String _path;
        private readonly Object? _value;

        public AstarteGenericIndividualEvent(String interfaceName, String path, Object? value)
        {
            _interfaceName = interfaceName;
            _path = path;
            _value = value;
        }

        public String GetInterfaceName()
        {
            return _interfaceName;
        }

        public String GetPath()
        {
            return _path;
        }

        public Object? GetValue()
        {
            return _value;
        }

        public String? GetValueString()
        {
            return (String?)_value;
        }
    }
}
