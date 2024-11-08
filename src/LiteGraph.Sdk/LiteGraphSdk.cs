namespace LiteGraph.Sdk
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// LiteGraph SDK. 
    /// </summary>
    public class LiteGraphSdk : SdkBase
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="endpoint">Endpoint URL.</param>
        public LiteGraphSdk(string endpoint = "http://localhost:8701/") : base(endpoint)
        {

        }

        #endregion

        #region Public-Methods

        #region General-Routes

        #endregion

        #region Graph-APIs

        /// <summary>
        /// Check if a graph exists by GUID.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> GraphExists(Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a graph.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graph.</returns>
        public async Task<Graph> CreateGraph(Guid guid, string name, object data = null, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            string url = Endpoint + "v1.0/graphs";
            return await PutCreate<Graph>(url, new Graph { GUID = guid, Name = name, Data = data }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read graphs.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graphs.</returns>
        public async Task<IEnumerable<Graph>> ReadGraphs(CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs";
            return await GetMany<Graph>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Search graphs.
        /// </summary>
        /// <param name="searchReq">Search request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Search result.</returns>
        public async Task<SearchResult> SearchGraphs(SearchRequest searchReq, CancellationToken token = default)
        {
            if (searchReq == null) throw new ArgumentNullException(nameof(searchReq));

            string url = Endpoint + "v1.0/graphs/search";
            string json = Serializer.SerializeJson(searchReq, true);
            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(json), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                SearchResult result = Serializer.DeserializeJson<SearchResult>(Encoding.UTF8.GetString(bytes));
                return result;
            }

            return null;
        }

        /// <summary>
        /// Read graph.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graph.</returns>
        public async Task<Graph> ReadGraph(Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + guid;
            return await Get<Graph>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a graph.
        /// </summary>
        /// <param name="graph">Graph.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graph.</returns>
        public async Task<Graph> UpdateGraph(Graph graph, CancellationToken token = default)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            string url = Endpoint + "v1.0/graphs/" + graph.GUID;
            return await PutUpdate<Graph>(url, graph, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a graph.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="force">Force recursive deletion of edges and nodes.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteGraph(Guid guid, bool force = false, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + guid;
            if (force) url += "?force";
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Export a graph to GEXF format.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>String containing GEXF XML data.</returns>
        public async Task<string> ExportGraphToGexf(Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + guid + "/export/gexf";
            byte[] bytes = await Get(url, token).ConfigureAwait(false);
            if (bytes != null && bytes.Length > 0) return Encoding.UTF8.GetString(bytes);
            return null;
        }

        /// <summary>
        /// Execute a batch existence request.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="req">Existence request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Existence result.</returns>
        public async Task<ExistenceResult> BatchExistence(Guid graphGuid, ExistenceRequest req, CancellationToken token = default)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/existence";
            return await Post<ExistenceRequest, ExistenceResult>(url, req, token).ConfigureAwait(false);
        }

        #endregion

        #region Node-APIs

        /// <summary>
        /// Check if a node exists by GUID.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> NodeExists(Guid graphGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create multiple nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodes">Nodes.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<List<Node>> CreateNodes(Guid graphGuid, List<Node> nodes, CancellationToken token = default)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (nodes.Count < 1) return new List<Node>();
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/multiple";
            return await PutCreate<List<Node>>(url, nodes, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a node.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Node.</returns>
        public async Task<Node> CreateNode(Node node, CancellationToken token = default)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            string url = Endpoint + "v1.0/graphs/" + node.GraphGUID + "/nodes";
            return await PutCreate<Node>(url, node, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> ReadNodes(Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Search nodes.
        /// </summary>
        /// <param name="searchReq">Search request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Search result.</returns>
        public async Task<SearchResult> SearchNodes(SearchRequest searchReq, CancellationToken token = default)
        {
            if (searchReq == null) throw new ArgumentNullException(nameof(searchReq));

            string url = Endpoint + "v1.0/graphs/" + searchReq.GraphGUID + "/nodes/search";

            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(Serializer.SerializeJson(searchReq, true)), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                SearchResult result = Serializer.DeserializeJson<SearchResult>(Encoding.UTF8.GetString(bytes));
                return result;
            }

            return null;
        }

        /// <summary>
        /// Read a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Node.</returns>
        public async Task<Node> ReadNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid;
            return await Get<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a node.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Node.</returns>
        public async Task<Node> UpdateNode(Node node, CancellationToken token = default)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            string url = Endpoint + "v1.0/graphs/" + node.GraphGUID + "/nodes/" + node.GUID;
            return await PutUpdate<Node>(url, node, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid;
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete all nodes within a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteNodes(Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/all";
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete multiple nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">List of node GUIDs.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteNodes(Guid graphGuid, List<Guid> nodeGuids, CancellationToken token = default)
        {
            if (nodeGuids == null) throw new ArgumentNullException(nameof(nodeGuids));
            if (nodeGuids.Count < 1) return;
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/multiple";
            await Delete<List<Guid>>(url, nodeGuids, token).ConfigureAwait(false);
        }

        #endregion

        #region Edge-APIs

        /// <summary>
        /// Check if an edge exists by GUID.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> EdgeExists(Guid graphGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create multiple edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edges">Edges.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<List<Edge>> CreateEdges(Guid graphGuid, List<Edge> edges, CancellationToken token = default)
        {
            if (edges == null) throw new ArgumentNullException(nameof(edges));
            if (edges.Count < 1) return new List<Edge>();
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges/multiple";
            return await PutCreate<List<Edge>>(url, edges, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create an edge.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edge.</returns>
        public async Task<Edge> CreateEdge(Edge edge, CancellationToken token = default)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            string url = Endpoint + "v1.0/graphs/" + edge.GraphGUID + "/edges";
            return await PutCreate<Edge>(url, edge, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> ReadEdges(Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Search edges.
        /// </summary>
        /// <param name="searchReq">Search request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Search result.</returns>
        public async Task<SearchResult> SearchEdges(SearchRequest searchReq, CancellationToken token = default)
        {
            if (searchReq == null) throw new ArgumentNullException(nameof(searchReq));

            string url = Endpoint + "v1.0/graphs/" + searchReq.GraphGUID + "/edges/search";

            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(Serializer.SerializeJson(searchReq, true)), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                SearchResult result = Serializer.DeserializeJson<SearchResult>(Encoding.UTF8.GetString(bytes));
                return result;
            }

            return null;
        }

        /// <summary>
        /// Read an edge.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid"></param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edge.</returns>
        public async Task<Edge> ReadEdge(Guid graphGuid, Guid edgeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges/" + edgeGuid;
            return await Get<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update an edge.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edge.</returns>
        public async Task<Edge> UpdateEdge(Edge edge, CancellationToken token = default)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            string url = Endpoint + "v1.0/graphs/" + edge.GraphGUID + "/edges/" + edge.GUID;
            return await PutUpdate<Edge>(url, edge, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete an edge.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid"></param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteEdge(Guid graphGuid, Guid edgeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges/" + edgeGuid;
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete multiple edges within a graph.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuids">Edge GUIDs.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteEdges(Guid graphGuid, List<Guid> edgeGuids, CancellationToken token = default)
        {
            if (edgeGuids == null) throw new ArgumentNullException(nameof(edgeGuids));
            if (edgeGuids.Count < 1) return;
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges/multiple";
            await Delete<List<Guid>>(url, edgeGuids, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete all edges.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteEdges(Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges/all";
            await Delete(url, token).ConfigureAwait(false);
        }

        #endregion

        #region Routes-and-Traversal

        /// <summary>
        /// Get edges from a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetEdgesFromNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/edges/from";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get edges to a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetEdgesToNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/edges/to";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get edges from a given node to a given node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetEdgesBetween(
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges/between?from=" + fromNodeGuid + "&to=" + toNodeGuid;
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all edges to or from a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetAllNodeEdges(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/edges";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get child nodes from a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> GetChildrenFromNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/children";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get parent nodes from a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> GetParentsFromNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/parents";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get neighboring nodes from a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> GetNodeNeighbors(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/neighbors";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get routes between two nodes.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Routes.</returns>
        public async Task<RouteResult> GetRoutes(Guid graphGuid, Guid fromNodeGuid, Guid toNodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/routes";
            
            RouteRequest req = new RouteRequest
            {
                Graph = graphGuid,
                From = fromNodeGuid,
                To = toNodeGuid
            };

            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(Serializer.SerializeJson(req, true)), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                RouteResult resp = Serializer.DeserializeJson<RouteResult>(Encoding.UTF8.GetString(bytes));
                return resp;
            }

            return null;
        }

        #endregion

        #endregion

        #region Private-Methods

        #endregion
    }
}
