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
    public class AstarteAggregateDeviceTest : IAstarteAggregateDatastreamEventListener
    {
        private IAstarteDeviceMockData astarteMockData;
        private AstarteDevice astarteDevice;
        private AstarteMockDevice astarteMockDevice;
        private AstarteHttpRequestTest astarteHttpRequestTest;

        public AstarteAggregateDeviceTest()
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
        /// Test for aggregated object datastreams in the direction from device to server.
        /// </summary>
        [Fact, TestPriority(6)]
        public async Task AggregateFromDeviceToServer()
        {
            Console.WriteLine("Test for aggregated object datastreams"
            + " in the direction from device to server.");
            string interfaceName = astarteMockDevice.InterfaceDeviceAggr;

            AstarteDeviceAggregateDatastreamInterface astarteDeviceAggregateDatastream =
            (AstarteDeviceAggregateDatastreamInterface)astarteDevice
            .GetInterface(interfaceName);

            astarteDeviceAggregateDatastream
            .StreamData("/%{sensor_id}", astarteMockDevice.MockDataDictionary, DateTime.Now);
            Thread.Sleep(500);

            var response = await astarteHttpRequestTest.GetServerInterfaceAsync(interfaceName);

            Assert.NotEmpty(response);

            if (response != null)
            {

                dynamic jsonInfo;
                List<MockDataDevice> astarteAggregateData;
                try
                {
                    jsonInfo = JsonConvert.DeserializeObject<object>(response);
                    astarteAggregateData = JsonConvert.DeserializeObject
                    <List<MockDataDevice>>(jsonInfo.data["%{sensor_id}"].ToString());
                }
                catch (JsonException ex)
                {
                    throw new AstartePairingException(ex.Message, ex);
                }
                Assert.True(AstarteMockDevice.MockData.Equals(astarteAggregateData.First()));
            }

        }

        /// <summary>
        /// Test for aggregated object datastreams in the direction from server to device.
        /// </summary>
        /// <returns></returns>
        [Fact, TestPriority(5)]
        public async void AggregateFromServerToDevice()
        {
            Console.WriteLine("Test for aggregated object datastreams"
            + " in the direction from server to device.");
            string interfaceName = astarteMockDevice.InterfaceServerAggr;

            AstarteServerAggregateDatastreamInterface astarteServerAggregateDatastream =
            (AstarteServerAggregateDatastreamInterface)astarteDevice
            .GetInterface(interfaceName);

            astarteServerAggregateDatastream.AddListener(this);

            Dictionary<string, object> data = astarteMockDevice.MockDataDictionary;

            data.Remove("datetime_endpoint");
            data.Remove("datetimearray_endpoint");

            Thread.Sleep(500);

            await astarteHttpRequestTest
                .PostServerInterfaceAsync(
                interfaceName,
                "/sensorUuid",
                data);

        }

#pragma warning disable xUnit1013
        public void ValueReceived(AstarteAggregateDatastreamEvent e)
        {
            MockDataDevice astarteAggregateData;
            dynamic jsonInfo = JsonConvert.SerializeObject(e.GetValues());

            Dictionary<string, object> data = astarteMockDevice.MockDataDictionary;
            data["datetime_endpoint"] = new DateTime();
            data["datetimearray_endpoint"] = new DateTime[0];

            astarteAggregateData = JsonConvert.DeserializeObject
                    <MockDataDevice>(jsonInfo.ToString());


            Assert.True(AstarteMockDevice.MockData.Equals(astarteAggregateData));
        }
    }
}
