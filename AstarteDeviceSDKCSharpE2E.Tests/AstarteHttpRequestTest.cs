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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AstarteDeviceSDKCSharp.Device;
using AstarteDeviceSDKCSharpE2E.Tests.AstarteMockData;
using AstarteDeviceSDKCSharpE2E.Tests.Utilities;
using Newtonsoft.Json;

namespace AstarteDeviceSDKCSharpE2E.Tests
{
    public class AstarteHttpRequestTest
    {
        private IAstarteDeviceMockData astarteMockData;
        private AstarteMockDevice astarteMockDevice;
        private AstarteDevice astarteDevice;

        private HttpClient httpClient;

        public AstarteHttpRequestTest()
        {
            astarteMockData = new AstarteDeviceMockData();
            astarteMockDevice = astarteMockData.GetAstarteMockData();
            astarteDevice = astarteMockData.GetAstarteDevice();
            httpClient = new HttpClient();
        }

        public async Task<string> GetServerInterfaceAsync(string interfaces)
        {
            string httpQuery =
            astarteMockDevice.ApiUrl
            + "/appengine/v1/"
            + astarteMockDevice.Realm
            + "/devices/"
            + astarteMockDevice.DeviceId
            + "/interfaces/"
            + interfaces;

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", astarteMockDevice.AppEngineToken);

            var result = await httpClient.GetAsync(httpQuery);

            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }

            return String.Empty;

        }

        public async Task PostServerInterfaceAsync
        (string interfaces, string endpoint, object payload)
        {
            string httpQuery =
            astarteMockDevice.ApiUrl
            + "/appengine/v1/"
            + astarteMockDevice.Realm
            + "/devices/"
            + astarteMockDevice.DeviceId
            + "/interfaces/"
            + interfaces
            + endpoint;

            var json = new Dictionary<string, object>();
            json.Add("data", payload);

            var jsonBody = JsonConvert.SerializeObject(json);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization =
             new AuthenticationHeaderValue("Bearer", astarteMockDevice.AppEngineToken);

            var response = await httpClient.PostAsync(httpQuery, content);

            Thread.Sleep(1000);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Erorr sending request to Astrate."
                + response.StatusCode.ToString());
            }

        }

        public async Task DeleteServerInterfaceAsync(string interfaces, string endpoint)
        {
            string httpQuery =
            astarteMockDevice.ApiUrl
            + "/appengine/v1/"
            + astarteMockDevice.Realm
            + "/devices/"
            + astarteMockDevice.DeviceId
            + "/interfaces/"
            + interfaces
            + endpoint;

            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", astarteMockDevice.AppEngineToken);

            var response = await httpClient.DeleteAsync(httpQuery);

        }

    }
}
