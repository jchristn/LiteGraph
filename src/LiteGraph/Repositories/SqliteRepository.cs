namespace LiteGraph.Repositories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Caching;
    using ExpressionTree;
    using LiteGraph;
    using LiteGraph.Serialization;
    using Microsoft.Data.Sqlite;
    using SQLitePCL;

    /// <summary>
    /// Sqlite repository.
    /// </summary>
    public class SqliteRepository : RepositoryBase
    {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

        // Helpful references for Sqlite JSON:
        // https://stackoverflow.com/questions/33432421/sqlite-json1-example-for-json-extract-set
        // https://www.sqlite.org/json1.html

        #region Public-Members

        /// <summary>
        /// Sqlite database filename.
        /// </summary>
        public string Filename
        {
            get
            {
                return _Filename;
            }
        }

        /// <summary>
        /// Maximum supported statement length.
        /// Default for Sqlite is 1000000 (see https://www.sqlite.org/limits.html).
        /// </summary>
        public int MaxStatementLength
        {
            get
            {
                return _MaxStatementLength;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxStatementLength));
                _MaxStatementLength = value;
            }
        }

        /// <summary>
        /// Number of records to retrieve for object list retrieval.
        /// </summary>
        public int SelectBatchSize
        {
            get
            {
                return _SelectBatchSize;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(SelectBatchSize));
                _SelectBatchSize = value;
            }
        }

        /// <summary>
        /// Timestamp format.
        /// </summary>
        public string TimestampFormat
        {
            get
            {
                return _TimestampFormat;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(TimestampFormat));
                string test = DateTime.UtcNow.ToString(value);
                _TimestampFormat = value;
            }
        }

        /// <summary>
        /// True to enable a full-text index over node and edge data properties.
        /// </summary>
        public bool IndexData { get; set; } = true;

        #endregion

        #region Private-Members

        private string _Filename = "litegraph.db";
        private string _ConnectionString = "Data Source=litegraph.db;Pooling=false";
        private int _MaxStatementLength = 1000000; // https://www.sqlite.org/limits.html
        private int _SelectBatchSize = 100;
        private string _TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";
        private readonly object _QueryLock = new object();
        private readonly object _CreateLock = new object();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="filename">Sqlite database filename.</param>
        public SqliteRepository(string filename = "litegraph.db")
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            _Filename = filename;
            _ConnectionString = "Data Source=" + filename + ";Pooling=false";
        }

        #endregion

        #region Public-Methods

        #region General

        /// <inheritdoc />
        public override void InitializeRepository()
        {
            CreateTablesAndIndices();
        }

        #endregion

        #region Graphs

        /// <inheritdoc />
        public override Graph CreateGraph(Guid guid, string name, object data = null)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            string createQuery = InsertGraphQuery(new Graph
            {
                GUID = guid,
                Name = name,
                Data = data,
                CreatedUtc = DateTime.UtcNow
            }); 
            
            DataTable createResult = null;
            Graph created = null;
            Graph existing = null;

            lock (_CreateLock)
            {
                existing = ReadGraph(guid);
                if (existing != null) return existing;
                createResult = Query(createQuery, true);
            }
            
            created = GraphFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override IEnumerable<Graph> ReadGraphs(
            Expr graphFilter = null, 
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            int skip = 0;

            while (true)
            {
                DataTable result = Query(SelectGraphsQuery(graphFilter, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    yield return GraphFromDataRow(result.Rows[i]);
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override Graph ReadGraph(Guid guid)
        {
            DataTable result = Query(SelectGraphQuery(guid));
            if (result != null && result.Rows.Count == 1) return GraphFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override Graph UpdateGraph(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            ValidateGraphExists(graph.GUID);
            Graph updated = GraphFromDataRow(Query(UpdateGraphQuery(graph), true).Rows[0]);
            return updated;
        }

        /// <inheritdoc />
        public override void DeleteGraph(Guid graphGuid, bool force = false)
        {
            Graph graph = ReadGraph(graphGuid);
            if (graph != null)
            {
                if (force)
                {
                    Query(DeleteGraphEdgesQuery(graph.GUID), true);
                    Query(DeleteGraphNodesQuery(graph.GUID), true);
                }

                Query(DeleteGraphQuery(graphGuid), true);
            }
        }

        /// <inheritdoc />
        public override bool ExistsGraph(Guid graphGuid)
        {
            if (ReadGraph(graphGuid) != null) return true;
            return false;
        }

        #endregion

        #region Nodes

        /// <inheritdoc />
        public override Node CreateNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            ValidateGraphExists(node.GraphGUID);

            string createQuery = InsertNodeQuery(node);
            DataTable createResult = null;
            Node created = null;
            Node existing = null;

            lock (_CreateLock)
            {
                existing = ReadNode(node.GraphGUID, node.GUID);
                if (existing != null) return existing;
                createResult = Query(createQuery, true);
            }

            created = NodeFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override List<Node> CreateMultipleNodes(Guid graphGuid, List<Node> nodes)
        {
            if (nodes == null || nodes.Count < 1) return null;
            ValidateGraphExists(graphGuid);

            foreach (Node node in nodes)
                node.GraphGUID = graphGuid;

            ExistenceRequest req = new ExistenceRequest
            {
                Nodes = nodes.Select(n => n.GUID).ToList()
            };

            ExistenceResult result = BatchExistence(graphGuid, req);
            if (result.ExistingNodes != null && result.ExistingNodes.Count > 0)
                throw new InvalidOperationException("The requested node GUIDs already exist: " + string.Join(",", result.ExistingNodes));

            string createQuery = InsertMultipleNodesQuery(nodes);
            string retrieveQuery = SelectMultipleNodesQuery(nodes.Select(n => n.GUID).ToList());
            DataTable createResult = null;
            DataTable retrieveResult = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
                retrieveResult = Query(retrieveQuery, true);
            }
                        
            return NodesFromDataTable(retrieveResult);
        }

        /// <inheritdoc />
        public override IEnumerable<Node> ReadNodes(
            Guid graphGuid,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable result = Query(SelectNodesQuery(graphGuid, nodeFilter, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    yield return NodeFromDataRow(result.Rows[i]);
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override Node ReadNode(Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(graphGuid);
            DataTable result = Query(SelectNodeQuery(graphGuid, nodeGuid));
            if (result != null && result.Rows.Count == 1) return NodeFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override Node UpdateNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            ValidateGraphExists(node.GraphGUID);
            return NodeFromDataRow(Query(UpdateNodeQuery(node), true).Rows[0]);
        }

        /// <inheritdoc />
        public override void DeleteNode(Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(graphGuid);
            Query(DeleteNodeQuery(graphGuid, nodeGuid), true);
        }

        /// <inheritdoc />
        public override void DeleteNodes(Guid graphGuid)
        {
            ValidateGraphExists(graphGuid);
            Query(DeleteNodesQuery(graphGuid), true);
            DeleteEdges(graphGuid);
        }

        /// <inheritdoc />
        public override void DeleteNodes(Guid graphGuid, List<Guid> nodeGuids)
        {
            if (nodeGuids == null || nodeGuids.Count < 1) return;
            ValidateGraphExists(graphGuid);
            Query(DeleteNodesQuery(graphGuid, nodeGuids), true);
            DeleteNodeEdges(graphGuid, nodeGuids);
        }

        /// <inheritdoc />
        public override void DeleteNodeEdges(Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(graphGuid);
            DeleteNodeEdges(graphGuid, new List<Guid> { nodeGuid });
        }

        /// <inheritdoc />
        public override void DeleteNodeEdges(Guid graphGuid, List<Guid> nodeGuids)
        {
            if (nodeGuids == null || nodeGuids.Count < 1) return;
            ValidateGraphExists(graphGuid);
            string query = DeleteNodeEdgesQuery(graphGuid, nodeGuids);

            lock (_CreateLock)
                Query(query);
        }

        /// <inheritdoc />
        public override bool ExistsNode(Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(graphGuid);
            if (ReadNode(graphGuid, nodeGuid) != null) return true;
            return false;
        }

        /// <inheritdoc />
        public override IEnumerable<Node> GetParents(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable allEdges = Query(SelectConnectedEdgesQuery(graphGuid, nodeGuid, edgeFilter, skip, order));
                if (allEdges == null || allEdges.Rows.Count < 1) break;

                for (int i = 0; i < allEdges.Rows.Count; i++)
                {
                    Edge edge = EdgeFromDataRow(allEdges.Rows[i]);
                    if (edge.To.Equals(nodeGuid))
                    {
                        Node parent = ReadNode(graphGuid, edge.From);
                        if (parent != null) yield return parent;
                        else Logging.Log(SeverityEnum.Warn,"node " + edge.From + " referenced in graph " + graphGuid + " but does not exist");
                    }

                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Node> GetChildren(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable allEdges = Query(SelectConnectedEdgesQuery(graphGuid, nodeGuid, edgeFilter, skip, order));
                if (allEdges == null || allEdges.Rows.Count < 1) break;

                for (int i = 0; i < allEdges.Rows.Count; i++)
                {
                    Edge edge = EdgeFromDataRow(allEdges.Rows[i]);
                    if (edge.From.Equals(nodeGuid))
                    {
                        Node child = ReadNode(graphGuid, edge.To);
                        if (child != null) yield return child;
                        else Logging.Log(SeverityEnum.Warn, "node " + edge.To + " referenced in graph " + graphGuid + " but does not exist");
                    }

                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Node> GetNeighbors(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            List<Guid> visited = new List<Guid>();

            while (true)
            {
                DataTable allEdges = Query(SelectConnectedEdgesQuery(graphGuid, nodeGuid, edgeFilter, skip, order));
                if (allEdges == null || allEdges.Rows.Count < 1) break;

                for (int i = 0; i < allEdges.Rows.Count; i++)
                {
                    Edge edge = EdgeFromDataRow(allEdges.Rows[i]);
                    if (edge.From.Equals(nodeGuid))
                    {
                        if (visited.Contains(edge.To))
                        {
                            skip++;
                            continue;
                        }
                        else
                        {
                            Node neighbor = ReadNode(graphGuid, edge.To);
                            if (neighbor != null)
                            {
                                visited.Add(edge.To);
                                yield return neighbor;
                            }
                            else Logging.Log(SeverityEnum.Warn, "node " + edge.From + " referenced in graph " + graphGuid + " but does not exist");
                            skip++;
                        }
                    }
                    if (edge.To.Equals(nodeGuid))
                    {
                        if (visited.Contains(edge.From))
                        {
                            skip++;
                            continue;
                        }
                        else
                        {
                            Node neighbor = ReadNode(graphGuid, edge.From);
                            if (neighbor != null)
                            {
                                visited.Add(edge.From);
                                yield return neighbor;
                            }
                            else Logging.Log(SeverityEnum.Warn, "node " + edge.From + " referenced in graph " + graphGuid + " but does not exist");
                            skip++;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<RouteDetail> GetRoutes(
            SearchTypeEnum searchType, 
            Guid graphGuid,
            Guid fromNodeGuid, 
            Guid toNodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null)
        {
            #region Retrieve-Objects

            Graph graph = ReadGraph(graphGuid);
            if (graph == null) throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");

            Node fromNode = ReadNode(graphGuid, fromNodeGuid);
            if (fromNode == null) throw new ArgumentException("No node with GUID '" + fromNodeGuid + "' exists in graph '" + graphGuid + "'");

            Node toNode = ReadNode(graphGuid, toNodeGuid);
            if (toNode == null) throw new ArgumentException("No node with GUID '" + toNodeGuid + "' exists in graph '" + graphGuid + "'");

            #endregion

            #region Perform-Search

            RouteDetail routeDetail = new RouteDetail();

            if (searchType == SearchTypeEnum.DepthFirstSearch)
            {
                foreach (RouteDetail route in GetRoutesDfs(
                    graph, 
                    fromNode, 
                    toNode,
                    edgeFilter, 
                    nodeFilter,
                    new List<Node>{ fromNode },
                    new List<Edge>()))
                {
                    if (route != null) yield return route;
                }
            }
            else
            {
                throw new ArgumentException("Unknown search type '" + searchType.ToString() + ".");
            }

            #endregion
        }

        #endregion

        #region Batch

        /// <inheritdoc />
        public override ExistenceResult BatchExistence(Guid graphGuid, ExistenceRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (!req.ContainsExistenceRequest()) throw new ArgumentException("Supplied existence request contains no valid existence filters.");

            Graph graph = ReadGraph(graphGuid);
            if (graph == null) throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");

            ExistenceResult resp = new ExistenceResult();

            #region Nodes

            if (req.Nodes != null)
            {
                resp.ExistingNodes = new List<Guid>();
                resp.MissingNodes = new List<Guid>();

                string nodesQuery = BatchExistsNodesQuery(graphGuid, req.Nodes);
                DataTable nodesResult = Query(nodesQuery);
                if (nodesResult != null && nodesResult.Rows != null && nodesResult.Rows.Count > 0)
                {
                    foreach (DataRow row in nodesResult.Rows)
                    {
                        if (row["exists"] != null && row["exists"] != DBNull.Value)
                        {
                            int exists = Convert.ToInt32(row["exists"]);
                            if (exists == 1)
                                resp.ExistingNodes.Add(Guid.Parse(row["id"].ToString()));
                            else
                                resp.MissingNodes.Add(Guid.Parse(row["id"].ToString()));
                        }
                    }
                }
            }

            #endregion

            #region Edges

            if (req.Edges != null)
            {
                resp.ExistingEdges = new List<Guid>();
                resp.MissingEdges = new List<Guid>();

                string edgesQuery = BatchExistsEdgesQuery(graphGuid, req.Edges);
                DataTable edgesResult = Query(edgesQuery);
                if (edgesResult != null && edgesResult.Rows != null && edgesResult.Rows.Count > 0)
                {
                    foreach (DataRow row in edgesResult.Rows)
                    {
                        if (row["exists"] != null && row["exists"] != DBNull.Value)
                        {
                            int exists = Convert.ToInt32(row["exists"]);
                            if (exists == 1)
                                resp.ExistingEdges.Add(Guid.Parse(row["id"].ToString()));
                            else
                                resp.MissingEdges.Add(Guid.Parse(row["id"].ToString()));
                        }
                    }
                }
            }

            #endregion

            #region Edges-Between

            if (req.EdgesBetween != null)
            {
                resp.ExistingEdgesBetween = new List<EdgeBetween>();
                resp.MissingEdgesBetween = new List<EdgeBetween>();

                string betweenQuery = BatchExistsEdgesBetweenQuery(graphGuid, req.EdgesBetween);
                DataTable betweenResult = Query(betweenQuery);
                if (betweenResult != null && betweenResult.Rows != null && betweenResult.Rows.Count > 0)
                {
                    foreach (DataRow row in betweenResult.Rows)
                    {
                        if (row["exists"] != null && row["exists"] != DBNull.Value)
                        {
                            int exists = Convert.ToInt32(row["exists"]);
                            if (exists == 1)
                                resp.ExistingEdgesBetween.Add(new EdgeBetween
                                {
                                    From = Guid.Parse(row["fromguid"].ToString()),
                                    To = Guid.Parse(row["toguid"].ToString())
                                });
                            else
                                resp.MissingEdgesBetween.Add(new EdgeBetween
                                {
                                    From = Guid.Parse(row["fromguid"].ToString()),
                                    To = Guid.Parse(row["toguid"].ToString())
                                });
                        }
                    }
                }
            }

            #endregion

            return resp;
        }

        #endregion

        #region Edges

        /// <inheritdoc />
        public override IEnumerable<Edge> GetConnectedEdges(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable result = Query(SelectConnectedEdgesQuery(graphGuid, nodeGuid, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> GetEdgesFrom(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable result = Query(SelectEdgesFromQuery(graphGuid, nodeGuid, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> GetEdgesTo(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable result = Query(SelectEdgesToQuery(graphGuid, nodeGuid, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> GetEdgesBetween(
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable result = Query(SelectEdgesBetweenQuery(graphGuid, fromNodeGuid, toNodeGuid, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override Edge CreateEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            ValidateGraphExists(edge.GraphGUID);

            string insertQuery = InsertEdgeQuery(edge);
            DataTable createResult = null;
            Edge created = null;
            Edge existing = null;

            lock (_CreateLock)
            {
                existing = ReadEdge(edge.GraphGUID, edge.GUID);
                if (existing != null) return existing;
                createResult = Query(insertQuery, true);
            }

            created = EdgeFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override List<Edge> CreateMultipleEdges(Guid graphGuid, List<Edge> edges)
        {
            if (edges == null || edges.Count < 1) return null;
            ValidateGraphExists(graphGuid);

            foreach (Edge edge in edges)
                edge.GraphGUID = graphGuid;

            ExistenceRequest req = new ExistenceRequest();
            req.Edges = edges.Select(n => n.GUID).ToList();
            req.Nodes = edges.Select(n => n.From).Concat(edges.Select(n => n.To)).Distinct().ToList();

            ExistenceResult result = BatchExistence(graphGuid, req);
            if (result.ExistingEdges != null && result.ExistingEdges.Count > 0)
                throw new InvalidOperationException("The requested edge GUIDs already exist: " + string.Join(",", result.ExistingEdges));

            if (result.MissingNodes != null && result.MissingNodes.Count > 0)
                throw new KeyNotFoundException("The specified node GUIDs do not exist: " + string.Join(",", result.MissingNodes));

            string createQuery = InsertMultipleEdgesQuery(edges);
            string retrieveQuery = SelectMultipleEdgesQuery(edges.Select(n => n.GUID).ToList());
            DataTable createResult = null;
            DataTable retrieveResult = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
                retrieveResult = Query(retrieveQuery, true);
            }

            return EdgesFromDataTable(retrieveResult);
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> ReadEdges(
            Guid graphGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            ValidateGraphExists(graphGuid);
            int skip = 0;

            while (true)
            {
                DataTable result = Query(SelectEdgesQuery(graphGuid, edgeFilter, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    yield return EdgeFromDataRow(result.Rows[i]);
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override Edge ReadEdge(Guid graphGuid, Guid edgeGuid)
        {
            ValidateGraphExists(graphGuid);
            DataTable result = Query(SelectEdgeQuery(graphGuid, edgeGuid));
            if (result != null && result.Rows.Count == 1) return EdgeFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override Edge UpdateEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            ValidateGraphExists(edge.GraphGUID);
            return EdgeFromDataRow(Query(UpdateEdgeQuery(edge), true).Rows[0]);
        }

        /// <inheritdoc />
        public override void DeleteEdge(Guid graphGuid, Guid edgeGuid)
        {
            ValidateGraphExists(graphGuid);
            Query(DeleteEdgeQuery(graphGuid, edgeGuid), true);
        }

        /// <inheritdoc />
        public override void DeleteEdges(Guid graphGuid)
        {
            ValidateGraphExists(graphGuid);
            Query(DeleteEdgesQuery(graphGuid), true);
        }

        /// <inheritdoc />
        public override void DeleteEdges(Guid graphGuid, List<Guid> edgeGuids)
        {
            if (edgeGuids == null || edgeGuids.Count < 1) return;
            ValidateGraphExists(graphGuid);
            Query(DeleteEdgesQuery(graphGuid, edgeGuids), true);
        }

        /// <inheritdoc />
        public override bool ExistsEdge(Guid graphGuid, Guid edgeGuid)
        {
            if (ReadEdge(graphGuid, edgeGuid) != null) return true;
            return false;
        }

        #endregion

        #endregion

        #region Private-Methods

        #region General

        private string Sanitize(string val)
        {
            if (String.IsNullOrEmpty(val)) return val;

            string ret = "";

            //
            // null, below ASCII range, above ASCII range
            //
            for (int i = 0; i < val.Length; i++)
            {
                if (((int)(val[i]) == 10) ||      // Preserve carriage return
                    ((int)(val[i]) == 13))        // and line feed
                {
                    ret += val[i];
                }
                else if ((int)(val[i]) < 32)
                {
                    continue;
                }
                else
                {
                    ret += val[i];
                }
            }

            //
            // double dash
            //
            int doubleDash = 0;
            while (true)
            {
                doubleDash = ret.IndexOf("--");
                if (doubleDash < 0)
                {
                    break;
                }
                else
                {
                    ret = ret.Remove(doubleDash, 2);
                }
            }

            //
            // open comment
            // 
            int openComment = 0;
            while (true)
            {
                openComment = ret.IndexOf("/*");
                if (openComment < 0) break;
                else
                {
                    ret = ret.Remove(openComment, 2);
                }
            }

            //
            // close comment
            //
            int closeComment = 0;
            while (true)
            {
                closeComment = ret.IndexOf("*/");
                if (closeComment < 0) break;
                else
                {
                    ret = ret.Remove(closeComment, 2);
                }
            }

            //
            // in-string replacement
            //
            ret = ret.Replace("'", "''");
            return ret;
        }

        private DataTable Query(string query, bool isTransaction = false)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(query);
            if (query.Length > MaxStatementLength) throw new ArgumentException("Query exceeds maximum statement length of " + MaxStatementLength + " characters.");

            DataTable result = new DataTable();

            if (isTransaction)
            {
                query = query.Trim();
                query = "BEGIN TRANSACTION; " + query + " END TRANSACTION;";
            }

            if (Logging.LogQueries) Logging.Log(SeverityEnum.Debug, "query: " + query);

            lock (_QueryLock)
            {
                using (SqliteConnection conn = new SqliteConnection(_ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        
                        using (SqliteCommand cmd = new SqliteCommand(query, conn))
                        {
                            using (SqliteDataReader rdr = cmd.ExecuteReader())
                            {
                                result.Load(rdr);
                            }
                        }

                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        if (isTransaction)
                        {
                            using (SqliteCommand cmd = new SqliteCommand("ROLLBACK;", conn))
                                cmd.ExecuteNonQuery();
                        }

                        e.Data.Add("IsTransaction", isTransaction);
                        e.Data.Add("Query", query);
                        throw;
                    }
                }
            }

            if (Logging.LogResults) Logging.Log(SeverityEnum.Debug, "result: " + query + ": " + (result != null ? result.Rows.Count + " rows" : "(null)"));
            return result;
        }

        private void CreateTablesAndIndices()
        {
            List<string> queries = new List<string>();

            #region Graphs

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'graphs' ("
                + "id VARCHAR(64) NOT NULL UNIQUE, "
                + "name VARCHAR(128), "
                + "data TEXT, "
                + "'createdutc' VARCHAR(64)"
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_id' ON 'graphs' (id ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_name' ON 'graphs' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_createdutc' ON 'graphs' ('createdutc' ASC);");

            if (IndexData)
                queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_data' ON 'graphs' ('data' ASC);");

            #endregion

            #region Nodes

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'nodes' ("
                + "id VARCHAR(64) NOT NULL UNIQUE, "
                + "graphid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "data TEXT, "
                + "'createdutc' VARCHAR(64)"
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_id' ON 'nodes' (id ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_graphid' ON 'nodes' (graphid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_name' ON 'nodes' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_createdutc' ON 'nodes' ('createdutc' ASC);");

            if (IndexData)
                queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_data' ON 'nodes' (data ASC);");

            #endregion

            #region Edges

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'edges' ("
                + "id VARCHAR(64) NOT NULL UNIQUE, "
                + "graphid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "fromguid VARCHAR(64) NOT NULL, "
                + "toguid VARCHAR(64) NOT NULL, "
                + "cost INT NOT NULL, "
                + "data TEXT, "
                + "'createdutc' VARCHAR(64)"
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_id' ON 'edges' (id ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_graphid' ON 'edges' (graphid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_name' ON 'edges' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_fromguid' ON 'edges' (fromguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_toguid' ON 'edges' (toguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_createdutc' ON 'edges' ('createdutc' ASC);");

            if (IndexData)
                queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_data' ON 'edges' (data ASC);");

            #endregion

            #region Run-Queries

            foreach (string query in queries)
                Query(query);

            #endregion
        }

        private string GetDataRowStringValue(DataRow row, string column)
        {
            if (row[column] != null
                && row[column] != DBNull.Value)
            {
                return row[column].ToString();
            }
            return null;
        }

        private object GetDataRowJsonValue(DataRow row, string column)
        {
            if (row[column] != null
                && row[column] != DBNull.Value)
            {
                return Serializer.DeserializeJson<object>(row[column].ToString());
            }
            return null;
        }

        private bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        private List<object> ObjectToList(object obj)
        {
            if (obj == null) return null;
            List<object> ret = new List<object>();
            var enumerator = ((IEnumerable)obj).GetEnumerator();
            while (enumerator.MoveNext())
            {
                ret.Add(enumerator.Current);
            }
            return ret;
        }

        private string EnumerationOrderToClause(EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            switch (order)
            {
                case EnumerationOrderEnum.CostAscending:
                    return "cost ASC";
                case EnumerationOrderEnum.CostDescending:
                    return "cost DESC";
                case EnumerationOrderEnum.CreatedAscending:
                    return "createdutc ASC";
                case EnumerationOrderEnum.CreatedDescending:
                    return "createdutc DESC";
                case EnumerationOrderEnum.GuidAscending:
                    return "id ASC";
                case EnumerationOrderEnum.GuidDescending:
                    return "id DESC";
                case EnumerationOrderEnum.NameAscending:
                    return "name ASC";
                case EnumerationOrderEnum.NameDescending:
                    return "name DESC";
                default:
                    throw new ArgumentException("Unknown enumeration order '" + order.ToString() + "'.");
            }
        }

        private string ExpressionToWhereClause(Expr expr)
        {
            if (expr == null) return null;
            if (expr.Left == null) return null;

            string clause = "";

            if (expr.Left is Expr)
            {
                clause += ExpressionToWhereClause((Expr)expr.Left) + " ";
            }
            else
            {
                if (!(expr.Left is string))
                {
                    throw new ArgumentException("Left term must be of type Expression or String");
                }

                clause += "json_extract(data, '$." + Sanitize(expr.Left.ToString()) + "') ";
            }

            switch (expr.Operator)
            {
                #region Process-By-Operators

                case OperatorEnum.And:
                    #region And

                    if (expr.Right == null) return null;
                    clause += "AND ";

                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.Or:
                    #region Or

                    if (expr.Right == null) return null;
                    clause += "OR ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.Equals:
                    #region Equals

                    if (expr.Right == null) return null;
                    clause += "= ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.NotEquals:
                    #region NotEquals

                    if (expr.Right == null) return null;
                    clause += "<> ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize (expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.In:
                    #region In

                    if (expr.Right == null) return null;
                    int inAdded = 0;
                    if (!IsList(expr.Right)) return null;
                    List<object> inTempList = ObjectToList(expr.Right);
                    clause += " IN (";
                    foreach (object currObj in inTempList)
                    {
                        if (currObj == null) continue;
                        if (inAdded > 0) clause += ",";
                        if (currObj is DateTime || currObj is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(currObj).ToString(TimestampFormat) + "'";
                        }
                        else if (currObj is int || currObj is long || currObj is decimal)
                        {
                            clause += currObj.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(currObj.ToString()) + "'";
                        }
                        inAdded++;
                    }
                    clause += ")";
                    break;

                #endregion

                case OperatorEnum.NotIn:
                    #region NotIn

                    if (expr.Right == null) return null;
                    int notInAdded = 0;
                    if (!IsList(expr.Right)) return null;
                    List<object> notInTempList = ObjectToList(expr.Right);
                    clause += " NOT IN (";
                    foreach (object currObj in notInTempList)
                    {
                        if (currObj == null) continue;
                        if (notInAdded > 0) clause += ",";
                        if (currObj is DateTime || currObj is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(currObj).ToString(TimestampFormat) + "'";
                        }
                        else if (currObj is int || currObj is long || currObj is decimal)
                        {
                            clause += currObj.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(currObj.ToString()) + "'";
                        }
                        notInAdded++;
                    }
                    clause += ")";
                    break;

                #endregion

                case OperatorEnum.Contains:
                    #region Contains

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause +=
                            "(" +
                            "'$." + Sanitize(expr.Left.ToString()) + "' LIKE ('%" + Sanitize(expr.Right.ToString()) + "') " +
                            "OR '$." + Sanitize(expr.Left.ToString()) + "' LIKE ('%" + Sanitize(expr.Right.ToString()) + "%') " +
                            "OR '$." + Sanitize(expr.Left.ToString()) + "' LIKE ('" + Sanitize(expr.Right.ToString()) + "%')" +
                            ")";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.ContainsNot:
                    #region ContainsNot

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause +=
                            "(" +
                            "'$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '%" + Sanitize(expr.Right.ToString()) + "' " +
                            "AND '$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '%" + Sanitize(expr.Right.ToString()) + "%' " +
                            "AND '$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '" + Sanitize(expr.Right.ToString()) + "%'" +
                            ")";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.StartsWith:
                    #region StartsWith

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + "' LIKE '" + Sanitize(expr.Right.ToString()) + "%')";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.StartsWithNot:
                    #region StartsWithNot

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '" + Sanitize(expr.Right.ToString()) + "%'";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.EndsWith:
                    #region EndsWith

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + " LIKE '%" + Sanitize(expr.Right.ToString()) + "')";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.EndsWithNot:
                    #region EndsWith

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + " NOT LIKE '%" + Sanitize(expr.Right.ToString()) + "')";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.GreaterThan:
                    #region GreaterThan

                    if (expr.Right == null) return null;
                    clause += "> ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'$." + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.GreaterThanOrEqualTo:
                    #region GreaterThanOrEqualTo

                    if (expr.Right == null) return null;
                    clause += ">= ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.LessThan:
                    #region LessThan

                    if (expr.Right == null) return null;
                    clause += "< ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.LessThanOrEqualTo:
                    #region LessThanOrEqualTo

                    if (expr.Right == null) return null;
                    clause += "<= ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause((Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.IsNull:
                    #region IsNull

                    clause += " IS NULL";
                    break;

                #endregion

                case OperatorEnum.IsNotNull:
                    #region IsNotNull

                    clause += " IS NOT NULL";
                    break;

                    #endregion

                    #endregion
            }

            clause += " ";

            return clause;
        }

        private void ValidateGraphExists(Guid graphGuid)
        {
            if (!ExistsGraph(graphGuid))
                throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");
        }

        #endregion

        #region Graphs

        private string InsertGraphQuery(Graph graph)
        {
            string ret =
                "INSERT INTO 'graphs' "
                + "VALUES ("
                + "'" + graph.GUID.ToString() + "',"
                + "'" + Sanitize(graph.Name) + "',";

            if (graph.Data == null) ret += "null,";
            else ret += "'" + Sanitize(Serializer.SerializeJson(graph.Data, false)) + "',";

            ret += 
                "'" + Sanitize(graph.CreatedUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string SelectGraphQuery(string name)
        {
            return "SELECT * FROM 'graphs' WHERE name = '" + Sanitize(name) + "';";
        }

        private string SelectGraphQuery(Guid guid)
        {
            return "SELECT * FROM 'graphs' WHERE id = '" + guid.ToString() + "';";
        }

        private string SelectGraphsQuery(
            Expr graphFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'graphs' WHERE "
                + "id IS NOT NULL ";

            if (graphFilter != null) ret += "AND " + ExpressionToWhereClause(graphFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateGraphQuery(Graph graph)
        {
            return
                "UPDATE 'graphs' "
                + "SET name = '" + Sanitize(graph.Name) + "' "
                + "WHERE id = '" + graph.GUID + "' "
                + "RETURNING *;";
        }

        private string DeleteGraphQuery(string name)
        {
            return "DELETE FROM 'graphs' WHERE name = '" + Sanitize(name) + "';";
        }

        private string DeleteGraphQuery(Guid guid)
        {
            return "DELETE FROM 'graphs' WHERE id = '" + guid.ToString() + "';";
        }

        private string DeleteGraphEdgesQuery(Guid guid)
        {
            return "DELETE FROM 'edges' WHERE graphid = '" + guid.ToString() + "';";
        }

        private string DeleteGraphNodesQuery(Guid guid)
        {
            return "DELETE FROM 'nodes' WHERE graphid = '" + guid.ToString() + "';";
        }

        private Graph GraphFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new Graph
            {
                GUID = Guid.Parse(row["id"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                Data = GetDataRowJsonValue(row, "data"),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString())
            };
        }

        #endregion

        #region Nodes

        private string InsertNodeQuery(Node node)
        {
            string ret =
                "INSERT INTO 'nodes' "
                + "VALUES ("
                + "'" + node.GUID.ToString() + "',"
                + "'" + node.GraphGUID.ToString() + "',"
                + "'" + Sanitize(node.Name) + "',";

            if (node.Data == null) ret += "null,";
            else ret += "'" + Sanitize(Serializer.SerializeJson(node.Data, false)) + "',";

            ret +=
                "'" + Sanitize(node.CreatedUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string InsertMultipleNodesQuery(List<Node> nodes)
        {
            string ret =
                "INSERT INTO 'nodes' "
                + "VALUES ";

            for (int i = 0; i < nodes.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "(";
                ret += "'" + nodes[i].GUID.ToString() + "',"
                    + "'" + nodes[i].GraphGUID.ToString() + "',"
                    + "'" + Sanitize(nodes[i].Name) + "',";

                if (nodes[i].Data == null) ret += "null,";
                else ret += "'" + Sanitize(Serializer.SerializeJson(nodes[i].Data, false)) + "',";
                ret += "'" + Sanitize(nodes[i].CreatedUtc.ToString(TimestampFormat)) + "'";
                ret += ")";
            }

            ret += ";";
            return ret;
        }

        private string SelectMultipleNodesQuery(List<Guid> guids)
        {
            string ret = "SELECT * FROM 'nodes' WHERE id IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private string SelectNodeQuery(Guid graphGuid, Guid nodeGuid)
        {
            return "SELECT * FROM 'nodes' WHERE "
                + "id = '" + nodeGuid.ToString() + "' "
                + "AND graphid = '" + graphGuid.ToString() + "';";
        }

        private string SelectNodesQuery(
            Guid graphGuid,
            Expr nodeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'nodes' WHERE "
                + "id IS NOT NULL "
                + "AND graphid = '" + graphGuid.ToString() + "' ";

            if (nodeFilter != null) ret += "AND " + ExpressionToWhereClause(nodeFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateNodeQuery(Node node)
        {
            string ret =
                "UPDATE 'nodes' SET "
                + "name = '" + Sanitize(node.Name) + "',";

            if (node.Data == null) ret += "data = null ";
            else ret += "data = '" + Sanitize(Serializer.SerializeJson(node.Data, false)) + "' ";

            ret +=
                "WHERE id = '" + node.GUID + "' "
                + "RETURNING *;";

            return ret;
        }

        private string DeleteNodeQuery(Guid graphGuid, Guid nodeGuid)
        {
            return
                "DELETE FROM 'nodes' WHERE "
                + "id = '" + nodeGuid.ToString() + "' "
                + "AND graphid = '" + graphGuid.ToString() + "';";
        }

        private string DeleteNodesQuery(Guid graphGuid)
        {
            return
                "DELETE FROM 'nodes' WHERE graphid = '" + graphGuid.ToString() + "';";
        }

        private string DeleteNodesQuery(Guid graphGuid, List<Guid> nodeGuids)
        {
            string ret =
                "DELETE FROM 'nodes' WHERE graphid = '" + graphGuid.ToString() + "' "
                + "AND id IN (";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(nodeGuids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private string DeleteNodeEdgesQuery(Guid graphGuid, List<Guid> nodeGuids)
        {
            string ret =
                "DELETE FROM 'edges' WHERE graphid = '" + graphGuid.ToString() + "' "
                + "AND (";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) ret += "OR ";
                ret += "(fromguid = '" + Sanitize(nodeGuids[i].ToString()) + "' OR toguid = '" + Sanitize(nodeGuids[i].ToString()) + "')";
            }

            ret += ")";
            return ret;
        }

        private Node NodeFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new Node
            {
                GUID = Guid.Parse(row["id"].ToString()),
                GraphGUID = Guid.Parse(row["graphid"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                Data = GetDataRowJsonValue(row, "data"),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString())
            };
        }

        private List<Node> NodesFromDataTable(DataTable table)
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;

            List<Node> ret = new List<Node>();

            foreach (DataRow row in table.Rows)
                ret.Add(NodeFromDataRow(row));

            return ret;
        }

        private string BatchExistsNodesQuery(Guid graphGuid, List<Guid> nodeGuids)
        {
            string query = "WITH temp(id) AS (VALUES ";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) query += ",";
                query += "('" + nodeGuids[i].ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.id, CASE WHEN nodes.id IS NOT NULL THEN 1 ELSE 0 END as \"exists\" "
                + "FROM temp "
                + "LEFT JOIN nodes ON temp.id = nodes.id AND nodes.graphid = '" + graphGuid.ToString() + "';";
            return query;
        }

        private string BatchExistsEdgesQuery(Guid graphGuid, List<Guid> edgeGuids)
        {
            string query = "WITH temp(id) AS (VALUES ";

            for (int i = 0; i < edgeGuids.Count; i++)
            {
                if (i > 0) query += ",";
                query += "('" + edgeGuids[i].ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.id, CASE WHEN edges.id IS NOT NULL THEN 1 ELSE 0 END as \"exists\" "
                + "FROM temp "
                + "LEFT JOIN edges ON temp.id = edges.id AND edges.graphid = '" + graphGuid.ToString() + "';";
            return query;
        }

        private string BatchExistsEdgesBetweenQuery(Guid graphGuid, List<EdgeBetween> edgesBetween)
        {
            string query = "WITH temp(fromguid, toguid) AS (VALUES ";

            for (int i = 0; i < edgesBetween.Count; i++)
            {
                EdgeBetween curr = edgesBetween[i];
                if (i > 0) query += ",";
                query += "('" + curr.From.ToString() + "','" + curr.To.ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.fromguid, temp.toguid, CASE WHEN edges.fromguid IS NOT NULL THEN 1 ELSE 0 END AS \"exists\" "
                + "FROM temp "
                + "LEFT JOIN edges ON temp.fromguid = edges.fromguid AND temp.toguid = edges.toguid AND edges.graphid = '" + graphGuid.ToString() + "';";

            return query;
        }

        #endregion

        #region Edges

        private string InsertEdgeQuery(Edge edge)
        {
            string ret =
                "INSERT INTO 'edges' "
                + "VALUES ("
                + "'" + edge.GUID.ToString() + "',"
                + "'" + edge.GraphGUID.ToString() + "',"
                + "'" + Sanitize(edge.Name) + "',"
                + "'" + edge.From.ToString() + "',"
                + "'" + edge.To.ToString() + "',"
                + "'" + edge.Cost.ToString() + "',";

            if (edge.Data == null) ret += "null,";
            else ret += "'" + Sanitize(Serializer.SerializeJson(edge.Data, false)) + "',";

            ret +=
                "'" + Sanitize(edge.CreatedUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string InsertMultipleEdgesQuery(List<Edge> edges)
        {
            string ret =
                "INSERT INTO 'edges' "
                + "VALUES ";

            for (int i = 0; i < edges.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "("
                    + "'" + edges[i].GUID.ToString() + "',"
                    + "'" + edges[i].GraphGUID.ToString() + "',"
                    + "'" + Sanitize(edges[i].Name) + "',"
                    + "'" + edges[i].From.ToString() + "',"
                    + "'" + edges[i].To.ToString() + "',"
                    + "'" + edges[i].Cost.ToString() + "',";

                if (edges[i].Data == null) ret += "null,";
                else ret += "'" + Sanitize(Serializer.SerializeJson(edges[i].Data, false)) + "',";

                ret += "'" + Sanitize(edges[i].CreatedUtc.ToString(TimestampFormat)) + "'";
                ret += ")";
            }

            ret += ";";
            return ret;
        }

        private string SelectMultipleEdgesQuery(List<Guid> guids)
        {
            string ret = "SELECT * FROM 'edges' WHERE id IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private string SelectEdgeQuery(Guid graphGuid, Guid edgeGuid)
        {
            return 
                "SELECT * FROM 'edges' WHERE "
                + "graphid = '" + graphGuid.ToString() + "' "
                + "AND id = '" + edgeGuid.ToString() + "';";
        }

        private string SelectEdgesQuery(
            Guid graphGuid,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' WHERE "
                + "graphid = '" + graphGuid.ToString() + "' ";

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause(edgeFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectConnectedEdgesQuery(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'edges' WHERE "
                + "graphid = '" + graphGuid.ToString() + "' AND "
                + "("
                + "fromguid = '" + nodeGuid.ToString() + "' "
                + "OR toguid = '" + nodeGuid.ToString() + "'"
                + ") ";

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause(edgeFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectEdgesFromQuery(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' WHERE "
                + "graphid = '" + graphGuid.ToString() + "' "
                + "AND fromguid = '" + nodeGuid.ToString() + "' ";

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause(edgeFilter) + " ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectEdgesToQuery(
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' WHERE "
                + "graphid = '" + graphGuid.ToString() + "' "
                + "AND toguid = '" + nodeGuid.ToString() + "' ";

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause(edgeFilter) + " ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectEdgesBetweenQuery(
            Guid graphGuid,
            Guid from,
            Guid to,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' WHERE "
                + "graphid = '" + graphGuid.ToString() + "' "
                + "AND fromguid = '" + from.ToString() + "' "
                + "AND toguid = '" + to.ToString() + "' ";

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause(edgeFilter) + " ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateEdgeQuery(Edge edge)
        {
            string ret =
                "UPDATE 'edges' SET "
                + "name = '" + Sanitize(edge.Name) + "',"
                + "cost = '" + edge.Cost.ToString() + "',";

            if (edge.Data == null) ret += "data = null ";
            else ret += "data = '" + Sanitize(Serializer.SerializeJson(edge.Data, false)) + "' ";

            ret +=
                "WHERE id = '" + edge.GUID + "' "
                + "RETURNING *;";

            return ret;
        }

        private string DeleteEdgeQuery(Guid graphGuid, Guid edgeGuid)
        {
            return
                "DELETE FROM 'edges' WHERE "
                + "graphid = '" + graphGuid.ToString() + "' "
                + "AND id = '" + edgeGuid.ToString() + "';";
        }

        private string DeleteEdgesQuery(Guid graphGuid)
        {
            return
                "DELETE FROM 'edges' WHERE graphid = '" + graphGuid.ToString() + "';";
        }

        private string DeleteEdgesQuery(Guid graphGuid, List<Guid> edgeGuids)
        {
            string ret =
                "DELETE FROM 'edges' WHERE graphid = '" + graphGuid.ToString() + "' "
                + "AND id IN (";

            for (int i = 0; i < edgeGuids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(edgeGuids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private List<Edge> EdgesFromDataTable(DataTable table)
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;

            List<Edge> ret = new List<Edge>();

            foreach (DataRow row in table.Rows)
                ret.Add(EdgeFromDataRow(row));

            return ret;
        }

        private Edge EdgeFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new Edge
            {
                GUID = Guid.Parse(row["id"].ToString()),
                GraphGUID = Guid.Parse(row["graphid"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                From = Guid.Parse(row["fromguid"].ToString()),
                To = Guid.Parse(row["toguid"].ToString()),
                Data = GetDataRowJsonValue(row, "data"),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString())
            };
        }

        #endregion

        #region Traversal-and-Routing

        private IEnumerable<RouteDetail> GetRoutesDfs(
            Graph graph,
            Node start,
            Node end,
            Expr edgeFilter,
            Expr nodeFilter,
            List<Node> visitedNodes,
            List<Edge> visitedEdges)
        {
            #region Get-Edges

            List<Edge> edges = GetEdgesFrom(graph.GUID, start.GUID, edgeFilter, EnumerationOrderEnum.CreatedDescending).ToList();

            #endregion

            #region Process-Each-Edge

            for (int i = 0; i < edges.Count; i++)
            {
                Edge nextEdge = edges[i];

                #region Retrieve-Next-Node

                Node nextNode = ReadNode(graph.GUID, nextEdge.To);
                if (nextNode == null)
                {
                    Logging.Log(SeverityEnum.Warn, "node " + nextEdge.To + " referenced in graph " + graph.GUID + " but does not exist");
                    continue;
                }

                #endregion

                #region Check-for-End

                if (nextNode.GUID.Equals(end.GUID))
                {
                    RouteDetail routeDetail = new RouteDetail();
                    routeDetail.Edges = new List<Edge>(visitedEdges);
                    routeDetail.Edges.Add(nextEdge);
                    yield return routeDetail;
                    continue;
                }

                #endregion

                #region Check-for-Cycles

                if (visitedNodes.Any(n => n.GUID.Equals(nextEdge.To)))
                {
                    continue; // cycle
                }

                #endregion

                #region Recursion-and-Variables

                List<Node> childVisitedNodes = new List<Node>(visitedNodes);
                List<Edge> childVisitedEdges = new List<Edge>(visitedEdges);

                childVisitedNodes.Add(nextNode);
                childVisitedEdges.Add(nextEdge);

                IEnumerable<RouteDetail> recurse = GetRoutesDfs(graph, nextNode, end, edgeFilter, nodeFilter, childVisitedNodes, childVisitedEdges);
                foreach (RouteDetail route in recurse)
                {
                    if (route != null) yield return route;
                }

                #endregion
            }

            #endregion
        }

        #endregion

        #endregion

#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
    }
}
