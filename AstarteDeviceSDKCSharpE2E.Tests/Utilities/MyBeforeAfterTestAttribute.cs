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
using AstarteDeviceSDKCSharp.Device;
using Xunit.Sdk;

namespace AstarteDeviceSDKCSharpE2E.Tests.Utilities
{
    public class MyBeforeAfterTestAttribute : BeforeAfterTestAttribute
    {
        private IAstarteDeviceMockData astarteMockData;
        private AstarteDevice astarteDevice;

        public MyBeforeAfterTestAttribute()
        {
            astarteMockData = new AstarteDeviceMockData();
            astarteDevice = astarteMockData.GetAstarteDevice();
        }

        public override void Before(System.Reflection.MethodInfo methodUnderTest)
        {
            // This method will run before the test method
            // Perform setup actions here
            Console.WriteLine("Before test method");
            Thread.Sleep(500);
        }

    }
}
