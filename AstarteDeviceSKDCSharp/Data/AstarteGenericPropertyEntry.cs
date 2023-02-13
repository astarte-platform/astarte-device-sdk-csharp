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
    public class AstarteGenericPropertyEntry
    {
        [Key]
        public string Id { get; set; }
        [Column("INTERFACE_FIELD_NAME")]
        [Required]
        public string InterfaceName { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public int InterfaceMajor { get; set; }
        [Required]
        public byte[] BsonValue { get; set; }
        public AstarteGenericPropertyEntry(string interfaceName, string path, byte[] bsonValue,
        int interfaceMajor)
        {
            Id = interfaceName + "/" + path;
            InterfaceName = interfaceName;
            Path = path;
            BsonValue = bsonValue;
            InterfaceMajor = interfaceMajor;
        }
    }
}
