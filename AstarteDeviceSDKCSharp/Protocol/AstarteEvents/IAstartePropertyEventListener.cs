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

using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public interface IAstartePropertyEventListener
    {
        /// <summary>
        /// Handles the event when a property is received.
        /// </summary>
        /// <param name="e">The AstartePropertyEvent object containing information 
        /// about the received property.</param>
        void PropertyReceived(AstartePropertyEvent e);

        /// <summary>
        /// Handles the event when a property is unset.
        /// </summary>
        /// <param name="e">The AstartePropertyEvent object containing information
        /// about the unset property.</param>
        void PropertyUnset(AstartePropertyEvent e);
    }
}
