using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// Route finder.
    /// Uses depth first search.
    /// </summary>
    public class RouteFinder
    {
        #region Public-Members

        /// <summary>
        /// Globally-unique identifier of the source node.
        /// </summary>
        public string FromGuid { get; set; } = null;

        /// <summary>
        /// Globally-unique identifier of the target node.
        /// </summary>
        public string ToGuid { get; set; } = null;

        /// <summary>
        /// Subset of edge types in which to search.
        /// </summary>
        public List<string> Types
        {
            get
            {
                return _Types;
            }
            set
            {
                if (value == null) _Types = new List<string>();
                else _Types = value;
            }
        }

        /// <summary>
        /// Filters to apply against each edge's JSON data.
        /// </summary>
        public List<SearchFilter> Filters
        {
            get
            {
                return _Filters;
            }
            set
            {
                if (value == null) _Filters = new List<SearchFilter>();
                else _Filters = value;
            }
        }
          
        #endregion

        #region Private-Members

        private LiteGraphClient _Client = null;
        private List<string> _Types = new List<string>();
        private List<SearchFilter> _Filters = new List<SearchFilter>(); 

        private Node _FromNode = null;
        private Node _ToNode = null;

        private List<string> _NodeGuidsVisited = new List<string>();
        private List<Node> _NodesVisited = new List<Node>();

        private List<string> _EdgeGuidsVisited = new List<string>();
        private List<Edge> _EdgesVisited = new List<Edge>();

        private List<List<string>> _ValidPaths = new List<List<string>>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="client">LiteGraphClient.</param>
        public RouteFinder(LiteGraphClient client)
        {
            _Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Find routes that exist between two nodes.
        /// </summary>
        /// <returns>Graph result.</returns>
        public GraphResult Find()
        {
            if (String.IsNullOrEmpty(FromGuid)) throw new ArgumentNullException(nameof(FromGuid));
            if (String.IsNullOrEmpty(ToGuid)) throw new ArgumentNullException(nameof(ToGuid));
            if (FromGuid.Equals(ToGuid)) throw new ArgumentException("From GUID and to GUID cannot be the same.");

            GraphResult r = new GraphResult(GraphOperation.FindRoutes);
            
            if (!GetInitialNodes()) 
            {
                _Client.Logger.Log("unable to find either from or to node");
                r.Time.End = DateTime.Now;
                return r;
            }
            
            r.Routes = new List<RouteDetail>();

            SearchRecursive(_FromNode, new List<string>());

            if (_ValidPaths != null && _ValidPaths.Count > 0)
            {
                foreach (List<string> path in _ValidPaths)
                {
                    RouteDetail rd = new RouteDetail();
                    rd.Edges = EdgeGuidsToEdges(path);
                    r.Routes.Add(rd);
                }
            }

            r.Time.End = DateTime.Now;
            return r;
        }

        #endregion

        #region Private-Methods

        private bool GetInitialNodes()
        {
            _FromNode = GetNode(FromGuid);
            if (_FromNode == null)
            {
                _Client.Logger.Log("unable to find from node " + FromGuid);
                return false;
            }

            _ToNode = GetNode(ToGuid);
            if (_ToNode == null)
            {
                _Client.Logger.Log("unable to find to node " + ToGuid);
                return false;
            }

            return true;
        }

        private Node GetNode(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult node = _Client.GetNode(guid);
            if (node != null && node.Data != null) return ((JObject)node.Data).ToObject<Node>();
            return null;
        }

        private Edge GetEdge(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            GraphResult edge = _Client.GetEdge(guid);
            if (edge != null && edge.Data != null) return ((JObject)edge.Data).ToObject<Edge>();
            return null;
        }

        private List<Edge> GetEdgesFromNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (String.IsNullOrEmpty(node.GUID)) throw new ArgumentException("Supplied node has no GUID.");
            return GetEdgesFromNode(node.GUID);
        }

        private List<Edge> GetEdgesFromNode(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            GraphResult edgesFromNode = _Client.GetEdgesFrom(guid, _Types, _Filters);

            if (edgesFromNode == null)
            {
                _Client.Logger.Log("no edges from node " + guid);
                return new List<Edge>();
            }

            List<Edge> edges = ((JArray)edgesFromNode.Data).ToObject<List<Edge>>();
            return edges;
        }

        private void SearchRecursive(Node startingNode, List<string> currentPath)
        {
            List<Edge> edges = GetEdgesFromNode(startingNode);
            if (edges == null || edges.Count < 1)
            {
                _Client.Logger.Log("node " + startingNode.GUID + " has no outbound edges");
                return;
            }

            foreach (Edge edge in edges)
            {
                List<string> updatedPath = null;

                if (edge.ToGUID.Equals(ToGuid))
                {
                    _Client.Logger.Log("edge " + edge.GUID + " connects to node " + ToGuid);
                    
                    updatedPath = new List<string>(currentPath);
                    updatedPath.Add(edge.GUID);
                    _ValidPaths.Add(updatedPath);

                    _EdgesVisited.Add(edge);
                    _EdgeGuidsVisited.Add(edge.GUID);

                    return;
                }
                else if (_EdgeGuidsVisited.Contains(edge.GUID))
                {
                    _Client.Logger.Log("edge " + edge.GUID + " already visited");

                    // check for existing valid routes including this edge
                    if (_ValidPaths.Exists(p => p.Contains(edge.GUID)))
                    {
                        _Client.Logger.Log("edge " + edge.GUID + " has route to node " + ToGuid);

                        List<List<string>> validPaths = _ValidPaths.Where(p => p.Contains(edge.GUID)).ToList<List<string>>();
                        foreach (List<string> validPath in validPaths)
                        {
                            if (!validPath.Contains(edge.GUID)) continue;
                            List<string> subset = validPath.GetRange(validPath.IndexOf(edge.GUID), validPath.Count - validPath.IndexOf(edge.GUID));
                            currentPath.AddRange(subset);
                            _ValidPaths.Add(currentPath);
                            return;
                        }
                    }

                    continue;
                }
                else
                {
                    Node nextNode = GetNode(edge.ToGUID);
                    if (nextNode == null)
                    {
                        _Client.Logger.Log("edge " + edge.GUID + " references unknown node " + edge.ToGUID);
                        return;
                    }

                    updatedPath = new List<string>(currentPath);
                    updatedPath.Add(edge.GUID);

                    _EdgesVisited.Add(edge);
                    _EdgeGuidsVisited.Add(edge.GUID);

                    SearchRecursive(nextNode, updatedPath);
                }
            } 
        } 

        private List<Edge> EdgeGuidsToEdges(List<string> guids)
        {
            List<Edge> ret = new List<Edge>();

            foreach (string guid in guids)
            {
                ret.Add(_EdgesVisited.First(e => e.GUID.Equals(guid)));
            }

            return ret;
        }

        #endregion
    }
}
