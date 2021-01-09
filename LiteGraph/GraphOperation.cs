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
    /// Graph operation.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GraphOperation
    {
        #region Node

        /// <summary>
        /// AddNode.
        /// </summary>
        [EnumMember(Value = "AddNode")]
        AddNode,
        /// <summary>
        /// UpdateNode.
        /// </summary>
        [EnumMember(Value = "UpdateNode")]
        UpdateNode,
        /// <summary>
        /// RemoveNode.
        /// </summary>
        [EnumMember(Value = "RemoveNode")]
        RemoveNode,
        /// <summary>
        /// NodeExists.
        /// </summary>
        [EnumMember(Value = "NodeExists")]
        NodeExists,
        /// <summary>
        /// GetAllNodes.
        /// </summary>
        [EnumMember(Value = "GetAllNodes")]
        GetAllNodes,
        /// <summary>
        /// GetNode.
        /// </summary>
        [EnumMember(Value = "GetNode")]
        GetNode,
        /// <summary>
        /// BreadthFirstSearch.
        /// </summary>
        [EnumMember(Value = "GetNeighbors")]
        GetNeighbors,
        /// <summary>
        /// SearchNodes.
        /// </summary>
        [EnumMember(Value = "SearchNodes")]
        SearchNodes,
        /// <summary>
        /// GetDescendants.
        /// </summary>
        [EnumMember(Value = "GetDescendants")]
        GetDescendants,

        #endregion

        #region Edge

        /// <summary>
        /// AddEdge.
        /// </summary>
        [EnumMember(Value = "AddEdge")]
        AddEdge,
        /// <summary>
        /// UpdateEdge.
        /// </summary>
        [EnumMember(Value = "UpdateEdge")]
        UpdateEdge,
        /// <summary>
        /// RemoveEdge.
        /// </summary>
        [EnumMember(Value = "RemoveEdge")]
        RemoveEdge,
        /// <summary>
        /// EdgeExists.
        /// </summary>
        [EnumMember(Value = "EdgeExists")]
        EdgeExists,
        /// <summary>
        /// GetAllEdges.
        /// </summary>
        [EnumMember(Value = "GetAllEdges")]
        GetAllEdges,
        /// <summary>
        /// GetEdges.
        /// </summary>
        [EnumMember(Value = "GetEdges")]
        GetEdges,
        /// <summary>
        /// GetEdge.
        /// </summary>
        [EnumMember(Value = "GetEdge")]
        GetEdge,
        /// <summary>
        /// SearchEdges.
        /// </summary>
        [EnumMember(Value = "SearchEdges")]
        SearchEdges

        #endregion
    }
}
