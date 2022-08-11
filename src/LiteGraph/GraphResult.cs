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
        [JsonProperty(PropertyName = "operation", Order = -2)]
        public GraphOperation Operation { get; set; } = GraphOperation.AddEdge;

        /// <summary>
        /// Time-related information for the result.
        /// </summary>
        [JsonProperty(PropertyName = "time", Order = -1)]
        public Timestamps Time { get; set; } = new Timestamps();

        /// <summary>
        /// Result.
        /// </summary>
        [JsonProperty(PropertyName = "result", Order = 990)]
        public object Result { get; set; } = null;

        /// <summary>
        /// Data.  Generally a JArray or JObject.
        /// </summary>
        [JsonProperty(PropertyName = "order", Order = 991)]
        public object Data { get; set; } = null;

        /// <summary>
        /// Routes.
        /// </summary>
        [JsonProperty(PropertyName = "routes", Order = 992)]
        public List<RouteDetail> Routes { get; set; } = null;

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
