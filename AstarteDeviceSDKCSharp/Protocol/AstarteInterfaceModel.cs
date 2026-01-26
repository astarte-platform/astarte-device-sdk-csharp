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

namespace AstarteDeviceSDK.Protocol
{
    public class AstarteInterfaceModel
    {
        [JsonProperty("interface_name")]
        public string InterfaceName { get; set; } = string.Empty;
        [JsonProperty("version_major")]
        public int MajorVersion { get; set; }
        [JsonProperty("version_minor")]
        public int MinorVersion { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("aggregation")]
        public string Aggregation { get; set; } = string.Empty;

        [JsonProperty("ownership")]
        public string Ownership { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("doc")]
        public string Doc { get; set; } = string.Empty;

        [JsonProperty("mappings")]
        public IList<Mapping> Mappings { get; set; } = new List<Mapping>();
    }

    public class Mapping
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("explicit_timestamp")]
        public bool? ExplicitTimestamp { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("reliability")]
        public string Reliability { get; set; } = string.Empty;

        [JsonProperty("retention")]
        public string Retention { get; set; } = string.Empty;

        [JsonProperty("expiry")]
        public int? Expiry { get; set; }

        [JsonProperty("allow_unset")]
        public bool AllowUnset { get; set; } = false;

    }
}
