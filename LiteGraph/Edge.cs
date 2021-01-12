using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    internal class Edge
    {
        [JsonIgnore]
        internal int Id { get; set; } = 0;

        [JsonProperty(PropertyName = "guid", Order = -4)]
        internal string GUID { get; set; } = null;

        [JsonProperty(PropertyName = "type", Order = -3)]
        internal string EdgeType { get; set; } = null;

        [JsonProperty(PropertyName = "from", Order = -2)]
        internal string FromGUID { get; set; } = null;

        [JsonProperty(PropertyName = "to", Order = -1)]
        internal string ToGUID { get; set; } = null;

        [JsonProperty(PropertyName = "created")]
        internal DateTime CreatedUtc { get; set; } = DateTime.Now.ToUniversalTime();

        [JsonProperty(PropertyName = "props", Order = 990)]
        internal JObject Properties { get; set; } = null;

        internal Edge()
        {

        }

        internal Edge(int id, string guid, string type, string fromGuid, string toGuid, DateTime createdUtc, string props)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
            if (String.IsNullOrEmpty(toGuid)) throw new ArgumentNullException(nameof(toGuid));
            if (String.IsNullOrEmpty(fromGuid)) throw new ArgumentNullException(nameof(fromGuid));
            if (id < 0) throw new ArgumentException("Id must be zero or greater.");

            Id = id;
            GUID = guid;
            EdgeType = type;
            FromGUID = fromGuid;
            ToGUID = toGuid;
            CreatedUtc = createdUtc.ToUniversalTime();
            Properties = JObject.Parse(props);
        }
    }
}
