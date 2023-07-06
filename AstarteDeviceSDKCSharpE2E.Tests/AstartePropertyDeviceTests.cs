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
    public class AstartePropertyDeviceTests : IAstartePropertyEventListener
    {
        private IAstarteDeviceMockData astarteMockData;
        private AstarteDevice astarteDevice;
        private AstarteMockDevice astarteMockDevice;
        private AstarteHttpRequestTest astarteHttpRequestTest;

        public AstartePropertyDeviceTests()
        {
            astarteMockData = new AstarteDeviceMockData();
            astarteDevice = astarteMockData.GetAstarteDevice();
            astarteMockDevice = astarteMockData.GetAstarteMockData();
            astarteHttpRequestTest = new();
        }

        [Fact]
        [MyBeforeAfterTest]
        public async void TestBeforeShouldRunBeforeTest()
        {
            await astarteDevice.Connect();
        }

        /// <summary>
        /// Test for individual properties in the direction from device to server.
        /// </summary>
        /// <returns></returns>
        [Fact, TestPriority(3)]
        public async Task PropertiesFromDeviceToServer()
        {
            Console.WriteLine("Test for individual properties "
            + "in the direction from device to server.");

            string interfaceName = astarteMockDevice.InterfaceDeviceProp;

            AstarteDevicePropertyInterface astarteDeviceProperty =
                    (AstarteDevicePropertyInterface)astarteDevice
                    .GetInterface(interfaceName);

            foreach (var data in astarteMockDevice.MockDataDictionary)
            {
                astarteDeviceProperty.SetProperty($"/{"sensorUuid"}/{data.Key}", data.Value);
                Thread.Sleep(500);
            }

            var response = await astarteHttpRequestTest.GetServerInterfaceAsync(interfaceName);

            Assert.NotEmpty(response);

            if (response != null)
            {

                dynamic jsonInfo;
                MockDataDevice astartePropDevice;

                try
                {
                    jsonInfo = JsonConvert.DeserializeObject<object>(response);
                    astartePropDevice = JsonConvert.DeserializeObject
                    <MockDataDevice>(jsonInfo.data.sensorUuid.ToString());
                }
                catch (JsonException ex)
                {
                    throw new AstartePairingException(ex.Message, ex);
                }
                Assert.True(AstarteMockDevice.MockData.Equals(astartePropDevice));
            }
        }

        /// <summary>
        /// Test for individual properties in the direction from server to device.
        /// </summary>
        /// <returns></returns>
        [Fact, TestPriority(4)]
        public async Task PropertiesFromServerToDevice()
        {
            Console.WriteLine("Test for individual properties in"
            + " the direction from server to device.");
            string interfaceName = astarteMockDevice.InterfaceServerProp;

            AstarteServerPropertyInterface astarteServerProperty =
                   (AstarteServerPropertyInterface)astarteDevice
                   .GetInterface(interfaceName);

            astarteServerProperty.AddListener(this);

            foreach (var data in astarteMockDevice.MockDataDictionary)
            {
                await astarteHttpRequestTest
                .PostServerInterfaceAsync(interfaceName, $"/{"sensorUuid"}/{data.Key}", data.Value);
                Thread.Sleep(500);
            }

            foreach (var data in astarteMockDevice.MockDataDictionary)
            {
                await astarteHttpRequestTest
                .DeleteServerInterfaceAsync(interfaceName, $"/{"sensorUuid"}/{data.Key}");
                Thread.Sleep(500);
            }
        }
#pragma warning disable xUnit1013
        public void PropertyReceived(AstartePropertyEvent e)
        {
            foreach (var item in astarteMockDevice.MockDataDictionary)
            {
                if (item.Key == e.GetPath().Remove(0, "/sensorUuid/".Length))
                {
                    if (item.Value is Array array && e.GetValue() is Array otherArray)
                    {
                        Assert.False(array == null || otherArray == null);
                        Assert.False((array.Length != otherArray.Length));
                        Assert.True(ArrayEquality(array, otherArray));
                    }
                }
            }
        }

#pragma warning disable xUnit1013
        public void PropertyUnset(AstartePropertyEvent e)
        {
            Console.WriteLine("Receiving message for unset variable " + e.GetValue());
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
