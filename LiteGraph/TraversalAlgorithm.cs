using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace LiteGraph
{
    /// <summary>
    /// Graph traversal algorithm.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TraversalAlgorithm
    {
        /// <summary>
        /// BreadthFirstSearch
        /// </summary>
        [EnumMember(Value = "BreadthFirstSearch")]
        BreadthFirstSearch,
        /// <summary>
        /// DepthFirstSearch
        /// </summary>
        [EnumMember(Value = "DepthFirstSearch")]
        DepthFirstSearch
    }
}
