using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// Result from a graph operation.
    /// </summary>
    public class GraphResult
    {
        #region Public-Members

        /// <summary>
        /// Type of operation.
        /// </summary>
        [JsonProperty(Order = -1)]
        public GraphOperation Operation { get; set; } = GraphOperation.AddEdge;

        /// <summary>
        /// Time-related information for the result.
        /// </summary>
        public Timestamps Time { get; set; } = new Timestamps();

        /// <summary>
        /// Result.
        /// </summary>
        [JsonProperty(Order = 990)]
        public object Result { get; set; } = null;

        /// <summary>
        /// Data.  Generally a JArray or JObject.
        /// </summary>
        [JsonProperty(Order = 991)]
        public object Data { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="operation">Type of operation.</param>
        public GraphResult(GraphOperation operation = GraphOperation.AddNode)
        {
            Operation = operation;
        }
         
        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
