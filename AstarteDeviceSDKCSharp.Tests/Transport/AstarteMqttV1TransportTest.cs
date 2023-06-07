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
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AstarteDeviceSDKCSharp.Tests
{
    public class AstarteMqttV1TransportTest
    {
        [Fact]
        public void IntegerToEncodedBSONTest()
        {
            long i = 3;
            byte[] encodedPayload = AstartePayload.Serialize(i, null);
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);

            Assert.Equal(i, decodedMessage.GetPayload());
        }

        [Fact]
        public void IntegerWTimestampToEncodedBSONTest()
        {
            long i = 3;
            byte[] encodedPayload = AstartePayload.Serialize(i, new DateTime());
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);

            Assert.Equal(i, decodedMessage.GetPayload());
        }

        [Fact]
        public void MapToEncodedBSONTest()
        {
            Dictionary<string, object> m = new();
            m.Add("/int", 1);
            m.Add("/double", 1.0);
            m.Add("/string", "s");

            byte[] encodedPayload = AstartePayload.Serialize(m, new DateTime());
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);
            Dictionary<string, object> payload = (Dictionary<string, object>)decodedMessage.GetPayload();
            Assert.True(m.Count == payload.Count && m.Except(payload).Any());
        }

        [Fact]
        public void ArrayToEncodedBSONTest()
        {
            int[] i = new int[] { 1, 2, 3 };
            byte[] encodedPayload = AstartePayload.Serialize(i, new DateTime());
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);

            var objects = decodedMessage.GetPayload() as object[];
            int[] payloadArray = objects.Select(x => Convert.ToInt32(x)).ToArray();
            Assert.True(payloadArray.GetType() == typeof(int[]));
            Assert.Equal(i, (int[])payloadArray);
        }

        [Fact]
        public void MapArrayToEncodedBSONTest()
        {
            Dictionary<string, object> m = new();
            int[] intarray = new int[] { 1, 2, 3 };
            DateTime now = new DateTime();
            DateTime[] dateTimeArray = new DateTime[] { now, now };
            m.Add("/intarray", intarray);
            m.Add("/double", 1.0);
            m.Add("/string", "s");
            m.Add("/datetime", now);
            m.Add("/datetimearray", dateTimeArray);

            byte[] encodedPayload = AstartePayload.Serialize(m, new DateTime());
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);
            Dictionary<string, object> payload = (Dictionary<string, object>)decodedMessage.GetPayload();
            Assert.True(m.Count == payload.Count && m.Except(payload).Any());
        }

        [Fact]
        public void BsonToEncodedBSONTest()
        {
            Dictionary<string, string> m = new();
            m.Add("first", "one");

            byte[] b = AstartePayload.Serialize(m, null);
            byte[] encodedPayload = AstartePayload.Serialize(b, new DateTime());
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);
            Assert.True(decodedMessage.GetPayload().GetType() == typeof(byte[]));
            Assert.Equal(b, (byte[])decodedMessage.GetPayload());
        }

        [Fact]
        public void BsonArrayToEncodedBSONTest()
        {
            Dictionary<string, string> m = new();
            m.Add("first", "one");

            byte[][] b = new byte[][] { AstartePayload.Serialize(m, null), AstartePayload.Serialize(m, null) };
            byte[] encodedPayload = AstartePayload.Serialize(b, null);
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);

            byte[][] payload = (decodedMessage.GetPayload() as object[]).OfType<byte[]>().ToArray();

            Assert.True(payload.GetType() == typeof(byte[][]));
            Assert.Equal(b, payload);
        }

        [Fact]
        public void DatetimeToEncodedBSONTest()
        {
            DateTime d = new DateTime();
            byte[] encodedPayload = AstartePayload.Serialize(d, new DateTime());
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);

            Assert.True(decodedMessage.GetPayload().GetType() == typeof(DateTime));
            Assert.Equal(d, (DateTime)decodedMessage.GetPayload());
        }

        [Fact]
        public void DatetimeArrayToEncodedBSONTest()
        {
            DateTime[] d = new DateTime[] { new DateTime(2009, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime() };
            byte[] encodedPayload = AstartePayload.Serialize(d, null);

            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);

            Assert.True(decodedMessage.GetPayload().GetType() == typeof(DateTime[]));
            Assert.Equal(d, decodedMessage.GetPayload());
        }

        [Fact]
        public void DoubleArrayToEncodedBSONTest()
        {
            double[] i = new double[] { 1.1, 2.1, 3.1 };
            byte[] encodedPayload = AstartePayload.Serialize(i, new DateTime());
            DecodedMessage decodedMessage = AstartePayload.Deserialize(encodedPayload);

            var objects = decodedMessage.GetPayload() as object[];
            double[] payloadArray = objects.Select(x => Convert.ToDouble(x)).ToArray();
            Assert.True(payloadArray.GetType() == typeof(double[]));
            Assert.Equal(i, (double[])payloadArray);
        }
    }
}
