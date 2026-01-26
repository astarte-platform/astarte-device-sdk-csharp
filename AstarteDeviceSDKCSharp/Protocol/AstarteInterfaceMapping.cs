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

using AstarteDeviceSDKCSharp.Protocol.AstarteException;

namespace AstarteDeviceSDK.Protocol
{
    public class AstarteInterfaceMapping
    {
        public string? Path { get; set; }
        public Type? MapType { get; set; }
        public Type? PrimitiveArrayType { get; set; }
        public bool AllowUnset { get; set; } = false;

        public static AstarteInterfaceMapping FromAstarteInterfaceMapping(Mapping astarteMapping)
        {
            AstarteInterfaceMapping astarteInterfaceMapping = new();
            astarteInterfaceMapping.ParseMappingFromAstarteInterface(astarteMapping);
            return astarteInterfaceMapping;
        }

        protected void ParseMappingFromAstarteInterface(Mapping astarteMappingObject)
        {
            Path = astarteMappingObject.Endpoint;
            MapType = StringToCSharpType(astarteMappingObject.Type);
            PrimitiveArrayType = StringToPrimitiveArrayCSharpType(astarteMappingObject.Type);
            AllowUnset = astarteMappingObject.AllowUnset;
        }

        public string? GetPath()
        {
            return Path;
        }

        public Type? GetTypeMapping()
        {
            return MapType;
        }

        public bool IsTypeCompatible(Type otherType)
        {
            return otherType == MapType || otherType == PrimitiveArrayType;
        }

        public virtual void ValidatePayload(Object payload, DateTime? timestamp)
        {
            ValidatePayload(payload);
        }

        public void ValidatePayload(Object payload)
        {
            if (!IsTypeCompatible(payload.GetType()))
            {
                throw new AstarteInvalidValueException(
                        $"Value incompatible with parameter type for {Path}: {MapType} expected," +
                        $" {payload.GetType()} found");
            }
            if (payload.GetType() == typeof(Double) && !IsFinite((Double)payload))
            {
                throw new AstarteInvalidValueException(
                    $"Value per {Path} cannot be NaN");
            }
            if (payload.GetType() == typeof(Double[]))
            {

                Double[] arrayPayload = (Double[])payload;

                foreach (Double value in arrayPayload)
                {
                    if (!IsFinite(value))
                    {
                        throw new AstarteInvalidValueException(
                            $"Value per {Path} cannot be NaN");
                    }
                }
            }
            if (payload.GetType() == typeof(double))
            {
                double doublePayload = (double)payload;
                if (!IsFinite(doublePayload))
                {
                    throw new AstarteInvalidValueException(
                            $"Value per {Path} cannot be NaN");
                }
            }
        }

        private static bool IsFinite(Double value)
        {
            return !(Double.IsInfinity(value) || Double.IsNaN(value));
        }

        private static Type StringToCSharpType(String typeString)
        {
            switch (typeString)
            {
                case "string":
                    return typeof(String);
                case "integer":
                    return typeof(Int32);
                case "double":
                    return typeof(Double);
                case "longinteger":
                    return typeof(Int64);
                case "boolean":
                    return typeof(Boolean);
                case "binaryblob":
                    return typeof(Byte[]);
                case "datetime":
                    return typeof(DateTime);
                case "stringarray":
                    return typeof(String[]);
                case "integerarray":
                    return typeof(Int32[]);
                case "doublearray":
                    return typeof(Double[]);
                case "longintegerarray":
                    return typeof(Int64[]);
                case "booleanarray":
                    return typeof(Boolean[]);
                case "binaryblobarray":
                    return typeof(Byte[][]);
                case "datetimearray":
                    return typeof(DateTime[]);
                default:
                    return typeof(Object);
            }
        }

        private static Type? StringToPrimitiveArrayCSharpType(string typeString)
        {
            switch (typeString)
            {
                case "binaryblob":
                    return typeof(byte[]);
                case "integerarray":
                    return typeof(int[]);
                case "doublearray":
                    return typeof(double[]);
                case "longintegerarray":
                    return typeof(long[]);
                case "booleanarray":
                    return typeof(bool[]);
                case "binaryblobarray":
                    return typeof(byte[][]);
                default:
                    return null;
            }
        }

    }
}
