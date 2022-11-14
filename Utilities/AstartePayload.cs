using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace AstarteDeviceSDKCSharp.Utilities
{

    internal class AstartePayloadItem
    {
        [JsonProperty("v")]
        public object Value { get; set; }
        [JsonProperty("t")]
        public DateTime? TimeStamp { get; set; }

        public AstartePayloadItem(object value, DateTime? timeStamp)
        {
            Value = value;
            if (timeStamp.HasValue) 
            {
                TimeStamp = timeStamp;
            }
        }
    }

    public class AstartePayload
    {
        public AstartePayload() { }
        public static byte[] Serialize(Object o, DateTime t)
        {
            if (o is not { })
            {
                return Array.Empty<byte>();
            }

            o = PrepareDateTimeValues(o);

            AstartePayloadItem payloadObject = new(o, t);

            MemoryStream ms = new();
            using (BsonDataWriter writer = new(ms))
            {
                JsonSerializer serializer = new();
                serializer.Serialize(writer, payloadObject);
            }

            byte[] payload = ms.ToArray();
            return payload;
        }

        private static object PrepareDateTimeValues(object o)
        {
            if (o.GetType().IsInstanceOfType(typeof(DateTime)))
            {
                return (DateTime)o;
            }

            if (o.GetType().IsInstanceOfType(typeof(DateTime[])))
            {
                return DateTimesArrayToDateList((DateTime[])o);
            }

            if (o.GetType().IsInstanceOfType(typeof(Dictionary<string, object>))) 
            {
                Dictionary<string, object> aggregate = new((Dictionary<string, object>)o);
                foreach (KeyValuePair<string, object> entry in aggregate) 
                {
                    if (entry.Value.GetType().IsInstanceOfType(typeof(DateTime)))
                    {
                        aggregate[entry.Key] = (DateTime)entry.Value;
                    }
                    else if (entry.Value.GetType().IsInstanceOfType(typeof(DateTime[]))) 
                    {
                        aggregate[entry.Key] = (List<DateTime>)entry.Value;
                    }
                }
                return aggregate;
            }

            return o;

        }

        private static object DateTimesArrayToDateList(DateTime[] o)
        {
            List<DateTime> list = new();
            foreach (DateTime dt in o) 
            {
                list.Add(dt);
            }
            return list; 
        }
    }
}
