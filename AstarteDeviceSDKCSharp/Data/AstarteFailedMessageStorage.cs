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

namespace AstarteDeviceSDKCSharp.Data
{
    public class AstarteFailedMessageStorage : IAstarteFailedMessageStorage
    {
        private readonly AstarteDbContext _astarteDbContext;

        private static List<AstarteFailedMessageEntry> _astarteFailedMessageVolatile = new();

        public AstarteFailedMessageStorage(string persistencyDir)
        {
            this._astarteDbContext = new AstarteDbContext(persistencyDir);
        }

        public void AckFirst()
        {
            var failedMessages = _astarteDbContext.AstarteFailedMessages
            .OrderBy(x => x.Id)
            .ToList();

            if (failedMessages.Count() > 0)
            {
                _astarteDbContext.AstarteFailedMessages.Remove(failedMessages.First());
            }

            _astarteDbContext.SaveChanges();
        }

        public void InsertStored(string topic, byte[] payload, int qos)
        {
            AstarteFailedMessageEntry failedMessageEntry
            = new AstarteFailedMessageEntry(qos, payload, topic);

            _astarteDbContext.AstarteFailedMessages.Add(failedMessageEntry);

            _astarteDbContext.SaveChanges();
        }

        public void InsertStored(string topic, byte[] payload, int qos, int relativeExpiry)
        {
            AstarteFailedMessageEntry failedMessageEntry
            = new AstarteFailedMessageEntry(qos, payload, topic, relativeExpiry);

            _astarteDbContext.AstarteFailedMessages.Add(failedMessageEntry);

            _astarteDbContext.SaveChanges();
        }

        public void InsertVolatile(string topic, byte[] payload, int qos)
        {
            AstarteFailedMessageEntry failedMessageEntry
            = new AstarteFailedMessageEntry(qos, payload, topic);

            _astarteFailedMessageVolatile.Add(failedMessageEntry);
        }

        public void InsertVolatile(string topic, byte[] payload, int qos, int relativeExpiry)
        {
            AstarteFailedMessageEntry failedMessageEntry
            = new AstarteFailedMessageEntry(qos, payload, topic, relativeExpiry);

            _astarteFailedMessageVolatile.Add(failedMessageEntry);
        }

        public bool IsEmpty()
        {
            return !_astarteDbContext.AstarteFailedMessages.Any();
        }

        public AstarteFailedMessageEntry? PeekFirst()
        {
            return _astarteDbContext.AstarteFailedMessages
            .OrderBy(x => x.Id)
            .FirstOrDefault();
        }

        public void RejectFirst()
        {
            var failedMessages = _astarteDbContext.AstarteFailedMessages
            .OrderBy(x => x.Id)
            .ToList();

            if (failedMessages.Count() > 0)
            {
                _astarteDbContext.AstarteFailedMessages.Remove(failedMessages.First());
            }

            _astarteDbContext.SaveChanges();
        }
    }
}
