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

using System.Data;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;

namespace AstarteDeviceSDKCSharp.Data
{
    public class AstarteFailedMessageStorage : IAstarteFailedMessageStorage
    {
        private readonly object _storeLock = new();
        private readonly string _dbConnectionString;

        SqliteConnection sqliteConnection = new SqliteConnection();
        SqliteConnection sqliteReadConnection = new SqliteConnection();
        private static List<AstarteFailedMessageEntry> _astarteFailedMessageVolatile = new();

        public AstarteFailedMessageStorage(string persistencyDir)
        {

            _dbConnectionString = $"Filename = {persistencyDir}{Path.DirectorySeparatorChar}AstarteDeviceDb";

            sqliteConnection = new SqliteConnection(_dbConnectionString);
            sqliteConnection.Open();

            sqliteReadConnection = new SqliteConnection(_dbConnectionString);
            sqliteReadConnection.Open();

        }

        public async Task InsertStored(string topic, byte[] payload, int qos, Guid guid)
        {
            try
            {
                lock (_storeLock)
                {
                    if (sqliteConnection.State != ConnectionState.Open)
                    {
                        sqliteConnection = new SqliteConnection(_dbConnectionString);
                        sqliteConnection.Open();
                    }

                    using (SqliteCommand insertCommand = new SqliteCommand())
                    {
                        insertCommand.Connection = sqliteConnection;
                        insertCommand.CommandText = "INSERT INTO AstarteFailedMessages (Qos, Payload, Topic, absolute_expiry, guid) " +
                                                    "VALUES (@qos,@payload,@topic,@absolute_expiry,@guid) ON CONFLICT DO NOTHING;";
                        insertCommand.Parameters.AddWithValue("@qos", qos);
                        insertCommand.Parameters.AddWithValue("@payload", payload);
                        insertCommand.Parameters.AddWithValue("@topic", topic);
                        insertCommand.Parameters.AddWithValue("@absolute_expiry", 0);
                        insertCommand.Parameters.AddWithValue("@guid", guid);

                        insertCommand.ExecuteNonQueryAsync().Wait();
                    }
                }

                Trace.WriteLine($"Insert fallback message in database:"
                + $"{topic} : {guid}");

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to insert fallback message in database. Error message: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task InsertStored(string topic, byte[] payload, int qos, Guid guid, int relativeExpiry)
        {
            try
            {
                lock (_storeLock)
                {
                    if (sqliteConnection.State != ConnectionState.Open)
                    {
                        sqliteConnection = new SqliteConnection(_dbConnectionString);
                        sqliteConnection.Open();
                    }

                    using (SqliteCommand insertCommand = new SqliteCommand())
                    {
                        insertCommand.Connection = sqliteConnection;
                        insertCommand.CommandText = "INSERT INTO AstarteFailedMessages (Qos, Payload, Topic, absolute_expiry, guid) " +
                                                    "VALUES (@qos,@payload,@topic,@absolute_expiry,@guid) ON CONFLICT DO NOTHING;";
                        insertCommand.Parameters.AddWithValue("@qos", qos);
                        insertCommand.Parameters.AddWithValue("@payload", payload);
                        insertCommand.Parameters.AddWithValue("@topic", topic);
                        insertCommand.Parameters.AddWithValue("@absolute_expiry", relativeExpiry);
                        insertCommand.Parameters.AddWithValue("@guid", guid);

                        insertCommand.ExecuteNonQueryAsync().Wait();
                    }
                }

                Trace.WriteLine($"Insert fallback message in database:"
                + $"{topic} : {guid},"
                + $" expiry time: {relativeExpiry}");

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to insert fallback message in database. Error message: {ex.Message}");
            }
            await Task.CompletedTask;
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

                if (sqliteConnection.State != ConnectionState.Open)
                {
                    sqliteConnection = new SqliteConnection(_dbConnectionString);
                    sqliteConnection.Open();
                }
                using (SqliteCommand cmd = new SqliteCommand(
                    "DELETE FROM AstarteFailedMessages WHERE guid = @guid;", sqliteConnection))
                {

                    cmd.Parameters.AddWithValue("@guid", failedMessages.Id);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

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

        public async Task SaveQueuedMessageAsync(ManagedMqttApplicationMessage message)
        {

            MqttUserProperty? retention = null;

            if (message.ApplicationMessage.UserProperties is null)
            {
                return;
            }

            retention = message.ApplicationMessage.UserProperties.Where(x => x.Name == "Retention").FirstOrDefault();

            if (retention == null || retention.Value == "DISCARD")
            {
                return;
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
        }

        public async Task<IList<ManagedMqttApplicationMessage>> LoadQueuedMessagesAsync()
        {
            var result = new List<ManagedMqttApplicationMessage>();
            foreach (var failedMessage in await GetAstarteFailedMessageStorage())
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
            }

            return result;

        }

        public async Task DeleteByGuidAsync(Guid applicationMessageId)
        {

            try
            {
                if (sqliteReadConnection.State != ConnectionState.Open)
                {
                    sqliteReadConnection = new SqliteConnection(_dbConnectionString);
                    sqliteReadConnection.Open();
                }

                SqliteCommand cmd = new SqliteCommand(
                    "DELETE FROM AstarteFailedMessages WHERE guid = @guid ;", sqliteReadConnection);

                cmd.Parameters.AddWithValue("@guid", applicationMessageId);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();

                if (_astarteFailedMessageVolatile is not null)
                {
                    var messageVolatile = _astarteFailedMessageVolatile
                        .Where(x => x.Guid == applicationMessageId)
                        .FirstOrDefault();
                    if (messageVolatile is not null)
                    {
                        _astarteFailedMessageVolatile.Remove(messageVolatile);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to delete fallback message from database. Error message: {ex.Message}");
            }
        }

        private async Task<IList<AstarteFailedMessageEntry>> GetAstarteFailedMessageStorage()
        {

            List<AstarteFailedMessageEntry> response = new List<AstarteFailedMessageEntry>();

            if (sqliteReadConnection.State != ConnectionState.Open)
            {
                sqliteReadConnection = new SqliteConnection(_dbConnectionString);
                sqliteReadConnection.Open();
            }

            string query = @"SELECT Qos, Payload, Topic, guid, absolute_expiry 
                FROM AstarteFailedMessages ORDER BY ID LIMIT 10000;";
            using SqliteCommand cmd = new SqliteCommand(query, sqliteReadConnection);
            using SqliteDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                response.Add(new AstarteFailedMessageEntry(
                    dr.GetInt32(0),
                    (byte[])dr["Payload"],
                    dr.GetString(2),
                    dr.GetGuid(3),
                    dr.GetInt32(4)));
            }
            return response;
        }

    }
}
