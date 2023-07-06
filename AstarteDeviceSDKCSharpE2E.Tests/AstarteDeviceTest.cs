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
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharpE2E.Tests.AstarteMockData;
using AstarteDeviceSDKCSharpE2E.Tests.Utilities;
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer("AstarteDeviceSDKCSharpE2E.Tests.Utilities.PriorityOrderer", "AstarteDeviceSDKCSharpE2E.Tests")]

namespace AstarteDeviceSDKCSharpE2E.Tests;

[TestCaseOrderer(
    "AstarteDeviceSDKCSharpE2E.Tests.Utilities.PriorityOrderer",
    "AstarteDeviceSDKCSharpE2E.Tests")]
public class AstarteDeviceTest
{
    private IAstarteDeviceMockData astarteMockData;
    private AstarteDevice astarteDevice;
    private AstarteMockDevice astarteMockDevice;
    private AstarteHttpRequestTest astarteHttpRequestTest;

    public AstarteDeviceTest()
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
    /// Test connection device to Astarte
    /// </summary>
    [Fact, TestPriority(1)]
    public void ConnectDeviceToAstarte()
    {
        Console.WriteLine("Test connection device to Astarte");

        Assert.True(astarteDevice.IsConnected());
    }

    /// <summary>
    /// Test disconnect device from Astarte
    /// </summary>
    [Fact, TestPriority(9)]
    public void DisconnectDeviceFromAstarte()
    {
        Console.WriteLine("Test disconnect device to Astarte");

        astarteDevice.Disconnect();
        Assert.False(astarteDevice.IsConnected());
    }
}
