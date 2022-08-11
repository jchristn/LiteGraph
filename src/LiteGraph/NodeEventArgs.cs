using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// Node event arguments.
    /// </summary>
    public class NodeEventArgs : EventArgs
    {
        #region Public-Members

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [JsonProperty(PropertyName = "guid", Order = -3)]
        public string GUID { get; private set; } = null;

        /// <summary>
        /// Name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = -2)]
        public string Name { get; private set; } = null;

        /// <summary>
        /// Node type.
        /// </summary>
        [JsonProperty(PropertyName = "guid", Order = -1)]
        public string NodeType { get; private set; } = null;

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
        public NodeEventArgs()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="type">Node type.</param>
        /// <param name="created">Timestamp from creation, in UTC.</param>
        /// <param name="props">JSON properties.</param>
        public NodeEventArgs(string guid, string name, string type, DateTime created, JObject props)
        {
            GUID = guid;
            Name = name;
            NodeType = type;
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
