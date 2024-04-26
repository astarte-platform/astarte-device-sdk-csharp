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

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;

namespace AstarteDeviceSDKCSharp.Data
{
    public class AstarteFailedMessageStorage : IAstarteFailedMessageStorage
    {
        private readonly AstarteDbContext _astarteDbContext;
        private readonly AstarteDbContext _astarteDbContextDelete;
        private readonly AstarteDbContext _astarteDbContextRead;

        private static List<AstarteFailedMessageEntry> _astarteFailedMessageVolatile = new();
        readonly HashSet<ManagedMqttApplicationMessage> stored = new HashSet<ManagedMqttApplicationMessage>();


        public AstarteFailedMessageStorage(string persistencyDir)
        {
            this._astarteDbContext = new AstarteDbContext(persistencyDir);
            this._astarteDbContextDelete = new AstarteDbContext(persistencyDir);
            this._astarteDbContextRead = new AstarteDbContext(persistencyDir);
        }

        public async Task InsertStored(string topic, byte[] payload, int qos, Guid guid)
        {
            AstarteFailedMessageEntry failedMessageEntry
            = new AstarteFailedMessageEntry(qos, payload, topic, guid);

            _astarteDbContext.AstarteFailedMessages.Add(failedMessageEntry);

            Trace.WriteLine($"Insert fallback message in database: "
            + $"{topic} : {guid}");

            await _astarteDbContext.SaveChangesAsync();
        }

        public async Task InsertStored(string topic, byte[] payload, int qos, Guid guid, int relativeExpiry)
        {
            try
            {
                AstarteFailedMessageEntry failedMessageEntry
                    = new AstarteFailedMessageEntry(qos, payload, topic, guid, relativeExpiry);

                _astarteDbContext.AstarteFailedMessages.Add(failedMessageEntry);

                Trace.WriteLine($"Insert fallback message in database:"
                + $"{topic} : {guid},"
                + $" expiry time: {relativeExpiry}");

                await _astarteDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to insert fallback message in database. Error message: {ex.Message}");
            }
        }

        public void InsertVolatile(string topic, byte[] payload, int qos, Guid guid)
        {
            AstarteFailedMessageEntry failedMessageEntry
            = new AstarteFailedMessageEntry(qos, payload, topic, guid);

            Trace.WriteLine($"Insert fallback message in cache memory: "
            + $"{topic} : {guid}");

            _astarteFailedMessageVolatile.Add(failedMessageEntry);
        }

        public void InsertVolatile(string topic, byte[] payload, int qos, Guid guid, int relativeExpiry)
        {
            AstarteFailedMessageEntry failedMessageEntry
            = new AstarteFailedMessageEntry(qos, payload, topic, guid, relativeExpiry);

            Trace.WriteLine($"Insert fallback message in cache memory: "
            + $"{topic} : {guid},"
            + $" expiry time: {relativeExpiry}");

            _astarteFailedMessageVolatile.Add(failedMessageEntry);
        }

        public async Task Reject(AstarteFailedMessageEntry failedMessages)
        {

            if (failedMessages is not null)
            {
                Trace.WriteLine($"Remove from local database "
                + $"{failedMessages.GetTopic()} : "
                + $"{failedMessages.GetPayload()}");
                _astarteDbContext.AstarteFailedMessages.Remove(failedMessages);
            }

            await _astarteDbContext.SaveChangesAsync();
        }

        public void RejectCache(AstarteFailedMessageEntry failedMessages)
        {
            if (failedMessages is not null)
            {
                Trace.WriteLine($"Remove from cache memory "
                + $"{failedMessages.GetTopic()} : "
                + $"{failedMessages.GetGuid()}");
                _astarteFailedMessageVolatile.Remove(failedMessages);
            }
        }

        public bool IsExpired(long expire)
        {
            return expire != 0 ? (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expire) : false;
        }

