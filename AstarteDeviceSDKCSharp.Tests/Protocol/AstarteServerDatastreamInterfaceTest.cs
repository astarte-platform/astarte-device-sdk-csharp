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
using System;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests.Protocol
{
    internal class AstarteClientTest : IAstarteDatastreamEventListener
    {
        public void ValueReceived(AstarteDatastreamEvent e)
        {
            Assert.Equal("/value", e.GetPath());
            Assert.Equal("org.test.Values", e.GetInterfaceName());
            Assert.Equal(10.6, e.GetValue());
        }
    }

    public class InterfaceFixture
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
          + "    \"mappings\": [\n"
          + "        {\n"
          + "            \"endpoint\": \"/value\",\n"
          + "            \"type\": \"double\",\n"
          + "            \"explicit_timestamp\": true,\n"
          + "        }\n"
          + "    ]\n"
          + "}\n";

        public AstarteServerDatastreamInterface datastreamInterface;

        public InterfaceFixture()
        {
            datastreamInterface = (AstarteServerDatastreamInterface)AstarteInterface.FromString(dtInterface);
        }
    }

    [CollectionDefinition("Interface collection")]
    public class InterfaceCollection : ICollectionFixture<InterfaceFixture>
    { }

    [Collection("Interface collection")]
    public class AstarteServerDatastreamInterfaceTest
    {
        private readonly AstarteServerDatastreamInterface datastreamInterface;
        public AstarteServerDatastreamInterfaceTest(InterfaceFixture interfaceFixture)
        {
            datastreamInterface = interfaceFixture.datastreamInterface;
        }

        [Fact]
        public void BuildSuccessful()
        {
            AstarteServerValue astarteServerValue = datastreamInterface.Build("/value", 10.6, new DateTime());

            Assert.NotNull(astarteServerValue);
            Assert.Equal("/value", astarteServerValue.GetInterfacePath());
            Assert.Equal(10.6, astarteServerValue.GetValue());
        }

        [Fact]
        public void PublishSuccessful()
        {
            IAstarteDatastreamEventListener listener = new AstarteClientTest();
            datastreamInterface.AddListener(listener);
            AstarteServerValue astarteServerValue = datastreamInterface.Build("/value", 10.6, new DateTime());
            datastreamInterface.Publish(astarteServerValue);
            datastreamInterface.RemoveListener(listener);
        }
    }
}
