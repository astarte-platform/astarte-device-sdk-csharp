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
using System.Threading;
using System.Threading.Tasks;
using AstarteDeviceSDKCSharp;
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharp.Protocol;
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using AstarteDeviceSDKCSharpE2E.Tests.AstarteMockData;
using AstarteDeviceSDKCSharpE2E.Tests.Utilities;
using Newtonsoft.Json;
using Xunit;


namespace AstarteDeviceSDKCSharpE2E.Tests
{
    [TestCaseOrderer(
        "AstarteDeviceSDKCSharpE2E.Tests.Utilities.PriorityOrderer",
        "AstarteDeviceSDKCSharpE2E.Tests")]
    public class AstarteDatastreamDeviceTests : IAstarteDatastreamEventListener
    {
        private IAstarteDeviceMockData astarteMockData;
        private AstarteDevice astarteDevice;
        private AstarteMockDevice astarteMockDevice;
        private AstarteHttpRequestTest astarteHttpRequestTest;

        public AstarteDatastreamDeviceTests()
        {
            astarteMockData = new AstarteDeviceMockData();
            astarteDevice = astarteMockData.GetAstarteDevice();
            astarteMockDevice = astarteMockData.GetAstarteMockData();
            astarteHttpRequestTest = new();
        }

        [Fact]
        [MyBeforeAfterTestAttribute]
        public async void TestBeforeShouldRunBeforeTest()
        {
            await astarteDevice.Connect();
        }

        /// <summary>
        /// Test for individual datastreams in the direction from device to server.
        /// </summary>
        /// <returns></returns>
        [Fact, TestPriority(2)]
        public async Task DatastreamFromDeviceToServer()
        {
            string interfaceName = astarteMockDevice.InterfaceDeviceData;
            Console.WriteLine("Test for individual datastreams"
            + " in the direction from device to server.");

            AstarteDeviceDatastreamInterface astarteDatastreamInterface =
            (AstarteDeviceDatastreamInterface)astarteDevice
            .GetInterface(interfaceName);

            Thread.Sleep(1000);

            foreach (var item in astarteMockDevice.MockDataDictionary)
            {
                astarteDatastreamInterface.StreamData($"/{item.Key}", item.Value, DateTime.Now);
                Thread.Sleep(500);
            }

            var response = await astarteHttpRequestTest.GetServerInterfaceAsync(interfaceName);

            Assert.NotEmpty(response);

            if (response != null)
            {
                dynamic jsonInfo;
                AstarteMockDataTimestamp astartePropDevice;

                try
                {
                    jsonInfo = JsonConvert.DeserializeObject<object>(response);
                    astartePropDevice = JsonConvert.DeserializeObject
                    <AstarteMockDataTimestamp>(jsonInfo.data.ToString());
                }
                catch (JsonException ex)
                {
                    throw new AstartePairingException(ex.Message, ex);
                }
                Assert.True(AstarteMockDevice.MockData.IntegerEndpoint
                .Equals(astartePropDevice.IntegerEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.DoubleEndpoint
                .Equals(astartePropDevice.DoubleEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.StringEndpoint
                .Equals(astartePropDevice.StringEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.LongintegerEndpoint
                .Equals(astartePropDevice.LongintegerEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.BooleanEndpoint
                .Equals(astartePropDevice.BooleanEndpoints.value));

                Assert.True(DateTime.Compare
                (AstarteMockDevice.MockData.DatetimeEndpoint,
                astartePropDevice.DatetimeEndpoints.value) == 0 ? true : false);

                Assert.True(AstarteMockDevice.MockData.IntegerarrayEndpoint
                .SequenceEqual(astartePropDevice.IntegerarrayEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.DoublearrayEndpoint
                .SequenceEqual(astartePropDevice.DoublearrayEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.LongintegerarrayEndpoint
                .SequenceEqual(astartePropDevice.LongintegerarrayEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.BooleanarrayEndpoint
                .SequenceEqual(astartePropDevice.BooleanarrayEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.StringarrayEndpoint
                .SequenceEqual(astartePropDevice.StringarrayEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.DatetimearrayEndpoint
                .SequenceEqual(astartePropDevice.DatetimearrayEndpoints.value));

                Assert.True(AstarteMockDevice.MockData.BinaryblobEndpoint
                .SequenceEqual(astartePropDevice.BinaryblobEndpoints.value));

                Assert.True(ArrayEquality(AstarteMockDevice.MockData.BinaryblobarrayEndpoint
                , astartePropDevice.BinaryblobarrayEndpoints.value));

            }

        }

        /// <summary>
        /// Test for individual datastreams in the direction from server to device.
        /// </summary>
        /// <returns></returns>
        [Fact, TestPriority(1)]
        public async Task DatastreamFromServerToDevice()
        {
            string interfaceName = astarteMockDevice.InterfaceServerData;
            Console.WriteLine("Test for individual datastreams"
            + " in the direction from server to device.");

            AstarteServerDatastreamInterface astarteServerDatastreamInterface =
            (AstarteServerDatastreamInterface)astarteDevice
            .GetInterface(interfaceName);

            astarteServerDatastreamInterface.AddListener(this);

            foreach (var data in astarteMockDevice.MockDataDictionary)
            {
                await astarteHttpRequestTest
                .PostServerInterfaceAsync(interfaceName, $"/{data.Key}", data.Value);
                Thread.Sleep(1000);
            }

        }

#pragma warning disable xUnit1013
        public void ValueReceived(AstarteDatastreamEvent e)
        {
            foreach (var item in astarteMockDevice.MockDataDictionary)
            {
                if ($"/{item.Key}" == e.GetPath())
                {
                    if (item.Value is Array array && e.GetValue() is Array otherArray)
                    {
                        Assert.False(array == null || otherArray == null);
                        Assert.False((array.Length != otherArray.Length));
                        Assert.True(ArrayEquality(array, otherArray));
                    }

                    Assert.False(item.GetHashCode().Equals(e.GetValue().GetHashCode()));
                }
            }
        }

        private bool ArrayEquality(Array array, Array otherArray)
        {

            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) == null || otherArray.GetValue(i) == null) return false;

                if (array.GetValue(i) is Array arrayOfArray
                && otherArray.GetValue(i) is Array arrayOfOtherArray)
                {
                    if (!ArrayEquality(arrayOfArray, arrayOfOtherArray))
                    {
                        return false;
                    }
                }
                else if (!array.GetValue(i)!.GetHashCode()
                .Equals(otherArray.GetValue(i)!.GetHashCode()))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
