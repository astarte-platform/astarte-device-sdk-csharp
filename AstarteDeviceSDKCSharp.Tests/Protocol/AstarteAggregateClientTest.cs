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
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests.Protocol
{
    internal class AstarteAggregateClientTest : IAstarteAggregateDatastreamEventListener
    {
        public void ValueReceived(AstarteAggregateDatastreamEvent e)
        {
            Dictionary<string, object> rValues = e.GetValues();
            Assert.Equal("org.test.Values", e.GetInterfaceName());
            Assert.True(rValues.Count > 0);
            Assert.Equal(10.6, rValues["value"]);
            Assert.Equal("build", rValues["name"]);
        }
    }

    public class AggregateInterfaceFixture
    {
        public static string interfaceName = "org.test.Values";

        public string dtInterface = "{\n"
          + "    \"interface_name\": \""
          + interfaceName
          + "\",\n"
          + "    \"version_major\": 0,\n"
          + "    \"version_minor\": 1,\n"
          + "    \"type\": \"datastream\",\n"
          + "    \"ownership\": \"server\",\n"
          + "    \"aggregation\": \"object\",\n"
          + "    \"mappings\": [\n"
          + "        {\n"
          + "            \"endpoint\": \"/test/value\",\n"
          + "            \"type\": \"double\",\n"
          + "        },\n"
          + "        {\n"
          + "            \"endpoint\": \"/test/name\",\n"
          + "            \"type\": \"string\",\n"
          + "        }\n"
          + "    ]\n"
          + "}\n";

        public AstarteServerAggregateDatastreamInterface datastreamInterface;

        public AggregateInterfaceFixture()
        {
            datastreamInterface = (AstarteServerAggregateDatastreamInterface)AstarteInterface.FromString(dtInterface);
        }
    }

    [CollectionDefinition("Aggregate Interface collection")]
    public class AggregateInterfaceCollection : ICollectionFixture<AggregateInterfaceFixture>
    { }

    public class TestClass
    {
        public object value { get; set; }
        public string name { get; set; }
    }

    [Collection("Aggregate Interface collection")]
    public class AstarteServerAggregateDatastreamInterfaceTest
    {
        private readonly AstarteServerAggregateDatastreamInterface datastreamInterface;

        public AstarteServerAggregateDatastreamInterfaceTest(AggregateInterfaceFixture interfaceFixture)
        {
            datastreamInterface = interfaceFixture.datastreamInterface;
        }

        [Fact]
        public void BuildSuccessful()
        {
            TestClass mock = new TestClass
            {
                value = 10.6,
                name = "build"
            };

            AstarteServerValue astarteServerValue = datastreamInterface.Build("/test", mock, new DateTime());

            Assert.NotNull(astarteServerValue);
            Assert.Equal("/test", astarteServerValue.GetInterfacePath());

            Dictionary<string, object> rValues = astarteServerValue.GetMapValue();
            Assert.True(rValues.Count > 0);
            Assert.Equal(10.6, rValues["value"]);
            Assert.Equal("build", rValues["name"]);
        }

        [Fact]
        public void BuildNotSuccessful()
        {
            AstarteServerValue astarteServerValue = datastreamInterface.Build("/value2", null, new DateTime());
            Assert.Null(astarteServerValue);
        }

        [Fact]
        public void PublishSuccessful()
        {
            TestClass mock = new TestClass
            {
                value = 10.6,
                name = "build"
            };

            IAstarteAggregateDatastreamEventListener listener = new AstarteAggregateClientTest();
            datastreamInterface.AddListener(listener);

            AstarteServerValue astarteServerValue = datastreamInterface.Build("/test", mock, new DateTime());

            datastreamInterface.Publish(astarteServerValue);
            datastreamInterface.RemoveListener(listener);
        }
    }
}
