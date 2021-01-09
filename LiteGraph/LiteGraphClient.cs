using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// LiteGraph client.
    /// </summary>
    public class LiteGraphClient : IDisposable
    {
        #region Public-Members
         
        /// <summary>
        /// Logger settings.
        /// </summary>
        public LoggerSettings Logger
        {
            get
            {
                return _Logging;
            }
            set
            {
                if (value == null) _Logging = new LoggerSettings();
                else _Logging = value;
            }
        }

        /// <summary>
        /// Maximum supported statement length.
        /// </summary>
        public int MaxStatementLength
        {
            get
            {
                // https://www.sqlite.org/limits.html
                return 1000000;
            }
        }

        /// <summary>
        /// GUID property that must be present in every node added to the graph.
        /// </summary>
        public string NodeGuidProperty
        {
            get
            {
                return _NodeGuidProperty;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                _NodeGuidProperty = value;
            }
        }

        /// <summary>
        /// GUID property that must be present in every edge added to the graph.
        /// </summary>
        public string EdgeGuidProperty
        {
            get
            {
                return _EdgeGuidProperty;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                _EdgeGuidProperty = value;
            }
        }

        /// <summary>
        /// JSON.NET formatting mode.
        /// </summary>
        public Formatting JsonFormatting { get; set; } = Formatting.None;

        #endregion

        #region Private-Members

        private bool _Disposed = false;
        private readonly object _Lock = new object();
        private LoggerSettings _Logging = new LoggerSettings();
        private string _Filename = null;
        private string _ConnectionString = null;
        private string _NodeGuidProperty = "guid";
        private string _EdgeGuidProperty = "guid";
        private int _MaxResultsLimit = 100;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// LiteGraph client.
        /// </summary>
        public LiteGraphClient(string filename, LoggerSettings logging = null)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            _Filename = filename;
            _ConnectionString = "Data Source=" + filename;

            if (logging != null) _Logging = logging;

            InitializeTables();
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

        #region Node

        /// <summary>
        /// Add a node.
        /// The supplied JSON must contain the globally-unique identifier property as specified in GuidProperty.
        /// </summary>
        /// <param name="json">JSON object, which must include the globally-unique identifier property as specified in NodeGuidProperty.</param>
        /// <returns>Graph result.</returns>
        public GraphResult AddNode(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            GraphResult r = new GraphResult(GraphOperation.AddNode);

            JObject j = JObject.Parse(json);
            if (!j.ContainsKey(_NodeGuidProperty)) throw new ArgumentException("Supplied JSON does not contain the globally-unique identifier property '" + _NodeGuidProperty + "'.  To use a different property name, modify the 'NodeGuidProperty' field.");
            string guid = j[_NodeGuidProperty].ToString();
            if (String.IsNullOrEmpty(guid)) throw new ArgumentException("Supplied unique identifier in property '" + _NodeGuidProperty + "' is null or empty.");
            
            GraphResult existsResult = NodeExists(guid);
            if ((bool)existsResult.Result) throw new ArgumentException("Node with unique identifier '" + guid + "' already exists.");

            Node n = new Node(0, guid, DateTime.Now.ToUniversalTime(), j);
            Query(DatabaseHelper.Nodes.InsertQuery(n));
             
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Update a node.
        /// The supplied JSON must contain the globally-unique identifier property as specified in GuidProperty.
        /// </summary>
        /// <param name="json">JSON object, which must include the globally-unique identifier property as specified in NodeGuidProperty.</param>
        /// <returns>Graph result.</returns>
        public GraphResult UpdateNode(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            GraphResult r = new GraphResult(GraphOperation.UpdateNode);

            JObject j = JObject.Parse(json);
            if (!j.ContainsKey(_NodeGuidProperty)) throw new ArgumentException("Supplied JSON does not contain the globally-unique identifier property '" + _NodeGuidProperty + "'.  To use a different property name, modify the 'NodeGuidProperty' field.");
            string guid = j[_NodeGuidProperty].ToString();
            if (String.IsNullOrEmpty(guid)) throw new ArgumentException("Supplied unique identifier in property '" + _NodeGuidProperty + "' is null or empty.");

            DataTable result = Query(DatabaseHelper.Nodes.SelectByGuid(guid));
            if (result == null || result.Rows.Count < 1) throw new ArgumentException("Node with globally-unique identifier '" + guid + "' not found.");
            Node n = DatabaseHelper.Nodes.FromDataRow(result.Rows[0]);
            n.Properties = JObject.Parse(json);
            Query(DatabaseHelper.Nodes.UpdateQuery(n));
             
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Remove a node and its edges by the node's globally-unique identifier.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <returns>Graph result.</returns>
        public GraphResult RemoveNode(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.RemoveNode);

            Query(DatabaseHelper.Nodes.DeleteQuery(guid));
            Query(DatabaseHelper.Edges.DeleteNodeEdgesQuery(guid));
             
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Check if a node exists by its globally-unique identifier.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <returns>Graph result.</returns>
        public GraphResult NodeExists(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.NodeExists);

            DataTable result = Query(DatabaseHelper.Nodes.ExistsQuery(guid));
            if (result != null && result.Rows.Count > 0)
            {
                r.Result = true;
            }
            else
            {
                r.Result = false;
            }
             
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Retrieve all nodes.
        /// </summary>
        /// <returns>Graph result where 'Data' contains a JArray.</returns>
        public GraphResult GetAllNodes()
        {
            GraphResult r = new GraphResult(GraphOperation.GetAllNodes);

            DataTable result = Query(DatabaseHelper.Nodes.SelectAllQuery);
            if (result != null && result.Rows.Count > 0)
            {
                List<Node> nodes = DatabaseHelper.Nodes.FromDataTable(result);
                if (nodes != null && nodes.Count > 0)
                {
                    r.Data = JArray.FromObject(nodes);
                }
                else
                {
                    r.Data = JArray.FromObject(new List<object>());
                }
            }
            else
            {
                r.Data = JArray.FromObject(new List<object>());
            }
             
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Retrieve a node.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <returns>Graph result where 'Data' contains a JObject.</returns>
        public GraphResult GetNode(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.GetNode);

            DataTable result = Query(DatabaseHelper.Nodes.SelectByGuid(guid));
            if (result == null || result.Rows.Count < 1)
            {
                r.Data = JObject.FromObject(new object());
            }
            else
            {
                Node n = DatabaseHelper.Nodes.FromDataRow(result.Rows[0]);
                r.Data = JObject.FromObject(n);
            }
             
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Retrieve a node's neighbors.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="indexStart">Starting index.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <returns>Graph result where 'Data' contains a JArray.</returns>
        public GraphResult GetNeighbors(string guid, int indexStart = 0, int maxResults = 100)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.GetNeighbors);

            DataTable edgeResult = Query(DatabaseHelper.Edges.SelectByFilter(new List<string> { guid }, null, indexStart, maxResults));
            if (edgeResult != null && edgeResult.Rows.Count > 0)
            {
                List<Edge> edges = DatabaseHelper.Edges.FromDataTable(edgeResult);
                List<string> guids = new List<string>();
                foreach (Edge edge in edges)
                {
                    guids.Add(edge.FromGUID);
                    guids.Add(edge.ToGUID);
                }
                guids = guids.Distinct().ToList();
                while (guids.Contains(guid)) guids.Remove(guid);

                DataTable nodeResult = Query(DatabaseHelper.Nodes.SelectByFilter(guids, null, indexStart, maxResults));
                if (nodeResult != null && nodeResult.Rows.Count > 0)
                {
                    List<Node> nodes = DatabaseHelper.Nodes.FromDataTable(nodeResult);
                    r.Data = JArray.FromObject(nodes);
                }
                else
                {
                    r.Data = JArray.FromObject(new List<object>());
                }
            }
            else
            {
                r.Data = JArray.FromObject(new List<object>());
            }

            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Search nodes.
        /// </summary>
        /// <param name="guids">Subset of node GUIDs to search.</param>
        /// <param name="filters">Filters to apply against each node's JSON data.</param>
        /// <param name="indexStart">Starting index.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <returns>Graph result where 'Data' contains a JArray.</returns>
        public GraphResult SearchNodes(List<string> guids, List<SearchFilter> filters, int indexStart = 0, int maxResults = 100)
        {
            GraphResult r = new GraphResult(GraphOperation.SearchNodes);

            if (indexStart < 0) throw new ArgumentException("Index start must be zero or greater.");
            if (maxResults < 1) throw new ArgumentException("Max results must be greater than zero.");
            if (maxResults > _MaxResultsLimit) throw new ArgumentException("Max results must not exceed " + _MaxResultsLimit);
            DataTable result = Query(DatabaseHelper.Nodes.SelectByFilter(guids, filters, indexStart, maxResults));
            if (result != null && result.Rows.Count > 0)
            {
                List<Node> nodes = DatabaseHelper.Nodes.FromDataTable(result);
                r.Data = JArray.FromObject(nodes);
            }
            else
            {
                r.Data = JArray.FromObject(new List<object>());
            }

            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Retrieve a tree from the supplied node.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="maxDepth">The maximum depth to search.</param>
        /// <returns>Graph result.</returns>
        public GraphResult GetDescendants(string guid, int maxDepth = 5)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.GetDescendants);

            DataTable result = Query(DatabaseHelper.Nodes.SelectByGuid(guid));
            if (result != null && result.Rows.Count > 0)
            {
                Node n = DatabaseHelper.Nodes.FromDataRow(result.Rows[0]);
                n.Descendents = GetDescendantsFromNode(n, n.GUID, 0, maxDepth);
                r.Data = JObject.FromObject(n);
            }

            r.Time.End = DateTime.Now;
            return r;
        }

        #endregion

        #region Edge

        /// <summary>
        /// Add an edge from one node to another.
        /// </summary>
        /// <param name="fromGuid">Globally-unique identifier of the source node.</param>
        /// <param name="toGuid">Globally-unique identifier of the target node.</param>
        /// <param name="json">JSON object, which must include the globally-unique identifier property as specified in EdgeGuidProperty.</param>
        /// <returns>Graph result.</returns>
        public GraphResult AddEdge(string fromGuid, string toGuid, string json)
        {
            GraphResult r = new GraphResult(GraphOperation.AddEdge);

            if (String.IsNullOrEmpty(fromGuid)) throw new ArgumentNullException(nameof(fromGuid));
            if (String.IsNullOrEmpty(toGuid)) throw new ArgumentNullException(nameof(toGuid));
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            JObject j = JObject.Parse(json);
            if (!j.ContainsKey(_EdgeGuidProperty)) throw new ArgumentException("Supplied JSON does not contain the globally-unique identifier property '" + _EdgeGuidProperty + "'.  To use a different property name, modify the 'EdgeGuidProperty' field.");
            string guid = j[_NodeGuidProperty].ToString();
            if (String.IsNullOrEmpty(guid)) throw new ArgumentException("Supplied unique identifier in property '" + _EdgeGuidProperty + "' is null or empty.");

            GraphResult existsResult = EdgeExists(guid);
            if ((bool)existsResult.Result) throw new ArgumentException("Edge with unique identifier '" + guid + "' already exists.");

            existsResult = NodeExists(fromGuid);
            if (!(bool)existsResult.Result) throw new ArgumentException("Node with unique identifier '" + fromGuid + "' not found.");

            existsResult = NodeExists(toGuid);
            if (!(bool)existsResult.Result) throw new ArgumentException("Node with unique identifier '" + toGuid + "' not found.");

            Edge e = new Edge(0, guid, fromGuid, toGuid, DateTime.Now.ToUniversalTime(), json);
            Query(DatabaseHelper.Edges.InsertQuery(e));

            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Update an edge's properties.
        /// </summary>
        /// <param name="json">JSON object, which must include the globally-unique identifier property as specified in EdgeGuidProperty.</param>
        /// <returns>Graph result.</returns>
        public GraphResult UpdateEdge(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            GraphResult r = new GraphResult(GraphOperation.UpdateEdge);

            JObject j = JObject.Parse(json);
            if (!j.ContainsKey(_EdgeGuidProperty)) throw new ArgumentException("Supplied JSON does not contain the globally-unique identifier property '" + _EdgeGuidProperty + "'.  To use a different property name, modify the 'EdgeGuidProperty' field.");
            string guid = j[_EdgeGuidProperty].ToString();
            if (String.IsNullOrEmpty(guid)) throw new ArgumentException("Supplied unique identifier in property '" + _EdgeGuidProperty + "' is null or empty.");

            DataTable result = Query(DatabaseHelper.Edges.SelectByGuid(guid));
            if (result == null || result.Rows.Count < 1) throw new ArgumentException("Edge with globally-unique identifier '" + guid + "' not found.");

            Edge e = DatabaseHelper.Edges.FromDataRow(result.Rows[0]);
            e.Properties = JObject.Parse(json);
            Query(DatabaseHelper.Edges.UpdateQuery(e));

            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Remove an edge by its globally-unique identifier.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <returns>Graph result.</returns>
        public GraphResult RemoveEdge(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.RemoveEdge); 
            Query(DatabaseHelper.Edges.DeleteQuery(guid)); 
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Check if an edge exists by its globally-unique identifier.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <returns>Graph result.</returns>
        public GraphResult EdgeExists(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.EdgeExists);

            DataTable result = Query(DatabaseHelper.Edges.ExistsQuery(guid));
            if (result != null && result.Rows.Count > 0)
            {
                r.Result = true;
            }
            else
            {
                r.Result = false;
            }

            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Retrieve all edges.
        /// </summary>
        /// <returns>Graph result where 'Data' contains a JArray.</returns>
        public GraphResult GetAllEdges()
        {
            GraphResult r = new GraphResult(GraphOperation.GetAllEdges);

            DataTable result = Query(DatabaseHelper.Edges.SelectAllQuery);
            if (result != null && result.Rows.Count > 0)
            {
                List<Edge> edges = DatabaseHelper.Edges.FromDataTable(result);
                if (edges != null && edges.Count > 0)
                {
                    r.Data = JArray.FromObject(edges);
                }
                else
                {
                    r.Data = JArray.FromObject(new List<object>());
                }
            }
            else
            {
                r.Data = JArray.FromObject(new List<object>());
            }

            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Retrieve all edges to or from a given node.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="indexStart">Starting index.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <returns>Graph result where 'Data' contains a JArray.</returns>
        public GraphResult GetEdges(string guid, int indexStart = 0, int maxResults = 100)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.GetEdges);

            List<string> guids = new List<string>();
            guids.Add(guid);

            DataTable result = Query(DatabaseHelper.Edges.SelectByFilter(guids, null, indexStart, maxResults));
            if (result != null && result.Rows.Count > 0)
            {
                List<Edge> edges = DatabaseHelper.Edges.FromDataTable(result);
                r.Data = JArray.FromObject(edges);
            }
            else
            {
                r.Data = JArray.FromObject(new List<object>());
            }
             
            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Retrieve an edge.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <returns>Graph result where 'Data' contains a JObject.</returns>
        public GraphResult GetEdge(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult r = new GraphResult(GraphOperation.GetEdge);

            DataTable result = Query(DatabaseHelper.Edges.SelectByGuid(guid));
            if (result == null || result.Rows.Count < 1)
            {
                r.Data = JObject.FromObject(new object());
            }
            else
            {
                Edge e = DatabaseHelper.Edges.FromDataRow(result.Rows[0]);
                r.Data = JObject.FromObject(e);
            }

            r.Time.End = DateTime.Now;
            return r;
        }

        /// <summary>
        /// Search edges.
        /// </summary>
        /// <param name="guids">Subset of edge GUIDs, from node GUIDs, and to node GUIDs to search.</param>
        /// <param name="filters">Filters to apply against each edge's JSON data.</param>
        /// <param name="indexStart">Starting index.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <returns>Graph result where 'Data' contains a JArray.</returns>
        public GraphResult SearchEdges(List<string> guids, List<SearchFilter> filters, int indexStart = 0, int maxResults = 100)
        {
            if (indexStart < 0) throw new ArgumentException("Index start must be zero or greater.");
            if (maxResults < 1) throw new ArgumentException("Max results must be greater than zero.");
            if (maxResults > _MaxResultsLimit) throw new ArgumentException("Max results must not exceed " + _MaxResultsLimit);
            GraphResult r = new GraphResult(GraphOperation.SearchEdges);

            DataTable result = Query(DatabaseHelper.Edges.SelectByFilter(guids, filters, indexStart, maxResults));
            if (result != null && result.Rows.Count > 0)
            {
                List<Edge> edges = DatabaseHelper.Edges.FromDataTable(result);
                r.Data = JArray.FromObject(edges);
            }
            else
            {
                r.Data = JArray.FromObject(new List<object>());
            }

            r.Time.End = DateTime.Now;
            return r;
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
                // placeholder
            }

            _Disposed = true;
        }
         
        private void InitializeTables()
        { 
            Query(DatabaseHelper.Nodes.CreateTableQuery);
            Query(DatabaseHelper.Edges.CreateTableQuery);
        }

        private DataTable Query(string query)
        {
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(query);
            if (query.Length > MaxStatementLength) throw new ArgumentException("Query exceeds maximum statement length of " + MaxStatementLength + " characters.");

            DataTable result = new DataTable();

            if (Logger.LogQueries) Logger.Log("query: " + query);

            try
            {
                using (SqliteConnection conn = new SqliteConnection(_ConnectionString))
                {
                    conn.Open();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    using (SqliteCommand cmd = new SqliteCommand(query, conn))
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    {
                        using (SqliteDataReader rdr = cmd.ExecuteReader())
                        {
                            result.Load(rdr);
                        }
                    }

                    conn.Close();
                }

                if (Logger.LogResults) Logger.Log("result: " + (result != null ? result.Rows.Count + " rows" : "(null)"));
                return result;
            }
            catch (Exception e)
            {
                e.Data.Add("Query", query);
                throw;
            }
        }

        private List<Node> GetDescendantsFromNode(Node n, string startingGuid, int currentDepth, int maxDepth)
        {
            List<Node> ret = new List<Node>();

            List<SearchFilter> sf = new List<SearchFilter>();
            sf.Add(new SearchFilter(DatabaseHelper.Edges.FromGuidField, SearchCondition.Equals, n.GUID));

            DataTable edgeResult = Query(DatabaseHelper.Edges.SelectEdgesFrom(n.GUID, 0, 0));
            if (edgeResult != null && edgeResult.Rows.Count > 0)
            { 
                List<Edge> edges = DatabaseHelper.Edges.FromDataTable(edgeResult);
                foreach (Edge edge in edges)
                {
                    if (edge.ToGUID.Equals(startingGuid))
                    {
                        _Logging.Log("cycle detected from starting node " + startingGuid + " with edge " + edge.GUID);
                        continue;
                    }
                    else
                    {
                        DataTable nodeResult = Query(DatabaseHelper.Nodes.SelectByGuid(edge.ToGUID));
                        if (nodeResult != null && nodeResult.Rows.Count > 0)
                        {
                            Node node = DatabaseHelper.Nodes.FromDataRow(nodeResult.Rows[0]);
                            if (currentDepth < maxDepth) node.Descendents = GetDescendantsFromNode(node, startingGuid, (currentDepth + 1), maxDepth);
                            ret.Add(node);
                        }
                    }
                }
            }

            return ret;
        }

        #endregion
    }
}
