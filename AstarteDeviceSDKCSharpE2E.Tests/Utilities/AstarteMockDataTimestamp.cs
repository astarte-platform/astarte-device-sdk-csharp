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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AstarteDeviceSDKCSharpE2E.Tests.Utilities
{
    public class AstarteMockDataTimestamp
    {
        [JsonProperty("binaryblob_endpoint")]
        public BinaryblobEndpoint BinaryblobEndpoints { get; set; }

        [JsonProperty("binaryblobarray_endpoint")]
        public BinaryblobarrayEndpoint BinaryblobarrayEndpoints { get; set; }

        [JsonProperty("boolean_endpoint")]
        public BooleanEndpoint BooleanEndpoints { get; set; }

        [JsonProperty("booleanarray_endpoint")]
        public BooleanarrayEndpoint BooleanarrayEndpoints { get; set; }

        [JsonProperty("datetime_endpoint")]
        public DatetimeEndpoint DatetimeEndpoints { get; set; }

        [JsonProperty("datetimearray_endpoint")]
        public DatetimearrayEndpoint DatetimearrayEndpoints { get; set; }

        [JsonProperty("double_endpoint")]
        public DoubleEndpoint DoubleEndpoints { get; set; }

        [JsonProperty("doublearray_endpoint")]
        public DoublearrayEndpoint DoublearrayEndpoints { get; set; }

        [JsonProperty("integer_endpoint")]
        public IntegerEndpoint IntegerEndpoints { get; set; }

        [JsonProperty("integerarray_endpoint")]
        public IntegerarrayEndpoint IntegerarrayEndpoints { get; set; }

        [JsonProperty("longinteger_endpoint")]
        public LongintegerEndpoint LongintegerEndpoints { get; set; }

        [JsonProperty("longintegerarray_endpoint")]
        public LongintegerarrayEndpoint LongintegerarrayEndpoints { get; set; }

        [JsonProperty("string_endpoint")]
        public StringEndpoint StringEndpoints { get; set; }

        [JsonProperty("stringarray_endpoint")]
        public StringarrayEndpoint StringarrayEndpoints { get; set; }

        public class BinaryblobarrayEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public byte[][] value { get; set; }
        }

        public class BinaryblobEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public byte[] value { get; set; }
        }

        public class BooleanarrayEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public List<bool> value { get; set; }
        }

        public class BooleanEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public bool value { get; set; }
        }

        public class DatetimearrayEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public List<DateTime> value { get; set; }
        }

        public class DatetimeEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public DateTime value { get; set; }
        }

        public class DoublearrayEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public List<double> value { get; set; }
        }

        public class DoubleEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public double value { get; set; }
        }

        public class IntegerarrayEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public List<int> value { get; set; }
        }

        public class IntegerEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public int value { get; set; }
        }

        public class LongintegerarrayEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public List<long> value { get; set; }
        }

        public class LongintegerEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public long value { get; set; }
        }

        public class StringarrayEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public List<string> value { get; set; }
        }

        public class StringEndpoint
        {
            public DateTime reception_timestamp { get; set; }
            public DateTime timestamp { get; set; }
            public string value { get; set; }
        }
    }
}
