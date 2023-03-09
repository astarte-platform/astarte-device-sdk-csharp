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

using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AstarteDeviceSDKCSharp.Utilities
{

    internal class AstartePayloadItem
    {
        [JsonProperty("v")]
        public object Value { get; set; }
        [JsonProperty("t")]
        public DateTime? TimeStamp { get; set; }

        public AstartePayloadItem(object value, DateTime? timeStamp)
        {
            Value = value;
            if (timeStamp.HasValue)
            {
                TimeStamp = timeStamp;
            }
        }
    }

    public class AstartePayload
    {
        public AstartePayload() { }
        public static byte[] Serialize(Object? o, DateTime? t)
        {
            if (o is not { })
            {
                return Array.Empty<byte>();
            }

            o = PrepareDateTimeValues(o);

            AstartePayloadItem payloadObject = new(o, t);
            MemoryStream ms = new();
            using (BsonDataWriter writer = new(ms))
            {
                JsonSerializer serializer = new();
                serializer.Serialize(writer, payloadObject);
            }

            byte[] payload = ms.ToArray();
            return payload;
        }

        public static DecodedMessage? Deserialize(byte[] mqttPayload)
        {
            MemoryStream ms = new MemoryStream(mqttPayload);
            using BsonDataReader reader = new(ms);
            JsonSerializer serializer = new();

            DecodedMessage? decodedMessage = serializer.Deserialize<DecodedMessage>(reader);

            if (decodedMessage is null)
            {
                return decodedMessage;
            }

            object decodedObject = decodedMessage.GetPayload();
            Type payloadType = GetPayloadType(decodedObject);

            if (payloadType == typeof(DateTime))
            {
                decodedMessage.SetPayload(((DateTime)decodedObject).ToUniversalTime());
            }
            else if (payloadType == typeof(DateTime[]))
            {
                JArray bsonList = (JArray)decodedObject;
                DateTime[] dateTimes = new DateTime[bsonList.Count];

                for (int i = 0; i < ((JArray)decodedObject).Count; i++)
                {
                    JToken item = ((JArray)decodedObject).ElementAt(i);
                    DateTime dateItem;
                    if (DateTime.TryParse(item.ToString(), out dateItem))
                    {
                        dateTimes[i] = dateItem.ToUniversalTime();
                    }
                }
                decodedMessage.SetPayload(dateTimes);
            }
            else if (payloadType == typeof(Array))
            {
                JArray bsonList = (JArray)decodedObject;
                object[] objects = new object[bsonList.Count];
                for (int i = 0; i < bsonList.Count; i++)
                {
                    JToken item = ((JArray)decodedObject).ElementAt(i);
                    objects[i] = item.ToObject<object>()!;
                }
                decodedMessage.SetPayload(objects);
            }
            else if (payloadType == typeof(JObject))
            {

                // if payload is a map (JObject)
                Dictionary<string, object> dic = ((JObject)decodedMessage.GetPayload()).ToObject<Dictionary<string, object>>()!;
                decodedMessage.SetPayload(dic);

            }

            return decodedMessage;
        }

        private static Type GetPayloadType(object payload)
        {
            if (payload is DateTime)
            {
                return typeof(DateTime);
            }
            else if (payload is JObject)
            {
                if (payload is DateTime)
                    return typeof(DateTime);

                return payload.GetType();
            }
            else if (payload is JArray)
            {
                JArray bsonList = (JArray)payload;
                var item = bsonList.First();
                DateTime dateItem;
                if (DateTime.TryParse(item.ToString(), out dateItem))
                    return typeof(DateTime[]);

                return typeof(Array);

            }
            return payload.GetType();
        }

        private static object PrepareDateTimeValues(object o)
        {
            if (o.GetType().IsInstanceOfType(typeof(DateTime)))
            {
                return (DateTime)o;
            }

            if (o.GetType().IsInstanceOfType(typeof(DateTime[])))
            {
                return DateTimesArrayToDateList((DateTime[])o);
            }

            if (o.GetType().IsInstanceOfType(typeof(Dictionary<string, object>)))
            {
                Dictionary<string, object> aggregate = new((Dictionary<string, object>)o);
                foreach (KeyValuePair<string, object> entry in aggregate)
                {
                    if (entry.Value.GetType().IsInstanceOfType(typeof(DateTime)))
                    {
                        aggregate[entry.Key] = (DateTime)entry.Value;
                    }
                    else if (entry.Value.GetType().IsInstanceOfType(typeof(DateTime[])))
                    {
                        aggregate[entry.Key] = (List<DateTime>)entry.Value;
                    }
                }
                return aggregate;
            }

            return o;

        }

        private static object DateTimesArrayToDateList(DateTime[] o)
        {
            List<DateTime> list = new();
            foreach (DateTime dt in o)
            {
                list.Add(dt);
            }
            return list;
        }
    }
}
