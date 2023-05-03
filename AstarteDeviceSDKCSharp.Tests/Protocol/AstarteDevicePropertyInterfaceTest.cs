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
using AstarteDeviceSDKCSharp.Protocol.AstarteException;
using System;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests.Protocol
{
    public class AstarteDevicePropertyInterfaceFixture
    {
        public AstarteDevicePropertyInterface aInterface;
        public AstarteDevicePropertyInterfaceFixture()
        {
            string json = "{\n"
                + "    \"interface_name\": \"com.astarte.Test\",\n"
                + "    \"version_major\": 0,\n"
                + "    \"version_minor\": 1,\n"
                + "    \"type\": \"properties\",\n"
                + "    \"ownership\": \"device\",\n"
                + "    \"mappings\": [\n"
                + "        {\n"
                + "            \"endpoint\": \"/test/uno\",\n"
                + "            \"type\": \"integer\",\n"
                + "            \"database_retention_policy\": \"use_ttl\",\n"
                + "            \"database_retention_ttl\": 31536000 \n"
                + "        },\n"
                + "        {\n"
                + "            \"endpoint\": \"/test/due\",\n"
                + "            \"type\": \"integer\",\n"
                + "            \"database_retention_policy\": \"use_ttl\",\n"
                + "            \"database_retention_ttl\": 31536000 \n"
                + "        }\n"
                + "    ]\n"
                + "}";
            aInterface = (AstarteDevicePropertyInterface)AstarteInterface.FromString(json);
        }
    }

    [CollectionDefinition("Astarte Device Property Interface Collection")]
    public class AstarteDevicePropertyInterfaceCollection : ICollectionFixture<AstarteDevicePropertyInterfaceFixture>
    { }

    [Collection("Astarte Device Property Interface Collection")]
    public class AstarteDevicePropertyInterfaceTest
    {
        private readonly AstarteDevicePropertyInterface aInterface;

        public AstarteDevicePropertyInterfaceTest(AstarteDevicePropertyInterfaceFixture fixture)
        {
            aInterface = fixture.aInterface;
        }

        [Fact]
        public void ValidatePropertyTest()
        {
            aInterface.ValidatePayload("/test/uno", 1, null);
        }

        [Fact]
        public void ValidatePropertyInvalidValueTest()
        {
            Assert.Throws<AstarteInvalidValueException>(() =>
            aInterface.ValidatePayload("/test/uno", 1.0, null));
        }

        [Fact]
        public void ValidateAbsentPropertyTest()
        {
            Assert.Throws<AstarteInterfaceMappingNotFoundException>(() =>
            aInterface.ValidatePayload("/test/tre", 3, null));
        }

        [Fact]
        public void ValidatePropertyTimestampTest()
        {
            Assert.Throws<AstarteInvalidValueException>(() =>
            aInterface.ValidatePayload("/test/uno", 1.0, null));
        }
    }
}
