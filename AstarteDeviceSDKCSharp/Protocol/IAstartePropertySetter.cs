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

namespace AstarteDeviceSDKCSharp.Protocol
{
    public interface IAstartePropertySetter
    {
        /// <summary>
        /// Set the specified property on an interface.
        /// </summary>
        /// <param name="path">Endpoint</param>
        /// <param name="payload">Message for MQTT broker</param>
        public void SetProperty(String path, Object payload);

        /// <summary>
        /// Unset the specified property on an interface.
        /// </summary>
        /// <param name="path">Endpoint</param>
        public void UnsetProperty(String path);
    }
}
