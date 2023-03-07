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
using AstarteDeviceSDKCSharp.Protocol.AstarteExeption;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests
{
    public class AstarteInterfaceTest
    {

        [Fact]
        public void TestSuccessfulFromJson()
        {
            string iface =
              "{\n"
              + "    \"interface_name\": \"org.astarte-platform.genericsensors.AvailableSensors\",\n"
              + "    \"version_major\": 0,\n"
              + "    \"version_minor\": 1,\n"
              + "    \"type\": \"properties\",\n"
              + "    \"ownership\": \"device\",\n"
              + "    \"description\": \"Describes available generic sensors.\",\n"
              + "    \"doc\": \"This interface allows to describe available sensors and their attributes " +
              "such as name and sampled data measurement unit. Sensors are identified by their sensor_id. " +
              "See also org.astarte-platform.genericsensors.AvailableSensors.\",\n"
              + "    \"mappings\": [\n"
              + "        {\n"
              + "            \"endpoint\": \"/%{sensor_id}/name\",\n"
              + "            \"type\": \"string\",\n"
              + "            \"description\": \"Sensor name.\",\n"
              + "            \"doc\": \"An arbitrary sensor name.\"\n"
              + "        },\n"
              + "        {\n"
              + "            \"endpoint\": \"/%{sensor_id}/unit\",\n"
              + "            \"type\": \"string\",\n"
              + "            \"description\": \"Sample data measurement unit.\",\n"
              + "            \"doc\": \"SI unit such as m, kg, K, etc...\"\n"
              + "        }\n"
              + "    ]\n"
              + "}\n";

            AstarteInterface astarteInterface = AstarteInterface.FromString(iface);

            Assert.Equal("org.astarte-platform.genericsensors.AvailableSensors", astarteInterface.GetInterfaceName());
            Assert.Equal(0, astarteInterface.GetMajorVersion());
            Assert.Equal(1, astarteInterface.GetMinorVersion());
        }

        [Fact]
        public void TestInvalidVersionFromJson()
        {
            string iface =
            "{\n"
            + "    \"interface_name\": \"org.astarte-platform.genericsensors.AvailableSensors\",\n"
            + "    \"version_major\": 0,\n"
            + "    \"version_minor\": 0,\n"
            + "    \"type\": \"properties\",\n"
            + "    \"ownership\": \"device\",\n"
            + "    \"description\": \"Describes available generic sensors.\",\n"
            + "    \"doc\": \"This interface allows to describe available sensors and their attributes such " +
            "as name and sampled data measurement unit. Sensors are identified by their sensor_id. " +
            "See also org.astarte-platform.genericsensors.AvailableSensors.\",\n"
            + "    \"mappings\": [\n"
            + "        {\n"
            + "            \"endpoint\": \"/%{sensor_id}/name\",\n"
            + "            \"type\": \"string\",\n"
            + "            \"description\": \"Sensor name.\",\n"
            + "            \"doc\": \"An arbitrary sensor name.\"\n"
            + "        },\n"
            + "        {\n"
            + "            \"endpoint\": \"/%{sensor_id}/unit\",\n"
            + "            \"type\": \"string\",\n"
            + "            \"description\": \"Sample data measurement unit.\",\n"
            + "            \"doc\": \"SI unit such as m, kg, K, etc...\"\n"
            + "        }\n"
            + "    ]\n"
            + "}\n";

            Assert.Throws<AstarteInvalidInterfaceException>(() => AstarteInterface.FromString(iface));
        }
    }
}
