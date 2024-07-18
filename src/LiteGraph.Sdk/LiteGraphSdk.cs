namespace LiteGraph.Sdk
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

        #region Graph-Routes

        /// <summary>
        /// Create a graph.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graph.</returns>
        public async Task<Graph> CreateGraph(string name, object data = null, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            string url = Endpoint + "v1.0/graphs";
            return await PutCreate<Graph>(url, new Graph { Name = name, Data = data }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read graphs.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graphs.</returns>
        public async Task<IEnumerable> ReadGraphs(CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs";
            return await GetMany<Graph>(url, token).ConfigureAwait(false);
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

        #endregion

        #region Node-Routes

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
        public async Task<IEnumerable> ReadNodes(Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
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

        #endregion

        #region Edge-Routes

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
        public async Task<IEnumerable> ReadEdges(Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/edges";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
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

        #endregion

        #region Routes-and-Traversal

        /// <summary>
        /// Get edges from a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable> GetEdgesFromNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
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
        public async Task<IEnumerable> GetEdgesToNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/edges/to";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all edges to or from a node.
        /// </summary>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable> GetAllNodeEdges(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
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
        public async Task<IEnumerable> GetChildrenFromNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
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
        public async Task<IEnumerable> GetParentsFromNode(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
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
        public async Task<IEnumerable> GetNodeNeighbors(Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
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
        public async Task<RouteResponse> GetRoutes(Guid graphGuid, Guid fromNodeGuid, Guid toNodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/graphs/" + graphGuid + "/routes";
            
            RouteRequest req = new RouteRequest
            {
                Graph = graphGuid,
                From = fromNodeGuid,
                To = toNodeGuid
            };

            byte[] bytes = await Post(url, Encoding.UTF8.GetBytes(Serializer.SerializeJson(req, true)), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                RouteResponse resp = Serializer.DeserializeJson<RouteResponse>(Encoding.UTF8.GetString(bytes));
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
