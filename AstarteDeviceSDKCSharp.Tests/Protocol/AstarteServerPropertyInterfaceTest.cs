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
    internal class AstartePropertyClientTest : IAstartePropertyEventListener
    {
        void IAstartePropertyEventListener.PropertyReceived(AstartePropertyEvent e)
        {
            Assert.Equal("/enable", e.GetPath());
            Assert.Equal("org.test.Values", e.GetInterfaceName());
            Assert.Equal(true, e.GetValue());
        }

        void IAstartePropertyEventListener.PropertyUnset(AstartePropertyEvent e)
        {
            Assert.Equal("/enable", e.GetPath());
            Assert.Equal("org.test.Values", e.GetInterfaceName());
            Assert.Null(e.GetValue());
        }
    }

    public class AstarteServerPropertyInterfaceFixture
    {
        public static string interfaceName = "org.test.Values";
        private static readonly string prInterface =
            "{\n"
          + "    \"interface_name\": \""
          + interfaceName
          + "\",\n"
          + "    \"version_major\": 0,\n"
          + "    \"version_minor\": 1,\n"
          + "    \"type\": \"properties\",\n"
          + "    \"ownership\": \"server\",\n"
          + "    \"mappings\": [\n"
          + "        {\n"
          + "            \"endpoint\": \"/enable\",\n"
          + "            \"type\": \"boolean\",\n"
          + "        }\n"
          + "    ]\n"
          + "}\n";

        public AstarteServerPropertyInterface propertyInterface;

        public AstarteServerPropertyInterfaceFixture()
        {
            propertyInterface = (AstarteServerPropertyInterface)AstarteInterface.FromString(prInterface);
        }
    }

    [CollectionDefinition("Server property collection")]
    public class AstarteServerPropertyInterfaceCollection : ICollectionFixture<AstarteServerPropertyInterfaceFixture>
    { }

    [Collection("Server property collection")]
    public class AstarteServerPropertyInterfaceTest
    {
        private readonly AstarteServerPropertyInterface propertyInterface;
        public AstarteServerPropertyInterfaceTest(AstarteServerPropertyInterfaceFixture fixture)
        {
            propertyInterface = fixture.propertyInterface;
        }

        [Fact]
        public void BuildSuccessful()
        {
            AstarteServerValue astarteServerValue = propertyInterface.Build("/enable", true, new DateTime());

            Assert.NotNull(astarteServerValue);
            Assert.Equal("/enable", astarteServerValue.GetInterfacePath());
            Assert.Equal(true, astarteServerValue.GetValue());
        }

        [Fact]
        public void BuildNotSuccessful()
        {
            AstarteServerValue astarteServerValue = propertyInterface.Build("/enable1", true, new DateTime());

            Assert.Null(astarteServerValue);
        }

        [Fact]
        public void PublishPropertyReceivedSuccessful()
        {
            IAstartePropertyEventListener listener = new AstartePropertyClientTest();

            propertyInterface.AddListener(listener);
            AstarteServerValue astarteServerValue = propertyInterface.Build("/enable", true, new DateTime());

            propertyInterface.Publish(astarteServerValue);
            propertyInterface.RemoveListener(listener);
        }

        [Fact]
        public void PublishPropertyUnsetSuccessful()
        {
            IAstartePropertyEventListener listener = new AstartePropertyClientTest();

            propertyInterface.AddListener(listener);
            AstarteServerValue astarteServerValue = propertyInterface.Build("/enable", null, new DateTime());

            propertyInterface.Publish(astarteServerValue);
            propertyInterface.RemoveListener(listener);
        }
    }
}
