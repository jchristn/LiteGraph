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

        [JsonProperty(PropertyName = "guid", Order = -2)]
        internal string GUID { get; set; } = null;

        [JsonProperty(PropertyName = "type", Order = -1)]
        internal string NodeType { get; set; } = null;

        [JsonProperty(PropertyName = "created")]
        internal DateTime CreatedUtc { get; set; } = DateTime.Now.ToUniversalTime();
         
        [JsonProperty(PropertyName = "props", Order = 991)]
        internal JObject Properties { get; set; } = null;

        [JsonProperty(PropertyName = "descendents", Order = 992)]
        internal List<Node> Descendents { get; set; } = null;

        internal Node()
        {

        }

        internal Node(int id, string guid, string type, DateTime createdUtc, JObject props, List<Node> descendents = null)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
            if (id < 0) throw new ArgumentException("Id must be zero or greater.");

            Id = id;
            GUID = guid;
            NodeType = type;
            CreatedUtc = createdUtc.ToUniversalTime();
            Properties = props;
            Descendents = descendents;
        }
    }
}
