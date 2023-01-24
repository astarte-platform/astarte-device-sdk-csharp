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

using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace AstarteDeviceSDKCSharp.Utilities
{
    public class AstarteDeviceIdUtils
    {
        /// <summary>
        /// Generate a device Id in a random way based on Guid.
        /// </summary>
        /// <returns>Generated device Id, using the standard Astarte Device ID encoding (base64 urlencoding without padding)</returns>
        public static string GenerateId()
        {
            Uuid randomUUID = Uuid.NewUuid();

            var stream = new MemoryStream();
            var buffer = new BinaryWriter(stream);
            buffer.Write(randomUUID.ToByteArray());
            var encoded = Convert.ToBase64String(stream.ToArray()).TrimEnd('=');
            return encoded;
        }

        /// <summary>
        /// Generate a device Id based on Guid and a uniqueData.
        /// </summary>
        /// <param name="namespaceGuid">Guid namespace of the device_id</param>
        /// <param name="uniqueData">Device unique data used to generate the device_id</param>
        /// <returns>The generated device Id, using the standard Astarte Device ID encoding (base64 urlencoding without padding).</returns>
        public static string GenerateId(Guid namespaceGuid, string uniqueData)
        {
            Uuid uuid5FromName = NameUUIDFromNamespaceAndString(namespaceGuid, uniqueData);

            var stream = new MemoryStream();
            var buffer = new BinaryWriter(stream);
            buffer.Write(uuid5FromName.LeastSignificantBits);
            buffer.Write(uuid5FromName.MostSignificantBits);
            var encodedStr = Base64UrlEncoder.Encode(stream.ToArray().Reverse().ToArray());
            return encodedStr;
        }

        private static Uuid NameUUIDFromNamespaceAndString(Uuid nameUuid, String data)
        {
            return NameUUIDFromNamespaceAndBytes(
                nameUuid, Encoding.UTF8.GetBytes(data.ToString()));
        }

        private static Uuid NameUUIDFromNamespaceAndBytes(Uuid namespaceGuid, byte[] data)
        {
            var namespaceBytes = ToBytes(namespaceGuid);
            byte[] hash;
            using (var sha1 = SHA1.Create())
            {
                hash = new byte[namespaceBytes.Length + data.Length];
                sha1.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, hash, 0);
                sha1.TransformFinalBlock(data, 0, data.Length);
                byte[]? sha1Bytes = sha1.Hash;

                if (sha1Bytes != null)
                {
                    sha1Bytes[6] &= 0x0f; /* clear version        */
                    sha1Bytes[6] |= 0x50; /* set to version 5     */
                    sha1Bytes[8] &= 0x3f; /* clear variant        */
                    sha1Bytes[8] |= 0x80; /* set to IETF variant  */

                    return FromBytes(sha1Bytes);
                }
                else
                {
                    throw new ArgumentException("Computed hash code is null");
                }
            }
        }

        private static Uuid FromBytes(byte[] data)
        {
            // Based on the private UUID(bytes[]) constructor
            long msb = 0;
            long lsb = 0;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            for (int i = 0; i < 8; i++) msb = (msb << 8) | (data[i] & 0xff);
            for (int i = 8; i < 16; i++) lsb = (lsb << 8) | (data[i] & 0xff);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
            return new Uuid(msb, lsb);
        }

        private static byte[] ToBytes(Uuid uuid)
        {
            // inverted logic of fromBytes()
            byte[] outId = new byte[16];
            long msb = uuid.MostSignificantBits;
            long lsb = uuid.LeastSignificantBits;
            for (int i = 0; i < 8; i++) outId[i] = (byte)((msb >> ((7 - i) * 8)) & 0xff);
            for (int i = 8; i < 16; i++) outId[i] = (byte)((lsb >> ((15 - i) * 8)) & 0xff);
            return outId;
        }
    }
}