        public async Task SaveQueuedMessagesAsync(IList<ManagedMqttApplicationMessage> messages)
        {
            MqttUserProperty? retention = null;
            foreach (var message in messages)
            {

                if (message.ApplicationMessage.UserProperties is null)
                {
                    continue;
                }

                if (!stored.Contains(message))
                {
                    retention = message.ApplicationMessage.UserProperties.Where(x => x.Name == "Retention").FirstOrDefault();

                    if (retention == null || retention.Value == "DISCARD")
                    {
                        continue;
                    }

                    if (retention.Value == "STORED")
                    {
                        if (message.ApplicationMessage.MessageExpiryInterval > 0)
                        {
                            await InsertStored
                              (
                                  message.ApplicationMessage.Topic,
                                  message.ApplicationMessage.Payload,
                                  (int)message.ApplicationMessage.QualityOfServiceLevel,
                                  message.Id,
                                  (int)message.ApplicationMessage.MessageExpiryInterval
                              );
                        }
                        else
                        {
                            await InsertStored
                              (
                                  message.ApplicationMessage.Topic,
                                  message.ApplicationMessage.Payload,
                                  (int)message.ApplicationMessage.QualityOfServiceLevel,
                                  message.Id
                              );
                        }
                    }
                    else if (retention.Value == "VOLATILE")
                    {

                        if (message.ApplicationMessage.MessageExpiryInterval > 0)
                        {
                            InsertVolatile
                            (
                                message.ApplicationMessage.Topic,
                                message.ApplicationMessage.Payload,
                                (int)message.ApplicationMessage.QualityOfServiceLevel,
                                message.Id,
                                (int)message.ApplicationMessage.MessageExpiryInterval
                            );
                        }
                        else
                        {
                            InsertVolatile
                            (
                                message.ApplicationMessage.Topic,
                                message.ApplicationMessage.Payload,
                                (int)message.ApplicationMessage.QualityOfServiceLevel,
                                message.Id
                            );
                        }
                    }
                    stored.Add(message);
                }
            }
        }

        public async Task<IList<ManagedMqttApplicationMessage>> LoadQueuedMessagesAsync()
        {
            var result = new List<ManagedMqttApplicationMessage>();
            foreach (var failedMessage in await _astarteDbContext.AstarteFailedMessages.ToListAsync())
            {
                if (IsExpired(failedMessage.GetExpiry()))
                {
                    await Reject(failedMessage);
                    continue;
                }

                var item = new ManagedMqttApplicationMessage
                {
                    ApplicationMessage
                   = new MqttApplicationMessage
                   {
                       ContentType = null,
                       CorrelationData = null,
                       Dup = false,
                       MessageExpiryInterval = 0,
                       Payload = failedMessage.GetPayload(),
                       QualityOfServiceLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)failedMessage.GetQos(),
                       ResponseTopic = null,
                       Retain = false,
                       SubscriptionIdentifiers = null,
                       Topic = failedMessage.GetTopic(),
                       TopicAlias = 0,
                       UserProperties = null
                   },
                    Id = failedMessage.GetGuid(),
                };

                result.Add(item);
                stored.Add(item);
            }

            foreach (var messageVolatile in _astarteFailedMessageVolatile)
            {
                if (IsExpired(messageVolatile.GetExpiry()))
                {
                    RejectCache(messageVolatile);
                    continue;
                }

                var item = new ManagedMqttApplicationMessage
                {
                    ApplicationMessage
                   = new MqttApplicationMessage
                   {
                       ContentType = null,
                       CorrelationData = null,
                       Dup = false,
                       MessageExpiryInterval = 0,
                       Payload = messageVolatile.GetPayload(),
                       QualityOfServiceLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)messageVolatile.GetQos(),
                       ResponseTopic = null,
                       Retain = false,
                       SubscriptionIdentifiers = null,
                       Topic = messageVolatile.GetTopic(),
                       TopicAlias = 0,
                       UserProperties = null
                   },
                    Id = messageVolatile.GetGuid(),
                };

                result.Add(item);
                stored.Add(item);
            }

            return result;

        }

        public async Task DeleteByGuidAsync(ManagedMqttApplicationMessage applicationMessage)
        {
            if (stored.Contains(applicationMessage))
            {
                stored.Remove(applicationMessage);

                try
                {
                    var message = await GetAstarteFailedMessageStorage(applicationMessage.Id);

                    if (message is not null)
                    {
                        _astarteDbContextDelete.AstarteFailedMessages.Remove(message);
                    }

                    if (_astarteFailedMessageVolatile is not null)
                    {
                        var messageVolatile = _astarteFailedMessageVolatile
                            .Where(x => x.Guid == applicationMessage.Id)
                            .FirstOrDefault();
                        if (messageVolatile is not null)
                        {
                            _astarteFailedMessageVolatile.Remove(messageVolatile);
                        }

                    }

                    await _astarteDbContextDelete.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Failed to delete fallback message from database. Error message: {ex.Message}");
                }

            }

        }

        private async Task<AstarteFailedMessageEntry?> GetAstarteFailedMessageStorage(Guid guid)
        {
            var response = await _astarteDbContextRead.AstarteFailedMessages
                .Where(x => x.Guid.ToString() == guid.ToString().ToUpper())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return response is not null ? response : null;
        }
    }
}
