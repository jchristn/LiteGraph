using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LiteGraph
{
    /// <summary>
    /// Total cost and ordered list of edges between two nodes.
    /// </summary>
    public class RouteDetail
    {
        #region Public-Members

        /// <summary>
        /// Total cost of the route.
        /// </summary>
        [JsonProperty(PropertyName = "total_cost", Order = -1)]
        public int? TotalCost { get; set; } = null;

        /// <summary>
        /// Ordered list of edges that must be traversed.
        /// </summary>
        [JsonProperty(PropertyName = "edges", Order = 990)]
        public List<Edge> Edges { get; set; } = new List<Edge>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RouteDetail()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
