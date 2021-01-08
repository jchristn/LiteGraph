using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    internal class Node
    {
        [JsonIgnore]
        internal int Id { get; set; } = 0;

        [JsonProperty(PropertyName = "guid", Order = -1)]
        internal string GUID { get; set; } = null;

        [JsonProperty(PropertyName = "created")]
        internal DateTime CreatedUtc { get; set; } = DateTime.Now.ToUniversalTime();

        [JsonProperty(PropertyName = "props", Order = 990)]
        internal JObject Properties { get; set; } = null;

        internal Node()
        {

        }

        internal Node(int id, string guid, DateTime createdUtc, string props)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (id < 0) throw new ArgumentException("Id must be zero or greater.");

            Id = id;
            GUID = guid;
            CreatedUtc = createdUtc.ToUniversalTime();
            Properties = JObject.Parse(props);
        }
    }
}
