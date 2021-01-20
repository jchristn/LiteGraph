using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// Edge event arguments.
    /// </summary>
    public class EdgeEventArgs : EventArgs
    {
        #region Public-Members

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [JsonProperty(PropertyName = "guid", Order = -5)]
        public string GUID { get; private set; } = null;

        /// <summary>
        /// Edge type.
        /// </summary>
        [JsonProperty(PropertyName = "to", Order = -2)]
        public string EdgeType { get; private set; } = null;

        /// <summary>
        /// Globally-unique identifier of the from node.
        /// </summary>
        [JsonProperty(PropertyName = "from", Order = -4)]
        public string FromGUID { get; private set; } = null;

        /// <summary>
        /// Globally-unique identifier of the to node.
        /// </summary>
        [JsonProperty(PropertyName = "to", Order = -3)]
        public string ToGUID { get; private set; } = null;

        /// <summary>
        /// Cost.
        /// </summary>
        [JsonProperty(PropertyName = "cost", Order = -1)]
        public int? Cost { get; private set; } = null;

        /// <summary>
        /// Timestamp from creation, in UTC.
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime CreatedUtc { get; private set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// JSON properties.
        /// </summary>
        [JsonProperty(PropertyName = "props", Order = 990)]
        public JObject Properties { get; private set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public EdgeEventArgs()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="type">Edge type.</param>
        /// <param name="fromGuid">Globally-unique identifier of the from node.</param>
        /// <param name="toGuid">Globally-unique identifier of the to node.</param>
        /// <param name="cost">Cost for using this edge.</param>
        /// <param name="created">Timestamp from creation, in UTC.</param>
        /// <param name="props">JSON properties.</param>
        public EdgeEventArgs(string guid, string type, string fromGuid, string toGuid, int? cost, DateTime created, JObject props)
        {
            GUID = guid;
            EdgeType = type;
            FromGUID = fromGuid;
            ToGUID = toGuid;
            Cost = cost;
            CreatedUtc = created;
            Properties = props;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
