using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// Node in the graph.
    /// </summary>
    public class Node
    {
        #region Public-Members

        /// <summary>
        /// Database row ID.
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; } = 0;

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [JsonProperty(PropertyName = "guid", Order = -3)]
        public string GUID { get; set; } = null;

        /// <summary>
        /// Name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = -2)]
        public string Name { get; set; } = null;

        /// <summary>
        /// Node type.
        /// </summary>
        [JsonProperty(PropertyName = "type", Order = -1)]
        public string NodeType { get; set; } = null;

        /// <summary>
        /// Timestamp from creation, in UTC.
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime CreatedUtc { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// JSON properties.
        /// </summary>
        [JsonProperty(PropertyName = "props", Order = 991)]
        public JObject Properties { get; set; } = null;

        /// <summary>
        /// Descendents.
        /// </summary>
        [JsonProperty(PropertyName = "descendents", Order = 992)]
        public List<Node> Descendents { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Node()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="id">Database row ID.</param>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="type">Node type.</param>
        /// <param name="createdUtc">Timestamp from creation, in UTC.</param>
        /// <param name="props">JSON properties.</param>
        /// <param name="descendents">Descendents.</param>
        public Node(int id, string guid, string name, string type, DateTime createdUtc, JObject props, List<Node> descendents = null)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
            if (id < 0) throw new ArgumentException("Id must be zero or greater.");

            Id = id;
            GUID = guid;
            Name = name;
            NodeType = type;
            CreatedUtc = createdUtc.ToUniversalTime();
            Properties = props;
            Descendents = descendents;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
