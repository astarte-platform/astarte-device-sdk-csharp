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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace AstarteDeviceSDKCSharpE2E.Tests.AstarteMockData
{
    public class AstarteMockDevice
    {
        public string Realm { get; }
        public string DeviceId { get; }
        public string CredentialsSecret { get; }
        public string ApiUrl { get; }
        public string AppEngineToken { get; }
        public string PairingUrl { get; }
        public string InterfacesDir { get; }

        public string InterfaceServerData { get; }
        public string InterfaceDeviceData { get; }
        public string InterfaceServerAggr { get; }
        public string InterfaceDeviceAggr { get; }
        public string InterfaceServerProp { get; }
        public string InterfaceDeviceProp { get; }

        public static MockDataDevice MockData { get; set; }

        public Dictionary<string, object> MockDataDictionary { get; }

        public AstarteMockDevice()
        {
            Realm = Environment.GetEnvironmentVariable("E2E_REALM",
            EnvironmentVariableTarget.Process);
            DeviceId = Environment.GetEnvironmentVariable("E2E_DEVICE_ID",
            EnvironmentVariableTarget.Process);
            CredentialsSecret = Environment.GetEnvironmentVariable("E2E_CREDENTIALS_SECRET",
            EnvironmentVariableTarget.Process);
            ApiUrl = Environment.GetEnvironmentVariable("E2E_API_URL",
            EnvironmentVariableTarget.Process);
            AppEngineToken = Environment.GetEnvironmentVariable("E2E_TOKEN",
            EnvironmentVariableTarget.Process);

            InterfacesDir = Path.Combine
            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                , "Resources", "standard-interfaces");

            if (string.IsNullOrEmpty(Realm) || string.IsNullOrEmpty(DeviceId) ||
                string.IsNullOrEmpty(CredentialsSecret) || string.IsNullOrEmpty(ApiUrl) ||
                string.IsNullOrEmpty(AppEngineToken))
            {
                throw new ArgumentException("Missing one of the environment variables");
            }

            PairingUrl = ApiUrl + "/pairing";

            InterfaceServerData = "org.astarte-platform.csharp.e2etest.ServerDatastream";
            InterfaceDeviceData = "org.astarte-platform.csharp.e2etest.DeviceDatastream";
            InterfaceServerAggr = "org.astarte-platform.csharp.e2etest.ServerAggregate";
            InterfaceDeviceAggr = "org.astarte-platform.csharp.e2etest.DeviceAggregate";
            InterfaceServerProp = "org.astarte-platform.csharp.e2etest.ServerProperty";
            InterfaceDeviceProp = "org.astarte-platform.csharp.e2etest.DeviceProperty";

            MockDataDictionary = new Dictionary<string, object>
            {
                { "double_endpoint", 5.4 },
                { "integer_endpoint", 42 },
                { "boolean_endpoint", true },
                { "longinteger_endpoint", 45543543534L },
                { "string_endpoint", "hello" },
                { "binaryblob_endpoint", new byte[] { 104, 101, 108, 108, 111 } },
                { "datetime_endpoint", new DateTime(2022, 11, 22, 10, 11, 21, DateTimeKind.Utc) },
                { "doublearray_endpoint", new double[] { 22.2, 322.22, 12.3, 0.1 } },
                { "integerarray_endpoint", new int[] { 22, 322, 0, 10 } },
                { "booleanarray_endpoint", new bool[] { true, false, true, false } },
                { "longintegerarray_endpoint", new long[] { 45543543534L, 10, 0, 45543543534L } },
                { "stringarray_endpoint", new string[] { "hello", " world" } },
                { "binaryblobarray_endpoint", new byte[][] { new byte[] { 104, 101, 108, 108, 111 },
                 new byte[] { 119, 111, 114, 108, 100 } } },
                { "datetimearray_endpoint", new DateTime[] { new DateTime(2022, 11, 22, 10, 11, 21,
                DateTimeKind.Utc), new DateTime(2022, 10, 21, 12, 5, 33, DateTimeKind.Utc) } },
            };

            MockData = new MockDataDevice(
            5.4,
            42,
            true,
            45543543534L,
            "hello",
            new byte[] { 104, 101, 108, 108, 111 },
            new DateTime(2022, 11, 22, 10, 11, 21, DateTimeKind.Utc),
            new double[] { 22.2, 322.22, 12.3, 0.1 },
            new int[] { 22, 322, 0, 10 },
            new bool[] { true, false, true, false },
            new long[] { 45543543534L, 10, 0, 45543543534L },
            new string[] { "hello", " world" },
            new byte[][] { new byte[] { 104, 101, 108, 108, 111 },
                            new byte[] { 119, 111, 114, 108, 100 } },
            new DateTime[] { new DateTime(2022, 11, 22, 10, 11, 21,
            DateTimeKind.Utc), new DateTime(2022, 10, 21, 12, 5, 33, DateTimeKind.Utc) }
            );

        }

    }

    public class MockDataDevice : IEquatable<MockDataDevice>
    {
        [JsonProperty("double_endpoint")]
        public double DoubleEndpoint { get; init; }

        [JsonProperty("integer_endpoint")]
        public int IntegerEndpoint { get; init; }

        [JsonProperty("boolean_endpoint")]
        public bool BooleanEndpoint { get; init; }

        [JsonProperty("longinteger_endpoint")]
        public long LongintegerEndpoint { get; init; }

        [JsonProperty("string_endpoint")]
        public string StringEndpoint { get; init; }

        [JsonProperty("binaryblob_endpoint")]
        public byte[] BinaryblobEndpoint { get; init; }

        [JsonProperty("datetime_endpoint")]
        public DateTime DatetimeEndpoint { get; init; }

        [JsonProperty("doublearray_endpoint")]
        public double[] DoublearrayEndpoint { get; init; }

        [JsonProperty("integerarray_endpoint")]
        public int[] IntegerarrayEndpoint { get; init; }

        [JsonProperty("booleanarray_endpoint")]
        public bool[] BooleanarrayEndpoint { get; init; }

        [JsonProperty("longintegerarray_endpoint")]
        public long[] LongintegerarrayEndpoint { get; init; }

        [JsonProperty("stringarray_endpoint")]
        public string[] StringarrayEndpoint { get; init; }

        [JsonProperty("binaryblobarray_endpoint")]
        public byte[][] BinaryblobarrayEndpoint { get; init; }

        [JsonProperty("datetimearray_endpoint")]
        public DateTime[] DatetimearrayEndpoint { get; init; }

        public MockDataDevice
        (double doubleEndpoint,
        int integerEndpoint,
        bool booleanEndpoint,
        long longintegerEndpoint,
        string stringEndpoint,
        byte[] binaryblobEndpoint,
        DateTime datetimeEndpoint,
        double[] doublearrayEndpoint,
        int[] integerarrayEndpoint,
        bool[] booleanarrayEndpoint,
        long[] longintegerarrayEndpoint,
        string[] stringarrayEndpoint,
        byte[][] binaryblobarrayEndpoint,
         DateTime[] datetimearrayEndpoint)
        {
            DoubleEndpoint = doubleEndpoint;
            IntegerEndpoint = integerEndpoint;
            BooleanEndpoint = booleanEndpoint;
            LongintegerEndpoint = longintegerEndpoint;
            StringEndpoint = stringEndpoint;
            BinaryblobEndpoint = binaryblobEndpoint;
            DatetimeEndpoint = datetimeEndpoint;
            DoublearrayEndpoint = doublearrayEndpoint;
            IntegerarrayEndpoint = integerarrayEndpoint;
            BooleanarrayEndpoint = booleanarrayEndpoint;
            LongintegerarrayEndpoint = longintegerarrayEndpoint;
            StringarrayEndpoint = stringarrayEndpoint;
            BinaryblobarrayEndpoint = binaryblobarrayEndpoint;
            DatetimearrayEndpoint = datetimearrayEndpoint;
        }

        public bool Equals(MockDataDevice other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.DatetimeEndpoint == new DateTime() || other.DatetimearrayEndpoint.Length == 0)
            {
                return
            other.IntegerEndpoint == this.IntegerEndpoint &&
            other.DoubleEndpoint == this.DoubleEndpoint &&
            other.LongintegerEndpoint == this.LongintegerEndpoint &&
            other.StringEndpoint == this.StringEndpoint &&
            other.BooleanEndpoint == this.BooleanEndpoint &&
            other.BinaryblobEndpoint.SequenceEqual(this.BinaryblobEndpoint) &&
            other.IntegerarrayEndpoint.SequenceEqual(this.IntegerarrayEndpoint) &&
            other.DoublearrayEndpoint.SequenceEqual(this.DoublearrayEndpoint) &&
            other.LongintegerarrayEndpoint.SequenceEqual(this.LongintegerarrayEndpoint) &&
            other.StringarrayEndpoint.SequenceEqual(this.StringarrayEndpoint) &&
            CompareByteArrays(other.BinaryblobarrayEndpoint) &&
            other.BooleanarrayEndpoint.SequenceEqual(BooleanarrayEndpoint);
            }

            return
            other.IntegerEndpoint == this.IntegerEndpoint &&
            other.DoubleEndpoint == this.DoubleEndpoint &&
            other.LongintegerEndpoint == this.LongintegerEndpoint &&
            other.StringEndpoint == this.StringEndpoint &&
            other.BooleanEndpoint == this.BooleanEndpoint &&
            DateTime.Compare(other.DatetimeEndpoint, this.DatetimeEndpoint) == 0 ? true : false &&
            other.BinaryblobEndpoint.SequenceEqual(this.BinaryblobEndpoint) &&
            other.IntegerarrayEndpoint.SequenceEqual(this.IntegerarrayEndpoint) &&
            other.DoublearrayEndpoint.SequenceEqual(this.DoublearrayEndpoint) &&
            other.LongintegerarrayEndpoint.SequenceEqual(this.LongintegerarrayEndpoint) &&
            other.StringarrayEndpoint.SequenceEqual(this.StringarrayEndpoint) &&
            CompareByteArrays(other.BinaryblobarrayEndpoint) &&
            other.DatetimearrayEndpoint.SequenceEqual(DatetimearrayEndpoint) &&
            other.BooleanarrayEndpoint.SequenceEqual(BooleanarrayEndpoint);

        }

        public override bool Equals(object obj) => Equals(obj as MockDataDevice);

        public override int GetHashCode() =>
        (DoubleEndpoint, DoublearrayEndpoint,
         IntegerEndpoint, IntegerarrayEndpoint,
         BooleanEndpoint, BooleanarrayEndpoint,
         LongintegerEndpoint, LongintegerarrayEndpoint,
         StringEndpoint, StringarrayEndpoint,
         DatetimeEndpoint, DatetimearrayEndpoint,
         BinaryblobEndpoint, BinaryblobarrayEndpoint)
        .GetHashCode();

        private bool CompareByteArrays(byte[][] arrayByte)
        {
            if (this.BinaryblobarrayEndpoint.Length != arrayByte.Length)
            {
                return false;
            }
            for (int i = 0; i < arrayByte.Length; i++)
            {
                if (arrayByte[i].Length != this.BinaryblobarrayEndpoint[i].Length)
                {
                    return false;
                }
                if (!(arrayByte[i].SequenceEqual(this.BinaryblobarrayEndpoint[i])))
                {
                    return false;
                }
            }
            return true;
        }

    }

}
