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

using AstarteDeviceSDKCSharp.Protocol;

namespace AstarteDeviceSDK.Protocol
{
    public interface IAstarteProtocol
    {
        /// <summary>
        /// Utility function used to send the introspection to Astarte.
        /// </summary>
        Task SendIntrospection();

        /// <summary>
        /// Sends an aggregate message to an interface.
        /// </summary>
        /// <param name="astarteInterface">Astarte aggregate datastream interface</param>
        /// <param name="path">Endpoint</param>
        /// <param name="value">Payload for MQTT message</param>
        /// <param name="timeStamp">UTC</param>
        Task SendAggregate(AstarteAggregateDatastreamInterface astarteInterface, string path,
        Dictionary<string, object> value, DateTime? timeStamp);

        /// <summary>
        /// Sends an individual message to an interface with timestamp. 
        /// </summary>
        /// <param name="astarteInterface">Astarte aggregate datastream interface</param>
        /// <param name="path">Endpoint</param>
        /// <param name="value">Payload for MQTT message</param>
        /// <param name="timeStamp">UTC</param>
        Task SendIndividualValue(AstarteInterface astarteInterface, String path, Object? value,
        DateTime? timestamp);

        /// <summary>
        /// Sends an individual message to an interface.
        /// </summary>
        /// <param name="astarteInterface">Astarte aggregate data stream interface</param>
        /// <param name="path">Endpoint</param>
        /// <param name="value">Payload for MQTT message</param>
        Task SendIndividualValue(AstarteInterface astarteInterface, String path, Object? value);
        Task ResendAllProperties();
    }
}
