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

using AstarteDeviceSDKCSharp.Utilities;
using Microsoft.IdentityModel.Tokens;
using System;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests
{
    public class AstarteDeviceIdUtilsTest
    {

        [Fact]
        public void TestGenerateIdFromGuidDefault()
        {
            Guid nameUUID = new("f79ad91f-c638-4889-ae74-9d001a3b4cf8");
            string hardwareId = "myidentifierdata";

            string expectedDeviceId = "AJInS0w3VpWpuOqkXhgZdA";
            string deviceId = AstarteDeviceIdUtils.GenerateId(nameUUID, hardwareId);
            Assert.Equal(expectedDeviceId, deviceId);
        }

        [Fact]
        public void TestGenerateIdFromGuid()
        {
            Guid nameUUID = new("f79ad91f-c638-4889-ae74-9d001a3b4cf8");
            string hardwareId = "0099112233";

            string expectedDeviceId = "L7HnZkE2Ur2rOcWN9VvjFg";
            string deviceId = AstarteDeviceIdUtils.GenerateId(nameUUID, hardwareId);
            Assert.Equal(expectedDeviceId, deviceId);
        }

        [Fact]
        public void TestGenerateUUID()
        {
            string astarteDeviceId = AstarteDeviceIdUtils.GenerateId();
            byte[]? astarteDecodedId;
            try
            {
                astarteDecodedId = Base64UrlEncoder.DecodeBytes(astarteDeviceId);
            }
            catch
            {
                astarteDecodedId = null;
            }

            Assert.NotNull(astarteDecodedId);
            Assert.True(astarteDecodedId == null ? false : astarteDecodedId.Length > 0);
        }

    }
}
