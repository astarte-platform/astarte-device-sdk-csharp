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
using Newtonsoft.Json;
using System;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests
{
    public class AstarteInterfaceMappingTest
    {
        [Fact]
        public void TypeInt()
        {
            string interf = "{\n" + "     \"endpoint\": \"/integer\",\n"
            + "     \"type\": \"integer\",\n" + "   }\n";
            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            Assert.Equal("/integer", mapping.Endpoint);
            Assert.Equal("integer", mapping.Type);

            AstarteInterfaceMapping astarteInterfaceMapping
             = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(int)));
        }

        [Fact]
        public void TypeBadInt()
        {
            string interf = "{\n" + "     \"endpoint\": \"/integer\",\n" +
            "     \"type\": \"integer\",\n" + "   }\n";
            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            Assert.Equal("/integer", mapping.Endpoint);
            Assert.Equal("integer", mapping.Type);

            AstarteInterfaceMapping astarteInterfaceMapping =
             AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeString()
        {
            String interf = "{\n" + "     \"endpoint\": \"/string\",\n" +
            "     \"type\": \"string\",\n" + "   }\n";
            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            Assert.Equal("/string", mapping.Endpoint);
            Assert.Equal("string", mapping.Type);

            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeBadString()
        {
            String interf = "{\n" + "     \"endpoint\": \"/string\",\n" +
             "     \"type\": \"string\",\n" + "   }\n";
            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            Assert.Equal("/string", mapping.Endpoint);
            Assert.Equal("string", mapping.Type);

            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(int)));
        }

        [Fact]
        public void TypeDouble()
        {
            string interf = "{\n" + "     \"endpoint\": \"/double\",\n" +
            "     \"type\": \"double\",\n" + "   }\n";
            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            Assert.Equal("/double", mapping.Endpoint);
            Assert.Equal("double", mapping.Type);

            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(double)));
        }

        [Fact]
        public void TypeBadDouble()
        {
            String interf = "{\n" + "     \"endpoint\": \"/double\",\n" +
            "     \"type\": \"double\",\n" + "   }\n";
            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            Assert.Equal("/double", mapping.Endpoint);
            Assert.Equal("double", mapping.Type);

            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeLongInteger()
        {
            string interf =
            "{\n"
            + "     \"endpoint\": \"/longinteger\",\n"
            + "     \"type\": \"longinteger\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(long)));
        }

        [Fact]
        public void TypeBadLongInteger()
        {
            string interf =
            "{\n"
            + "     \"endpoint\": \"/longinteger\",\n"
            + "     \"type\": \"longinteger\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeBool()
        {
            string interf = "{\n" + "     \"endpoint\": \"/boolean\",\n" +
             "     \"type\": \"boolean\",\n" + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(bool)));
        }

        [Fact]
        public void TypeBadBool()
        {
            string interf = "{\n" + "     \"endpoint\": \"/boolean\",\n" + "     \"type\": \"boolean\",\n" + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeBinaryBlob()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/binaryblob\",\n"
            + "     \"type\": \"binaryblob\",\n"
            + "   }\n"; ;

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(byte[])));
        }

        [Fact]
        public void TypeBadBinaryBlob()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/binaryblob\",\n"
            + "     \"type\": \"binaryblob\",\n"
            + "   }\n"; ;

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeDateTime()
        {
            string interf = "{\n" + "     \"endpoint\": \"/datetime\",\n" + "     \"type\": \"datetime\",\n" + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(DateTime)));
        }

        [Fact]
        public void TypeBadDateTime()
        {
            string interf = "{\n" + "     \"endpoint\": \"/datetime\",\n" + "     \"type\": \"datetime\",\n" + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void ValidateDouble()
        {
            string interf = "{\n" + " \"endpoint\": \"/double\",\n" + " \"type\": \"double\",\n" + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);

            astarteInterfaceMapping.ValidatePayload((double)3.2);
        }

        [Fact]
        public void ValidateNanDouble()
        {
            string interf = "{\n" + " \"endpoint\": \"/double\",\n" + "     \"type\": \"double\",\n" + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);

            try
            {
                astarteInterfaceMapping.ValidatePayload(double.NaN);
            }
            catch (AstarteInvalidValueException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Fact]
        public void TypeIntArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/integer\",\n"
            + "     \"type\": \"integerarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(int[])));
        }

        [Fact]
        public void TypeBadIntArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/integer\",\n"
            + "     \"type\": \"integerarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping = AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeStringArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/string\",\n"
            + "     \"type\": \"stringarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(string[])));
        }

        [Fact]
        public void TypeBadStringArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/string\",\n"
            + "     \"type\": \"stringarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeDoubleArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/double\",\n"
            + "     \"type\": \"doublearray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(double[])));
        }

        [Fact]
        public void TypeBadDoubleArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/double\",\n"
            + "     \"type\": \"doublearray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeLongIntegerArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/longinteger\",\n"
            + "     \"type\": \"longintegerarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(long[])));
        }

        [Fact]
        public void TypeBadLongIntegerArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/longinteger\",\n"
            + "     \"type\": \"longintegerarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeBoolArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/boolean\",\n"
            + "     \"type\": \"booleanarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(bool[])));
        }

        [Fact]
        public void TypeBadBoolArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/boolean\",\n"
            + "     \"type\": \"booleanarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(string)));
        }

        [Fact]
        public void TypeBinaryBlobArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/binaryblob\",\n"
            + "     \"type\": \"binaryblobarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(byte[][])));
        }

        [Fact]
        public void TypeBadBinaryBlobArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/binaryblob\",\n"
            + "     \"type\": \"binaryblobarray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(byte[])));
        }

        [Fact]
        public void TypeDatetimeArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/datetime\",\n"
            + "     \"type\": \"datetimearray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.True(astarteInterfaceMapping.IsTypeCompatible(typeof(DateTime[])));
        }

        [Fact]
        public void TypeBadDatetimeArray()
        {
            string interf = "{\n"
            + "     \"endpoint\": \"/datetime\",\n"
            + "     \"type\": \"datetimearray\",\n"
            + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            Assert.False(astarteInterfaceMapping.IsTypeCompatible(typeof(DateTime)));
        }

        [Fact]
        public void ValidateDoubleArray()
        {
            string interf = "{\n"
                    + "     \"endpoint\": \"/double\",\n"
                    + "     \"type\": \"doublearray\",\n"
                    + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            object payload = new double[] { 3.0, 1.0 };

            astarteInterfaceMapping.ValidatePayload(payload);
        }

        [Fact]
        public void ValidateNaNDoubleArray()
        {
            string interf = "{\n"
                    + "     \"endpoint\": \"/double\",\n"
                    + "     \"type\": \"doublearray\",\n"
                    + "   }\n";

            Mapping mapping = JsonConvert.DeserializeObject<Mapping>(interf);
            AstarteInterfaceMapping astarteInterfaceMapping =
            AstarteInterfaceMapping.FromAstarteInterfaceMapping(mapping);
            object payload = new double[] { 3.0, double.NaN };

            Assert.Throws<AstarteInvalidValueException>(() =>
            astarteInterfaceMapping.ValidatePayload(payload));

        }
    }
}
