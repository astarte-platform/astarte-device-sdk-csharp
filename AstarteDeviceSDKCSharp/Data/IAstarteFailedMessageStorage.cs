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

using MQTTnet.Extensions.ManagedClient;

namespace AstarteDeviceSDKCSharp.Data
{
    public interface IAstarteFailedMessageStorage
    {
        void InsertVolatile(String topic, byte[] payload, int qos, Guid guid);

        void InsertVolatile(String topic, byte[] payload, int qos, Guid guid, int relativeExpiry);

        Task InsertStored(String topic, byte[] payload, int qos, Guid guid);

        Task InsertStored(String topic, byte[] payload, int qos, Guid guid, int relativeExpiry);

        Task Reject(AstarteFailedMessageEntry astarteFailedMessages);

        void RejectCache(AstarteFailedMessageEntry astarteFailedMessages);

        bool IsExpired(long expire);

        Task DeleteByGuidAsync(Guid applicationMessage);
        Task MarkAsProcessed(Guid applicationMessage);
        Task DeleteProcessed();

        Task<IList<ManagedMqttApplicationMessage>> LoadQueuedMessagesAsync();
        Task SaveQueuedMessageAsync(ManagedMqttApplicationMessage message);

    }
}
