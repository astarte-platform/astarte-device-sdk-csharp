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
    public class AstarteServerValue
    {
        private readonly Dictionary<string, object> _mapValue = new();
        private readonly object? _value;
        private readonly string _interfacePath = string.Empty;
        private readonly DateTime _timestamp;

        private AstarteServerValue(AstarteServerValueBuilder builder)
        {
            _mapValue = builder.mapValue;
            _interfacePath = builder.interfacePath;
            _timestamp = builder.timestamp;
            _value = builder.value;
        }

        public string GetInterfacePath()
        {
            return _interfacePath;
        }

        public DateTime GetTimestamp()
        {
            return _timestamp;
        }

        public object? GetValue()
        {
            return _value;
        }

        public Dictionary<string, object> GetMapValue()
        {
            return _mapValue;
        }

        public class AstarteServerValueBuilder
        {
            internal readonly Dictionary<string, object> mapValue = new();
            internal readonly object? value = null;
            internal string interfacePath = string.Empty;
            internal DateTime timestamp;

            public AstarteServerValueBuilder(object value)
            {
                this.value = value;
            }

            public AstarteServerValueBuilder(Dictionary<string, object> mapValue)
            {
                this.mapValue = mapValue;
            }

            public AstarteServerValueBuilder InterfacePath(string interfacePath)
            {
                this.interfacePath = interfacePath;
                return this;
            }

            public AstarteServerValueBuilder Timestamp(DateTime timeAstarteServerValuestamp)
            {
                this.timestamp = timeAstarteServerValuestamp;
                return this;
            }

            public AstarteServerValue? Build()
            {
                return new AstarteServerValue(this);
            }
        }
    }

}
