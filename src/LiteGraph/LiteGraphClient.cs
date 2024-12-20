﻿namespace LiteGraph
{
    using System;
    using ExpressionTree;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;
    using LiteGraph.Gexf;
    using LiteGraph.Repositories;
    using LiteGraph.Serialization;

    /// <summary>
    /// LiteGraph client.
    /// </summary>
    public class LiteGraphClient : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LoggingSettings Logging
        {
            get
            {
                return _Repository.Logging;
            }
            set
            {
                if (value == null) value = new LoggingSettings();
                _Repository.Logging = value;
            }
        }

        /// <summary>
        /// Serialization helper.
        /// </summary>
        public SerializationHelper Serializer
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

        private bool _Disposed = false;
        private RepositoryBase _Repository = new SqliteRepository();
        private SerializationHelper _Serializer = new SerializationHelper();
        private GexfWriter _Gexf = new GexfWriter();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate LiteGraph client.
        /// </summary>
        /// <param name="repository">Repository driver.</param>
        /// <param name="logging">Logging.</param>
        public LiteGraphClient(
            RepositoryBase repository = null,
            LoggingSettings logging = null)
        {
            if (repository != null) _Repository = repository;

            if (logging != null) Logging = logging;
            else Logging = new LoggingSettings();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Tear down the client and dispose of resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize the repository.
        /// </summary>
        public void InitializeRepository()
        {
            _Repository.InitializeRepository();
        }

        /// <summary>
        /// Convert data associated with a graph, node, or edge to a specific type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="data">Data.</param>
        /// <returns>Instance.</returns>
        public T ConvertData<T>(object data) where T : class, new()
        {
            if (data == null) return null;
            return _Serializer.DeserializeJson<T>(data.ToString());
        }

        #region Graphs

        /// <summary>
        /// Create a graph using a unique name.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="name">Unique name.</param>
        /// <param name="data">Data.</param>
        /// <returns>Graph.</returns>
        public Graph CreateGraph(Guid guid, string name, object data = null)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Graph existing = _Repository.ReadGraph(guid);
            if (existing != null) return existing;

            Graph graph = _Repository.CreateGraph(guid, name, data);
            Logging.Log(SeverityEnum.Info, "created graph name " + name + " GUID " + graph.GUID);
            return graph;
        }

        /// <summary>
        /// Read graphs.
        /// </summary>
        /// <param name="expr">
        /// Graph filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Graphs.</returns>
        public IEnumerable<Graph> ReadGraphs(
            Expr expr = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            if (order == EnumerationOrderEnum.CostAscending
                || order == EnumerationOrderEnum.CostDescending)
                throw new ArgumentException("Cost-based enumeration orders are only available to edge APIs.");

            Logging.Log(SeverityEnum.Debug, "retrieving graphs");

            foreach (Graph graph in _Repository.ReadGraphs(expr, order))
            {
                yield return graph;
            }
        }

        /// <summary>
        /// Read a graph by GUID.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <returns>Graph.</returns>
        public Graph ReadGraph(Guid graphGuid)
        {
            Logging.Log(SeverityEnum.Debug, "retrieving graph with GUID " + graphGuid);

            return _Repository.ReadGraph(graphGuid);
        }

        /// <summary>
        /// Update a graph.
        /// </summary>
        /// <param name="graph">Graph.</param>
        /// <returns>Graph.</returns>
        public Graph UpdateGraph(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            Logging.Log(SeverityEnum.Debug, "updating graph with name " + graph.Name + " GUID " + graph.GUID);

            return _Repository.UpdateGraph(graph);
        }

        /// <summary>
        /// Delete a graph.
        /// </summary>
        /// <param name="graphGuid">GUID.</param>
        /// <param name="force">True to force deletion of nodes and edges.</param>
        public void DeleteGraph(Guid graphGuid, bool force = false)
        {
            Graph graph = ReadGraph(graphGuid);
            if (graph == null) return;

            Logging.Log(SeverityEnum.Info, "deleting graph with name " + graph.Name + " GUID " + graph.GUID);

            if (force)
            {
                Logging.Log(SeverityEnum.Info, "deleting graph edges and nodes for graph GUID " + graph.GUID);

                _Repository.DeleteEdges(graph.GUID);
                _Repository.DeleteNodes(graph.GUID);
            }

            if (_Repository.ReadNodes(graph.GUID).Count() > 0)
                throw new InvalidOperationException("The specified graph has dependent nodes or edges.");

            if (_Repository.ReadEdges(graph.GUID).Count() > 0)
                throw new InvalidOperationException("The specified graph has dependent nodes or edges.");

            _Repository.DeleteGraph(graph.GUID, force);
        }

        /// <summary>
        /// Check if a graph exists by GUID.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsGraph(Guid guid)
        {
            return _Repository.ExistsGraph(guid);
        }

        /// <summary>
        /// Export graph to GEXF.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="filename">Filename.</param>
        /// <param name="includeData">True to include data.</param>
        public void ExportGraphToGexfFile(Guid guid, string filename, bool includeData = false)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            _Gexf.ExportToFile(this, guid, filename, includeData);
        }

        /// <summary>
        /// Render a graph as GEXF.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="includeData"></param>
        /// <returns></returns>
        public string RenderGraphAsGexf(Guid guid, bool includeData = false)
        {
            return _Gexf.RenderAsGexf(this, guid, includeData);
        }

        #endregion

        #region Nodes

        /// <summary>
        /// Create a node.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <returns>Node.</returns>
        public Node CreateNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (!_Repository.ExistsGraph(node.GraphGUID)) throw new ArgumentException("No graph with GUID '" + node.GraphGUID + "' exists.");
            if (_Repository.ExistsNode(node.GraphGUID, node.GUID)) throw new ArgumentException("A node with GUID '" + node.GUID + "' already exists in graph '" + node.GraphGUID + "'.");

            Node existing = _Repository.ReadNode(node.GraphGUID, node.GUID);
            if (existing != null) return existing;

            Node created = _Repository.CreateNode(node);

            Logging.Log(SeverityEnum.Debug, "created node " + created.GUID + " in graph " + created.GraphGUID);

            return created;
        }

        /// <summary>
        /// Create nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edges">Nodes.</param>
        /// <returns>Nodes.</returns>
        public List<Node> CreateNodes(Guid graphGuid, List<Node> edges)
        {
            if (edges == null) throw new ArgumentNullException(nameof(edges));

            if (!_Repository.ExistsGraph(graphGuid)) throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");

            List<Node> created = _Repository.CreateMultipleNodes(graphGuid, edges);
            Logging.Log(SeverityEnum.Debug, "created " + created.Count + " node(s) in graph " + graphGuid);

            return created;
        }

        /// <summary>
        /// Read nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="expr">
        /// Node filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Nodes.</returns>
        public IEnumerable<Node> ReadNodes(
            Guid graphGuid,
            Expr expr = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            if (order == EnumerationOrderEnum.CostAscending
                || order == EnumerationOrderEnum.CostDescending)
                throw new ArgumentException("Cost-based enumeration orders are only available to edge APIs.");

            foreach (Node node in _Repository.ReadNodes(graphGuid, expr, order))
            {
                yield return node;
            }
        }

        /// <summary>
        /// Read node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <returns>Node.</returns>
        public Node ReadNode(Guid graphGuid, Guid nodeGuid)
        {
            return _Repository.ReadNode(graphGuid, nodeGuid);
        }

        /// <summary>
        /// Update node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Node.</returns>
        public Node UpdateNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (!_Repository.ExistsGraph(node.GraphGUID)) throw new ArgumentException("No graph with GUID '" + node.GraphGUID + "' exists.");
            if (!_Repository.ExistsNode(node.GraphGUID, node.GUID)) throw new ArgumentException("No node with GUID '" + node.GUID + "' exists in graph '" + node.GraphGUID + "'.");

            Node updated = _Repository.UpdateNode(node);

            Logging.Log(SeverityEnum.Debug, "updated node " + updated.GUID + " in graph " + updated.GraphGUID);

            return updated;
        }

        /// <summary>
        /// Delete a node and all associated edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        public void DeleteNode(Guid graphGuid, Guid nodeGuid)
        {
            Node node = _Repository.ReadNode(graphGuid, nodeGuid);
            if (node != null)
            {
                Logging.Log(SeverityEnum.Info, "deleting edges connected to node " + nodeGuid + " in graph " + graphGuid);

                foreach (Edge edge in _Repository.GetConnectedEdges(graphGuid, nodeGuid))
                    _Repository.DeleteEdge(graphGuid, edge.GUID);

                Logging.Log(SeverityEnum.Info, "deleting node " + nodeGuid + " in graph " + graphGuid);

                _Repository.DeleteNode(graphGuid, nodeGuid);
            }
        }

        /// <summary>
        /// Delete all nodes associated with a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        public void DeleteNodes(Guid graphGuid)
        {
            _Repository.DeleteNodes(graphGuid);
        }

        /// <summary>
        /// Delete specific nodes associated with a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">Node GUIDs.</param>
        public void DeleteNodes(Guid graphGuid, List<Guid> nodeGuids)
        {
            _Repository.DeleteNodes(graphGuid, nodeGuids);
        }

        /// <summary>
        /// Check existence of a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsNode(Guid graphGuid, Guid nodeGuid)
        {
            return _Repository.ExistsNode(graphGuid, nodeGuid);
        }

        #endregion

        #region Edges

        /// <summary>
        /// Create an edge between two nodes.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <returns>Edge.</returns>
        public Edge CreateEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));

            if (!_Repository.ExistsGraph(edge.GraphGUID)) throw new ArgumentException("No graph with GUID '" + edge.GraphGUID + "' exists.");
            if (_Repository.ExistsEdge(edge.GraphGUID, edge.GUID)) throw new ArgumentException("An edge with GUID '" + edge.GUID + "' already exists in graph '" + edge.GraphGUID + "'.");

            if (!_Repository.ExistsNode(edge.GraphGUID, edge.From)) throw new ArgumentException("No node with GUID '" + edge.From + "' exists in graph '" + edge.GraphGUID + "'");
            if (!_Repository.ExistsNode(edge.GraphGUID, edge.To)) throw new ArgumentException("No node with GUID '" + edge.To + "' exists in graph '" + edge.GraphGUID + "'");

            Edge existing = _Repository.ReadEdge(edge.GraphGUID, edge.GUID);
            if (existing != null) return existing;

            Edge created = _Repository.CreateEdge(edge);

            Logging.Log(SeverityEnum.Debug, "created edge " + created.GUID + " in graph " + created.GraphGUID);

            return created;
        }

        /// <summary>
        /// Create edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edges">Edges.</param>
        /// <returns>Edges.</returns>
        public List<Edge> CreateEdges(Guid graphGuid, List<Edge> edges)
        {
            if (edges == null) throw new ArgumentNullException(nameof(edges));

            if (!_Repository.ExistsGraph(graphGuid)) throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");

            List<Edge> created = _Repository.CreateMultipleEdges(graphGuid, edges);
            Logging.Log(SeverityEnum.Debug, "created " + created.Count + " edges(s) in graph " + graphGuid);

            return created;
        }

        /// <summary>
        /// Create an edge between two nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNode">From node.</param>
        /// <param name="toNode">To node.</param>
        /// <param name="name">Name.</param>
        /// <param name="cost">Cost.</param>
        /// <param name="data">Data.</param>
        /// <returns>Edge.</returns>
        public Edge CreateEdge(
            Guid graphGuid,
            Node fromNode,
            Node toNode,
            string name,
            int cost = 0,
            object data = null)
        {
            if (fromNode == null) throw new ArgumentNullException(nameof(fromNode));
            if (toNode == null) throw new ArgumentNullException(nameof(toNode));
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Edge edge = new Edge
            {
                GraphGUID = graphGuid,
                From = fromNode.GUID,
                Name = name,
                To = toNode.GUID,
                Cost = cost,
                Data = data
            };

            if (!_Repository.ExistsGraph(edge.GraphGUID)) throw new ArgumentException("No graph with GUID '" + edge.GraphGUID + "' exists.");
            if (_Repository.ExistsEdge(edge.GraphGUID, edge.GUID)) throw new ArgumentException("An edge with GUID '" + edge.GUID + "' already exists in graph '" + edge.GraphGUID + "'.");

            if (!_Repository.ExistsNode(edge.GraphGUID, edge.From)) throw new ArgumentException("No node with GUID '" + edge.From + "' exists in graph '" + edge.GraphGUID + "'");
            if (!_Repository.ExistsNode(edge.GraphGUID, edge.To)) throw new ArgumentException("No node with GUID '" + edge.To + "' exists in graph '" + edge.GraphGUID + "'");

            Edge created = _Repository.CreateEdge(edge);

            Logging.Log(SeverityEnum.Debug, "created edge " + created.GUID + " in graph " + created.GraphGUID);

            return created;
        }

        /// <summary>
        /// Read edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="expr">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public IEnumerable<Edge> ReadEdges(
            Guid graphGuid,
            Expr expr = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            foreach (Edge edge in _Repository.ReadEdges(graphGuid, expr, order))
            {
                yield return edge;
            }
        }

        /// <summary>
        /// Read edge.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <returns>Edge.</returns>
        public Edge ReadEdge(Guid graphGuid, Guid edgeGuid)
        {
            return _Repository.ReadEdge(graphGuid, edgeGuid);
        }

        /// <summary>
        /// Update edge.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <returns>Edge.</returns>
        public Edge UpdateEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));

            if (!_Repository.ExistsGraph(edge.GraphGUID)) throw new ArgumentException("No graph with GUID '" + edge.GraphGUID + "' exists.");
            if (!_Repository.ExistsEdge(edge.GraphGUID, edge.GUID)) throw new ArgumentException("No edge with GUID '" + edge.GUID + "' exists in graph '" + edge.GraphGUID + "'");

            if (!_Repository.ExistsNode(edge.GraphGUID, edge.From)) throw new ArgumentException("No node with GUID '" + edge.From + "' exists in graph '" + edge.GraphGUID + "'");
            if (!_Repository.ExistsNode(edge.GraphGUID, edge.To)) throw new ArgumentException("No node with GUID '" + edge.To + "' exists in graph '" + edge.GraphGUID + "'");

            Edge updated = _Repository.UpdateEdge(edge);

            Logging.Log(SeverityEnum.Debug, "updated edge " + updated.GUID + " in graph " + updated.GraphGUID);

            return updated;
        }

        /// <summary>
        /// Delete edge.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        public void DeleteEdge(Guid graphGuid, Guid edgeGuid)
        {
            Edge edge = _Repository.ReadEdge(graphGuid, edgeGuid);
            if (edge != null)
            {
                _Repository.DeleteEdge(graphGuid, edgeGuid);
                Logging.Log(SeverityEnum.Debug, "deleted edge " + edgeGuid + " in graph " + graphGuid);
            }
        }

        /// <summary>
        /// Delete all edges associated with a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        public void DeleteEdges(Guid graphGuid)
        {
            _Repository.DeleteEdges(graphGuid);
        }

        /// <summary>
        /// Delete specific edges associated with a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuids">Edge GUIDs.</param>
        public void DeleteEdges(Guid graphGuid, List<Guid> edgeGuids)
        {
            _Repository.DeleteEdges(graphGuid, edgeGuids);
        }

        /// <summary>
        /// Check if an edge exists by GUID.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsEdge(Guid graphGuid, Guid edgeGuid)
        {
            return _Repository.ExistsEdge(graphGuid, edgeGuid);
        }

        #endregion

        #region Batch

        /// <summary>
        /// Batch existence check.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="req">Existence request.</param>
        /// <returns>Existence result.</returns>
        public ExistenceResult BatchExistence(Guid graphGuid, ExistenceRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (!req.ContainsExistenceRequest()) throw new ArgumentException("Supplied existence request contains no valid existence filters.");
            return _Repository.BatchExistence(graphGuid, req);
        }

        #endregion

        #region Routes-and-Traversal

        /// <summary>
        /// Get parents for a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Nodes.</returns>
        public IEnumerable<Node> GetParents(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            if (order == EnumerationOrderEnum.CostAscending
                || order == EnumerationOrderEnum.CostDescending)
                throw new ArgumentException("Cost-based enumeration orders are only available to edge APIs.");

            foreach (Node node in _Repository.GetParents(graphGuid, nodeGuid, edgeFilter, order))
            {
                yield return node;
            }
        }

        /// <summary>
        /// Get children for a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Nodes.</returns>
        public IEnumerable<Node> GetChildren(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            if (order == EnumerationOrderEnum.CostAscending
                || order == EnumerationOrderEnum.CostDescending)
                throw new ArgumentException("Cost-based enumeration orders are only available to edge APIs.");

            foreach (Node node in _Repository.GetChildren(graphGuid, nodeGuid, edgeFilter, order))
            {
                yield return node;
            }
        }

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
        public IEnumerable<Node> GetNeighbors(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            if (order == EnumerationOrderEnum.CostAscending
                || order == EnumerationOrderEnum.CostDescending)
                throw new ArgumentException("Cost-based enumeration orders are only available to edge APIs.");

            foreach (Node node in _Repository.GetNeighbors(graphGuid, nodeGuid, edgeFilter, nodeFilter, order))
            {
                yield return node;
            }
        }

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
        public IEnumerable<RouteDetail> GetRoutes(
            SearchTypeEnum searchType,
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null)
        {
            foreach (RouteDetail route in _Repository.GetRoutes(
                searchType,
                graphGuid,
                fromNodeGuid,
                toNodeGuid,
                edgeFilter,
                nodeFilter))
            {
                yield return route;
            }
        }

        /// <summary>
        /// Get edges from a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public IEnumerable<Edge> GetEdgesFrom(
            Guid graphGuid,
            Guid fromNodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            foreach (Edge edge in _Repository.GetEdgesFrom(graphGuid, fromNodeGuid, edgeFilter, order))
            {
                yield return edge;
            }
        }

        /// <summary>
        /// Get edges to a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <returns>Edges.</returns>
        public IEnumerable<Edge> GetEdgesTo(
            Guid graphGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            return _Repository.GetEdgesTo(graphGuid, toNodeGuid, edgeFilter, order);
        }

        /// <summary>
        /// Get edges between two nodes.
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
        public IEnumerable<Edge> GetEdgesBetween(
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            foreach (Edge edge in _Repository.GetEdgesBetween(graphGuid, fromNodeGuid, toNodeGuid, edgeFilter, order))
            {
                yield return edge;
            }
        }

        #endregion

        #endregion

        #region Private-Methods

        /// <summary>
        /// Dispose of the object.
        /// </summary>
        /// <param name="disposing">Disposing of resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }

            if (disposing)
            {
                _Repository = null;
                Logging = null;
            }

            _Disposed = true;
        }

        #endregion
    }
}