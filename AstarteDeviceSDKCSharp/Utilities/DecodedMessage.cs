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

using Newtonsoft.Json;

namespace AstarteDeviceSDKCSharp.Utilities
{
    public class DecodedMessage
    {
        [JsonProperty("v")]
        private object payload;
        [JsonProperty("t")]
        private DateTime? timestamp;

        public DecodedMessage(DateTime _timestamp, object _payload)
        {
            payload = _payload;
            timestamp = _timestamp;
        }

        public object GetPayload()
        {
            return payload;
        }

        public void SetPayload(object payload)
        {
            this.payload = payload;
        }

        public DateTime? GetTimestamp()
        {
            return timestamp;
        }

        public void SetTimestamp(DateTime timestamp)
        {
            this.timestamp = timestamp;
        }

        public bool PayloadEquality(object? otherPayload)
        {
            if (otherPayload is null)
            {
                return false;
            }

            if (payload is Array array && otherPayload is Array otherArray)
            {
                if (array == null || otherArray == null) return false;
                if (array.Length != otherArray.Length) return false;
                return ArrayEquality(array, otherArray);
            }

            return payload.GetHashCode().Equals(otherPayload.GetHashCode());

        }

        private bool ArrayEquality(Array array, Array otherArray)
        {

            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) == null || otherArray.GetValue(i) == null) return false;

                if (array.GetValue(i) is Array arrayOfArray && otherArray.GetValue(i) is Array arrayOfOtherArray)
                {
                    if (!ArrayEquality(arrayOfArray, arrayOfOtherArray))
                    {
                        return false;
                    }
                }
                else if (!array.GetValue(i)!.GetHashCode().Equals(otherArray.GetValue(i)!.GetHashCode()))
                {
                    return false;
                }
            }
            return true;
        }

    }

}
