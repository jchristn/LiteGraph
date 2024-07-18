namespace LiteGraph.Repositories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using ExpressionTree;
    using LiteGraph;
    using LiteGraph.Serialization;
    using Microsoft.Data.Sqlite;

    /// <summary>
    /// Repository base class.
    /// </summary>
    public abstract class RepositoryBase
    {
        #region Public-Members

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LoggingSettings Logging
        {
            get
            {
                return _Logging;
            }
            set
            {
                if (value == null) value = new LoggingSettings();
                _Logging = value;
            }
        }

        /// <summary>
        /// Serialization helper.
        /// </summary>
        public ISerializer Serializer
        {
            get
            {
                return _Serializer;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Serializer));
                _Serializer = value;
            }
        }

        #endregion

        #region Private-Members

        private LoggingSettings _Logging = new LoggingSettings();
        private ISerializer _Serializer = new SerializationHelper();

        #endregion

        #region Public-Methods

        #region General

        /// <inheritdoc />
        public abstract void InitializeRepository();

        #endregion

        #region Graphs

        /// <summary>
        /// Create a graph using a unique name.
        /// </summary>
        /// <param name="name">Unique name.</param>
        /// <param name="data">Data.</param>
        /// <returns>Graph.</returns>
        public abstract Graph CreateGraph(string name, object data = null);

        /// <summary>
        /// Read graphs.
        /// </summary>
        /// <param name="graphFilter">
        /// Graph filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Graphs.</returns>
        public abstract IEnumerable<Graph> ReadGraphs(
            Expr graphFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Read a graph by GUID.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <returns>Graph.</returns>
        public abstract Graph ReadGraph(Guid guid);

        /// <summary>
        /// Update a graph.
        /// </summary>
        /// <param name="graph">Graph.</param>
        /// <returns>Graph.</returns>
        public abstract Graph UpdateGraph(Graph graph);

        /// <summary>
        /// Delete a graph.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="force">True to force deletion of nodes and edges.</param>
        public abstract void DeleteGraph(Guid guid, bool force = false);

        /// <summary>
        /// Check if a graph exists by GUID.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsGraph(Guid guid);

        #endregion

        #region Nodes

        /// <summary>
        /// Create a node.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <returns>Node.</returns>
        public abstract Node CreateNode(Node node);

        /// <summary>
        /// Read nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeFilter">
        /// Node filter expression for Data JSON body.  
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> ReadNodes(
            Guid graphGuid,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Read node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <returns>Node.</returns>
        public abstract Node ReadNode(Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Update node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Node.</returns>
        public abstract Node UpdateNode(Node node);

        /// <summary>
        /// Delete a node and all associated edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        public abstract void DeleteNode(Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Delete all nodes from a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteNodes(Guid graphGuid);

        /// <summary>
        /// Check existence of a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsNode(Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Get nodes that have edges connecting to the specified node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> GetParents(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Get nodes to which the specified node has connecting edges connecting.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> GetChildren(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Get neighbors for a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="nodeFilter">
        /// Node filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> GetNeighbors(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Get routes between two nodes.
        /// </summary>
        /// <param name="searchType">Search type.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="nodeFilter">
        /// Node filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <returns>Route details.</returns>
        public abstract IEnumerable<RouteDetail> GetRoutes(
            SearchTypeEnum searchType,
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null);

        #endregion

        #region Edges

        /// <summary>
        /// Create an edge between two nodes.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <returns>Edge.</returns>
        public abstract Edge CreateEdge(Edge edge);

        /// <summary>
        /// Read edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> ReadEdges(
            Guid graphGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Read edge.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <returns>Edge.</returns>
        public abstract Edge ReadEdge(Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Update edge.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <returns>Edge.</returns>
        public abstract Edge UpdateEdge(Edge edge);

        /// <summary>
        /// Delete edge.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        public abstract void DeleteEdge(Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Delete all edges from a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteEdges(Guid graphGuid);

        /// <summary>
        /// Check if an edge exists by GUID.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsEdge(Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Get edges connected to or initiated from a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetConnectedEdges(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Get edges from a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetEdgesFrom(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Get edges to a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetEdgesTo(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        /// <summary>
        /// Get edges between two neighboring nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetEdgesBetween(
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending);

        #endregion

        #endregion

        #region Private-Methods

        #endregion
    }
}
