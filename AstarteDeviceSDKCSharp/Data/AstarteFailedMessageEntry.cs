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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstarteDeviceSDKCSharp.Data
{
    public class AstarteFailedMessageEntry : IAstarteFailedMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int Qos { get; set; }
        [Required]
        public byte[] Payload { get; set; }
        [Required]
        public string Topic { get; set; } = string.Empty;

        [Column("absolute_expiry")]
        [Required]
        public long AbsoluteExpiry { get; set; }


        public AstarteFailedMessageEntry(int qos, byte[] payload, string topic)
        {
            Qos = qos;
            Payload = payload;
            Topic = topic;
            AbsoluteExpiry = 0;
        }

        public AstarteFailedMessageEntry(int qos, byte[] payload, string topic,
        int relativeExpiry)
        {
            Qos = qos;
            Payload = payload;
            Topic = topic;
            AbsoluteExpiry = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + relativeExpiry;
        }

        public string GetTopic()
        {
            return Topic;
        }

        public byte[] GetPayload()
        {
            return Payload;
        }

        public int GetQos()
        {
            return Qos;
        }

        public long GetExpiry()
        {
            return AbsoluteExpiry;
        }

    }
}
