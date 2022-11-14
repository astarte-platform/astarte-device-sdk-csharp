using System.ComponentModel;
using AstarteDeviceSDK.Protocol;

namespace AstarteDeviceSDKCSharp.Protocol
{
    public class AstarteInterfaceDatastreamMapping :  AstarteInterfaceMapping
    {
        private bool explicitTimestamp;
        private MappingReliability reliability = MappingReliability.UNRELIABLE;
        private MappingRetention retention = MappingRetention.DISCARD;
        private int expiry;

        public enum MappingReliability
        {   
            [Description("unreliable")]
            UNRELIABLE,
            [Description("guaranteed")]
            GUARANTEED,
            [Description("unique")]
            UNIQUE
        }

        public enum MappingRetention
        {   
            [Description("discard")]
            DISCARD,
            [Description("volatile")]
            VOLATILE,
            [Description("stored")]
            STORED
        }

        public MappingReliability GetReliability()
        {
            return reliability;
        }



        internal static AstarteInterfaceDatastreamMapping FromAstarteInterfaceMappingMaps(Mapping astarteMappingObject)
        {
            AstarteInterfaceDatastreamMapping astarteInterfaceDatastreamMapping = new();

            if (astarteMappingObject.ExplicitTimestamp != null)
            {
                astarteInterfaceDatastreamMapping.explicitTimestamp = (bool)astarteMappingObject.ExplicitTimestamp;
            }

            if (astarteMappingObject.Reliability != null)
            {
                if(astarteMappingObject.Reliability == "unreliable"){
                    astarteInterfaceDatastreamMapping.reliability = MappingReliability.UNRELIABLE;
                }else if(astarteMappingObject.Reliability == "unreliable"){
                    astarteInterfaceDatastreamMapping.reliability = MappingReliability.GUARANTEED;
                }else if(astarteMappingObject.Reliability == "unique"){
                    astarteInterfaceDatastreamMapping.reliability = MappingReliability.UNIQUE;
                }

            }

            if (astarteMappingObject.Retention != null)
            {
                if(astarteMappingObject.Retention == "discard"){
                    astarteInterfaceDatastreamMapping.retention = MappingRetention.DISCARD;
                }else if(astarteMappingObject.Retention == "volatile"){
                    astarteInterfaceDatastreamMapping.retention = MappingRetention.VOLATILE;
                }else if(astarteMappingObject.Retention == "stored"){
                    astarteInterfaceDatastreamMapping.retention = MappingRetention.STORED;
                }
            }

            if (astarteMappingObject.Expiry != null)
            {
                astarteInterfaceDatastreamMapping.expiry = (int)astarteMappingObject.Expiry;
            }


            return astarteInterfaceDatastreamMapping;
        }
    }
}
