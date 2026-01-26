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
using AstarteDeviceSDKCSharp.Data;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using System;
using System.Collections.Generic;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests.Protocol
{
    public class AstarteDeviceAggregateDatastreamInterfaceFixture
    {
        #region json definitions
        public static string jsonWithArrayValues = "{\n"
                + "    \"interface_name\": \"com.astarte.ArrayTest\",\n"
                + "    \"version_major\": 0,\n"
                + "    \"version_minor\": 1,\n"
                + "    \"type\": \"datastream\",\n"
                + "    \"ownership\": \"device\",\n"
                + "    \"aggregation\": \"object\",\n"
                + "    \"mappings\": [\n"
                + "        {\n"
                + "            \"endpoint\": \"/test/int\",\n"
                + "            \"type\": \"integer\",\n"
                + "            \"database_retention_policy\": \"use_ttl\",\n"
                + "            \"database_retention_ttl\": 31536000, \n"
                + "            \"explicit_timestamp\": true\n"
                + "        },\n"
                + "        {\n"
                + "            \"endpoint\": \"/test/intArray\",\n"
                + "            \"type\": \"integerarray\",\n"
                + "            \"database_retention_policy\": \"use_ttl\",\n"
                + "            \"database_retention_ttl\": 31536000, \n"
                + "            \"explicit_timestamp\": true\n"
                + "        },\n"
                + "        {\n"
                + "            \"endpoint\": \"/test/doubleArray\",\n"
                + "            \"type\": \"doublearray\",\n"
                + "            \"database_retention_policy\": \"use_ttl\",\n"
                + "            \"database_retention_ttl\": 31536000, \n"
                + "            \"explicit_timestamp\": true\n"
                + "        }\n"
                + "    ]\n"
                + "}";

        public static string json = "{\n"
                + "    \"interface_name\": \"com.astarte.Test\",\n"
                + "    \"version_major\": 0,\n"
                + "    \"version_minor\": 1,\n"
                + "    \"type\": \"datastream\",\n"
                + "    \"ownership\": \"device\",\n"
                + "    \"aggregation\": \"object\",\n"
                + "    \"mappings\": [\n"
                + "        {\n"
                + "            \"endpoint\": \"/test/one\",\n"
                + "            \"type\": \"integer\",\n"
                + "            \"database_retention_policy\": \"use_ttl\",\n"
                + "            \"database_retention_ttl\": 31536000, \n"
                + "            \"explicit_timestamp\": true\n"
                + "        },\n"
                + "        {\n"
                + "            \"endpoint\": \"/test/two\",\n"
                + "            \"type\": \"integer\",\n"
                + "            \"database_retention_policy\": \"use_ttl\",\n"
                + "            \"database_retention_ttl\": 31536000, \n"
                + "            \"explicit_timestamp\": true\n"
                + "        }\n"
                + "    ]\n"
                + "}";

        #endregion

        public AstarteDeviceAggregateDatastreamInterface aInterface;
        public AstarteDeviceAggregateDatastreamInterface aInterfaceWArray;

        public AstarteDeviceAggregateDatastreamInterfaceFixture()
        {
            aInterface = (AstarteDeviceAggregateDatastreamInterface)AstarteInterface.FromString(json, new AstartePropertyStorage(""));
            aInterfaceWArray = (AstarteDeviceAggregateDatastreamInterface)AstarteInterface.FromString(jsonWithArrayValues, new AstartePropertyStorage(""));
        }
    }

    [CollectionDefinition("Astarte device aggregate datastream collection")]
    public class AstarteDeviceAggregateDatastreamInterfaceCollection : ICollectionFixture<AstarteDeviceAggregateDatastreamInterfaceFixture>
    { }

    [Collection("Astarte device aggregate datastream collection")]
    public class AstarteDeviceAggregateDatastreamInterfaceTest
    {
        private AstarteDeviceAggregateDatastreamInterface aInterface;
        private AstarteDeviceAggregateDatastreamInterface aInterfaceWArray;

        public AstarteDeviceAggregateDatastreamInterfaceTest(AstarteDeviceAggregateDatastreamInterfaceFixture fixture)
        {
            aInterface = fixture.aInterface;
            aInterfaceWArray = fixture.aInterfaceWArray;
        }

        [Fact]
        public void ValidateAggregateTest()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add("one", 1);
            payload.Add("two", 2);

            var exception = Record.Exception(() =>
            aInterface.ValidatePayload("/test", payload, new DateTime()));

            Assert.Null(exception);
        }

        [Fact]
        public void ValidateAggregateTooMuchPayloadTest()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add("one", 1);
            payload.Add("two", 2);
            payload.Add("three", 3);

            Assert.Throws<AstarteInterfaceMappingNotFoundException>(() =>
            aInterface.ValidatePayload("/test", payload, new DateTime()));
        }

        [Fact]
        public void ValidateAggregateTimestampRequiredTest()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add("one", 1);
            payload.Add("two", 2);

            Assert.Throws<AstarteInvalidValueException>(() =>
            aInterface.ValidatePayload("/test", payload, null));
        }

        [Fact]
        public void ValidateAggregateWithArraysTest()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add("int", 1);
            payload.Add("intArray", new int[] { 1, 2, -4 });
            payload.Add("doubleArray", new double[] { (double)2.0, (double)3.0, (double)-4.5 });

            var exception = Record.Exception(() =>
            aInterfaceWArray.ValidatePayload("/test", payload, new DateTime()));

            Assert.Null(exception);
        }

        [Fact]
        public void ValidateAggregateWithBadArraysTest()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add("int", 1);
            payload.Add("intArray", new int[] { 1, 2, -4 });
            payload.Add("doubleArray", new int[] { 1, 2, -4 });

            Assert.Throws<AstarteInvalidValueException>(() =>
            aInterfaceWArray.ValidatePayload("/test", payload, new DateTime()));
        }

        [Fact]
        public void ValidateAggregateWithMissingValueTest()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add("int", 1);
            payload.Add("intArray", new int[] { 1, 2, -4 });

            var exception = Record.Exception(() =>
            aInterfaceWArray.ValidatePayload("/test", payload, new DateTime()));

            Assert.Null(exception);
        }

    }
}
