using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// Edge in the graph.
    /// </summary>
    public class Edge
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
        [JsonProperty(PropertyName = "guid", Order = -5)]
        public string GUID { get; set; } = null;

        /// <summary>
        /// Edge type.
        /// </summary>
        [JsonProperty(PropertyName = "type", Order = -4)]
        public string EdgeType { get; set; } = null;

        /// <summary>
        /// Globally-unique identifier of the from node.
        /// </summary>
        [JsonProperty(PropertyName = "from", Order = -3)]
        public string FromGUID { get; set; } = null;

        /// <summary>
        /// Globally-unique identifier of the to node.
        /// </summary>
        [JsonProperty(PropertyName = "to", Order = -2)]
        public string ToGUID { get; set; } = null;

        /// <summary>
        /// Cost.
        /// </summary>
        [JsonProperty(PropertyName = "cost", Order = -1)]
        public int? Cost { get; set; } = null;

        /// <summary>
        /// Timestamp from creation, in UTC.
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime CreatedUtc { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// JSON properties.
        /// </summary>
        [JsonProperty(PropertyName = "props", Order = 990)]
        public JObject Properties { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Edge()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="id">Database row ID.</param>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="type">Edge type.</param>
        /// <param name="fromGuid">Globally-unique identifier of the from node.</param>
        /// <param name="toGuid">Globally-unique identifier of the to node.</param>
        /// <param name="cost">Cost.</param>
        /// <param name="createdUtc">Timestamp from creation, in UTC.</param>
        /// <param name="props">JSON properties.</param>
        public Edge(int id, string guid, string type, string fromGuid, string toGuid, int? cost, DateTime createdUtc, JObject props)
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
            Cost = cost;
            CreatedUtc = createdUtc.ToUniversalTime();
            Properties = props;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
