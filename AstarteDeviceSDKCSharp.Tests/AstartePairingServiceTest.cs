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

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests
{
    public class AstartePairingServiceTest
    {

        readonly WireMockServer _server = WireMockServer.Start();
        AstartePairingService _service;

        [Fact]
        public async Task TestThrowExceptionOnUnsuccessfulRegisterDeviceWithJwtTokenAsync()
        {
            // Arrange (start WireMock.Net server)
            _server
              .Given(Request.Create().WithPath($"/v1/test/agent/devices").UsingPost())
              .RespondWith(
                Response.Create()
                  .WithStatusCode(500)
                  .WithHeader("Content-Type", "application/json; charset=utf-8")
              );

            _service = new AstartePairingService($"{_server.Urls[0]}", "test");
            //Assert
            _ = await Assert.ThrowsAsync<AstartePairingException>(() =>
            //Act
            _service.RegisterDeviceWithJwtToken("001122334455", "token1"));
        }

        [Fact]
        public async Task TestThrowExceptionOnEmptyResponseDeviceWithJwtTokenAsync()
        {
            // Arrange (start WireMock.Net server)
            _server
              .Given(Request.Create().WithPath($"/v1/test/agent/devices").UsingPost())
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithHeader("Content-Type", "application/json; charset=utf-8")
                  .WithBody("{\n"
                    + "  \"data\": {\n"
                    + "    \"credentials_secret\": \""
                    + ""
                    + "\"\n"
                    + "  }\n"
                    + "}")
              ); ;

            _service = new AstartePairingService($"{_server.Urls[0]}", "test");

            //Assert
            _ = await Assert.ThrowsAsync<AstartePairingException>(() =>
          _service.RegisterDeviceWithJwtToken("001122334455", "token1"));
        }

        [Fact]
        public async Task TestSuccessfullRegisterDeviceWithJwtTokenAsync()
        {
            string expectedCredentialSecret = "TTkd5OgB13X/3qU0LXU7OCxyTXz5QHM2NY1IgidtPOs=";
            string expectedRegisterPath = "/v1/test/agent/devices";
            string deviceId = "YHjKs3SMTgqq09eD7fzm6w";

            JObject expectedRequestBody;

            expectedRequestBody = new(
                            new JProperty("data",
                                new JObject(
                                    new JProperty("hw_id", deviceId)
                                    )
                                ));

            string responseBody =
                "{\n"
                    + "  \"data\": {\n"
                    + "    \"credentials_secret\": \""
                    + expectedCredentialSecret
                    + "\"\n"
                    + "  }\n"
                    + "}";

            // Arrange (start WireMock.Net server)
            _server
              .Given(Request.Create().WithPath($"/v1/test/agent/devices").UsingPost())
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithHeader("Content-Type", "application/json; charset=utf-8")
                  .WithBody(responseBody)
              );
            _service = new AstartePairingService($"{_server.Urls[0]}", "test");

            //Act
            string credentialSecret = await _service.RegisterDeviceWithJwtToken(deviceId, expectedCredentialSecret);

            var customerReadRequests = _server.FindLogEntries(
                        Request.Create().WithPath("/v1/test/agent/devices").UsingPost()
                    );

            //Assert
            string registerPath;
            JObject requestBody;
            foreach (var item in customerReadRequests)
            {
                registerPath = item.RequestMessage.Path;
                requestBody = (JObject)item.RequestMessage.BodyData.BodyAsJson;
                Assert.Equal(expectedRequestBody, requestBody);
                Assert.Equal(expectedRegisterPath, registerPath);
            }

            Assert.Equal(expectedCredentialSecret, credentialSecret);
        }

        [Fact]
        public async Task TestThrowExceptionOnUnsuccessfulRegisterDeviceWithPrivateKeyAsync()
        {
            // Arrange (start WireMock.Net server)
            _server
              .Given(Request.Create().WithPath($"/v1/test/agent/devices").UsingPost())
              .RespondWith(
                Response.Create()
                  .WithStatusCode(500)
                  .WithHeader("Content-Type", "application/json; charset=utf-8")
              );

            _service = new AstartePairingService($"{_server.Urls[0]}", "test");
            //Assert
            _ = await Assert.ThrowsAsync<AstartePairingException>(() =>
            //Act
            _service.RegisterDeviceWithPrivateKey("001122334455", "filePath"));
        }

        [Fact]
        public async Task TestSuccessfullRegisterDeviceWithPrivateKeyAsync()
        {
            string expectedCredentialSecret = "TTkd5OgB13X/3qU0LXU7OCxyTXz5QHM2NY1IgidtPOs=";
            string expectedRegisterPath = "/v1/test/agent/devices";
            string deviceId = "YHjKs3SMTgqq09eD7fzm6w";

            JObject expectedRequestBody;

            expectedRequestBody = new(
                            new JProperty("data",
                                new JObject(
                                    new JProperty("hw_id", deviceId)
                                    )
                                ));

            string responseBody =
                "{\n"
                    + "  \"data\": {\n"
                    + "    \"credentials_secret\": \""
                    + expectedCredentialSecret
                    + "\"\n"
                    + "  }\n"
                    + "}";

            // Arrange (start WireMock.Net server)
            _server
              .Given(Request.Create().WithPath($"/v1/test/agent/devices").UsingPost())
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
                  .WithHeader("Content-Type", "application/json; charset=utf-8")
                  .WithBody(responseBody)
              );

            byte[] array = Convert.FromBase64String("MHcCAQEEIHD69jyGfwV/M03C2Q1FaYOgv35quQyHWXLd33WRcSvqoA" +
                "oGCCqGSM49AwEHoUQDQgAEMTK5P4imOy6/qZSz8nhfy/7UOm/G9ZsOK52/15QC+DZmw1iQIo81FIOWCSt7mVNEsu+5H9" +
                "JdorKpZMgsh6rzLQ==");
            string privateKey = new(PemEncoding.Write("EC PRIVATE KEY", array).ToArray());

            _service = new AstartePairingService($"{_server.Urls[0]}", "test");
            var myPath = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            myPath = Path.Combine(myPath, "key.pem");

            File.WriteAllText(myPath, privateKey);

            //Act
            string credentialSecret;
            try
            {
                credentialSecret = await _service.RegisterDeviceWithPrivateKey(deviceId,
                myPath);
            }
            finally
            {
                File.Delete(myPath);
            }

            var customerReadRequests = _server.FindLogEntries(
                        Request.Create().WithPath("/v1/test/agent/devices").UsingPost()
                    );

            //Assert
            string registerPath;
            JObject requestBody;
            foreach (var item in customerReadRequests)
            {
                registerPath = item.RequestMessage.Path;
                requestBody = (JObject)item.RequestMessage.BodyData.BodyAsJson;
                Assert.Equal(expectedRequestBody, requestBody);
                Assert.Equal(expectedRegisterPath, registerPath);
            }

            Assert.Equal(expectedCredentialSecret, credentialSecret);
        }

    }
}
