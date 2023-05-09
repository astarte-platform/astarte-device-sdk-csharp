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
using AstarteDeviceSDKCSharp.Protocol.AstarteEvents;
using Newtonsoft.Json;

namespace AstarteDeviceSDKExample
{
    public class ExampleGlobalEventListener : AstarteGlobalEventListener
    {
        /// <summary>
        /// This function gets called when the device receives data on a server owned
        /// properties interface.
        /// </summary>
        /// <param name="e"></param>
        public override void PropertyReceived(AstartePropertyEvent e)
        {
            Console.WriteLine(
                $"Received property on interface {e.GetInterfaceName()}, " +
                $"path: {e.GetPath()}, " +
                $"value:{e.GetValue()}");
        }

        /// <summary>
        /// This function gets called when the device receives an unset on a server owned
        /// properties interface.
        /// </summary>
        /// <param name="e"></param>
        public override void PropertyUnset(AstartePropertyEvent e)
        {
            Console.WriteLine(
                $"Received unset on interface {e.GetInterfaceName()}, " +
                $"path: {e.GetPath()}");
        }

        /// <summary>
        /// This function gets called when the device receives data on a server owned
        /// datastream interface with individual aggregation.
        /// </summary>
        /// <param name="e"></param>
        public override void ValueReceived(AstarteDatastreamEvent e)
        {
            Console.WriteLine(
                $"Received datastream value on interface {e.GetInterfaceName()}, " +
                $"path: {e.GetPath()}, " +
                $"value:{e.GetValue()}");
        }

        /// <summary>
        /// This function gets called when the device receives data on a server owned
        /// datastream interface with object aggregation.
        /// </summary>
        /// <param name="e"></param>
        public override void ValueReceived(AstarteAggregateDatastreamEvent e)
        {
            Console.WriteLine(
                $"Received aggregate datastream object on interface {e.GetInterfaceName()}, " +
                $"value:{JsonConvert.SerializeObject(e.GetValues())}");
        }
    }
}
