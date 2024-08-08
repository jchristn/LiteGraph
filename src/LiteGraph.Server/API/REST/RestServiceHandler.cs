namespace LiteGraph.Server.API.REST
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using LiteGraph.Serialization;
    using LiteGraph.Server.Classes;
    using SyslogLogging;
    using WatsonWebserver;
    using WatsonWebserver.Core;

    internal class RestServiceHandler
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Internal-Members

        #endregion

        #region Private-Members

        private readonly string _Header = "[RestServiceHandler] ";
        static string _Hostname = Dns.GetHostName();
        private Settings _Settings = null;
        private LoggingModule _Logging = null;
        private LiteGraphClient _LiteGraph = null;
        private SerializationHelper _Serializer = null;

        private Webserver _Webserver = null;

        private List<string> _Localhost = new List<string>
        {
            "127.0.0.1",
            "localhost",
            "::1"
        };

        #endregion

        #region Constructors-and-Factories

        internal RestServiceHandler(
            Settings settings,
            LoggingModule logging,
            LiteGraphClient litegraph,
            SerializationHelper serializer)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _Logging = logging ?? throw new ArgumentNullException(nameof(logging));
            _LiteGraph = litegraph ?? throw new ArgumentNullException(nameof(litegraph));
            _Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            _Webserver = new Webserver(_Settings.Rest, DefaultRequestHandler);
            _Webserver.Routes.PreRouting = PreRoutingHandler;
            _Webserver.Routes.AuthenticateRequest = AuthenticateRequest;
            _Webserver.Routes.PostRouting = PostRoutingHandler;

            InitializeRoutes();

            _Logging.Info(_Header + "starting REST server on " + _Settings.Rest.Prefix);
            _Webserver.Start();

            if (_Localhost.Contains(_Settings.Rest.Hostname))
            {
                Console.WriteLine("");
                Console.WriteLine("Important!");
                Console.WriteLine("| Configured to listen on localhost; LiteGraph will not be externally accessible");
                Console.WriteLine("| Modify " + Constants.SettingsFile + " to change the REST listener hostname");
                Console.WriteLine("");
            }
        }

        #endregion

        #region Internal-Methods

        internal void InitializeRoutes()
        {
            _Webserver.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/", RootRoute, ExceptionRoute);
            _Webserver.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/favicon.ico", FaviconRoute, ExceptionRoute);

            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.PUT, "/v1.0/graphs", GraphCreateRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.POST, "/v1.0/graphs/search", GraphSearchRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}", GraphReadRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.HEAD, "/v1.0/graphs/{graphGuid}", GraphExistsRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs", GraphReadManyRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.PUT, "/v1.0/graphs/{graphGuid}", GraphUpdateRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.DELETE, "/v1.0/graphs/{graphGuid}", GraphDeleteRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/export/gexf", GraphGexfExportRoute, ExceptionRoute);

            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.PUT, "/v1.0/graphs/{graphGuid}/nodes", NodeCreateRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.POST, "/v1.0/graphs/{graphGuid}/nodes/search", NodeSearchRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}", NodeReadRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.HEAD, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}", NodeExistsRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes", NodeReadManyRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.PUT, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}", NodeUpdateRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.DELETE, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}", NodeDeleteRoute, ExceptionRoute);

            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.PUT, "/v1.0/graphs/{graphGuid}/edges", EdgeCreateRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/edges/between", EdgesBetweenRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.POST, "/v1.0/graphs/{graphGuid}/edges/search", EdgeSearchRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/edges/{edgeGuid}", EdgeReadRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.HEAD, "/v1.0/graphs/{graphGuid}/edges/{edgeGuid}", EdgeExistsRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/edges", EdgeReadManyRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.PUT, "/v1.0/graphs/{graphGuid}/edges/{edgeGuid}", EdgeUpdateRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.DELETE, "/v1.0/graphs/{graphGuid}/edges/{edgeGuid}", EdgeDeleteRoute, ExceptionRoute);

            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}/edges/from", EdgesFromNodeRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}/edges/to", EdgesToNodeRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}/edges", AllEdgesToNodeRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}/neighbors", NodeNeighborsRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}/parents", NodeParentsRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/v1.0/graphs/{graphGuid}/nodes/{nodeGuid}/children", NodeChildrenRoute, ExceptionRoute);
            _Webserver.Routes.PostAuthentication.Parameter.Add(HttpMethod.POST, "/v1.0/graphs/{graphGuid}/routes", GetRoutesRoute, ExceptionRoute);
        }

        internal async Task PreRoutingHandler(HttpContextBase ctx)
        {
            ctx.Response.Headers.Add(Constants.HostnameHeader, _Hostname);
            ctx.Response.ContentType = Constants.JsonContentType;

            if (_Settings.Debug.Requests)
                _Logging.Debug(_Serializer.SerializeJson(ctx.Request, true));
        }

        internal async Task AuthenticateRequest(HttpContextBase ctx)
        {
            return;
        }

        internal async Task DefaultRequestHandler(HttpContextBase ctx)
        {
            _Logging.Warn(_Header + "unknown verb or endpoint: " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
            ctx.Response.StatusCode = 400;
            await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest), true));
        }

        internal async Task PostRoutingHandler(HttpContextBase ctx)
        {
            string msg =
                _Header
                + ctx.Request.Method.ToString() + " " + ctx.Request.Url.RawWithQuery + " "
                + ctx.Response.StatusCode + " "
                + ctx.Timestamp.TotalMs + "ms";

            if (ctx.Response.StatusCode > 299 && _Settings.Debug.Requests)
                msg += Environment.NewLine + ctx.Response.DataAsString;

            ctx.Timestamp.End = DateTime.UtcNow;
            _Logging.Debug(msg);
        }

        #endregion

        #region Private-Route-Implementations

        private async Task ExceptionRoute(HttpContextBase ctx, Exception e)
        {
            if (_Settings.Debug.Exceptions) _Logging.Exception(e);

            if (e is JsonException)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.DeserializationError), true));
            }
            else if (e is ArgumentException)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
            else
            {
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.InternalError, null, e.Message), true));
            }
        }

        private async Task RootRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.HtmlContentType;
            await ctx.Response.Send(Constants.DefaultHomepage);
        }

        private async Task FaviconRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.FaviconContentType;
            await ctx.Response.Send(File.ReadAllBytes(Constants.FaviconFile));
        }

        private async Task NoRequestBody(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest), true));
        }

        #region Graph-Routes

        private async Task GraphCreateRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NoRequestBody(ctx);
                return;
            }

            Graph graph = _Serializer.DeserializeJson<Graph>(ctx.Request.DataAsString);
            graph = _LiteGraph.CreateGraph(graph.GUID, graph.Name, graph.Data);

            ctx.Response.StatusCode = 201;
            await ctx.Response.Send(_Serializer.SerializeJson(graph, true));
        }

        private async Task GraphReadManyRoute(HttpContextBase ctx)
        {
            List<Graph> graphs = _LiteGraph.ReadGraphs().ToList();
            await ctx.Response.Send(_Serializer.SerializeJson(graphs, true));
        }

        private async Task GraphSearchRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await GraphReadManyRoute(ctx);
                return;
            }

            try
            {
                SearchRequest req = _Serializer.DeserializeJson<SearchRequest>(ctx.Request.DataAsString);

                SearchResult resp = new SearchResult();
                resp.Graphs = _LiteGraph.ReadGraphs(req.Expr, req.Ordering).ToList();
                await ctx.Response.Send(_Serializer.SerializeJson(resp, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task GraphReadRoute(HttpContextBase ctx)
        {
            Graph graph = _LiteGraph.ReadGraph(Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]));
            if (graph == null)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.NotFound), true));
            }
            else
            {
                await ctx.Response.Send(_Serializer.SerializeJson(graph, true));
            }
        }

        private async Task GraphExistsRoute(HttpContextBase ctx)
        {
            Graph graph = _LiteGraph.ReadGraph(Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]));
            if (graph == null)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send();
            }
            else
            {
                await ctx.Response.Send();
            }
        }

        private async Task GraphUpdateRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NoRequestBody(ctx);
                return;
            }

            Graph graph = _Serializer.DeserializeJson<Graph>(ctx.Request.DataAsString);
            graph = _LiteGraph.UpdateGraph(graph);

            if (graph == null)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.NotFound), true));
            }
            else
            {
                await ctx.Response.Send(_Serializer.SerializeJson(graph, true));
            }
        }

        private async Task GraphDeleteRoute(HttpContextBase ctx)
        {
            bool force = ctx.Request.QuerystringExists("force");

            try
            {
                _LiteGraph.DeleteGraph(
                    Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                    force);
                ctx.Response.StatusCode = 204;
                await ctx.Response.Send();
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task GraphGexfExportRoute(HttpContextBase ctx)
        {
            bool inclData = ctx.Request.QuerystringExists("incldata");

            try
            {
                string xml = _LiteGraph.RenderGraphAsGexf(
                    Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                    inclData);

                ctx.Response.ContentType = Constants.XmlContentType;
                await ctx.Response.Send(xml);
            }
            catch (ArgumentException)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.NotFound), true));
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.InternalError, null, e.Message), true));
            }
        }

        #endregion

        #region Node-Routes

        private async Task NodeCreateRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NoRequestBody(ctx);
                return;
            }

            try
            {
                Node node = _Serializer.DeserializeJson<Node>(ctx.Request.DataAsString);
                node.GraphGUID = Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]);
                node = _LiteGraph.CreateNode(node);
                ctx.Response.StatusCode = 201;
                await ctx.Response.Send(_Serializer.SerializeJson(node, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task NodeReadManyRoute(HttpContextBase ctx)
        {
            List<Node> nodes = _LiteGraph.ReadNodes(Guid.Parse(ctx.Request.Url.Parameters["graphGuid"])).ToList();
            await ctx.Response.Send(_Serializer.SerializeJson(nodes, true));
        }

        private async Task NodeSearchRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NodeReadManyRoute(ctx);
                return;
            }

            try
            {
                SearchRequest req = _Serializer.DeserializeJson<SearchRequest>(ctx.Request.DataAsString);
                req.GraphGUID = Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]);

                SearchResult resp = new SearchResult();
                resp.Nodes = _LiteGraph.ReadNodes(Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]), req.Expr, req.Ordering).ToList();
                await ctx.Response.Send(_Serializer.SerializeJson(resp, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task NodeReadRoute(HttpContextBase ctx)
        {
            Node node = _LiteGraph.ReadNode(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]));

            if (node == null)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.NotFound), true));
            }
            else
            {
                await ctx.Response.Send(_Serializer.SerializeJson(node, true));
            }
        }

        private async Task NodeExistsRoute(HttpContextBase ctx)
        {
            Node node = _LiteGraph.ReadNode(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]));

            if (node == null)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send();
            }
            else
            {
                await ctx.Response.Send();
            }
        }

        private async Task NodeUpdateRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NoRequestBody(ctx);
                return;
            }

            try
            {
                Node node = _Serializer.DeserializeJson<Node>(ctx.Request.DataAsString);
                node.GraphGUID = Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]);
                node = _LiteGraph.UpdateNode(node);
                await ctx.Response.Send(_Serializer.SerializeJson(node, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task NodeDeleteRoute(HttpContextBase ctx)
        {
            _LiteGraph.DeleteNode(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]));

            ctx.Response.StatusCode = 204;
            await ctx.Response.Send();
        }

        #endregion

        #region Edge-Routes

        private async Task EdgeCreateRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NoRequestBody(ctx);
                return;
            }

            try
            {
                Edge edge = _Serializer.DeserializeJson<Edge>(ctx.Request.DataAsString);
                edge.GraphGUID = Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]);
                edge = _LiteGraph.CreateEdge(edge);
                ctx.Response.StatusCode = 201;
                await ctx.Response.Send(_Serializer.SerializeJson(edge, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task EdgeReadManyRoute(HttpContextBase ctx)
        {
            List<Edge> edges = _LiteGraph.ReadEdges(Guid.Parse(ctx.Request.Url.Parameters["graphGuid"])).ToList();
            await ctx.Response.Send(_Serializer.SerializeJson(edges, true));
        }

        private async Task EdgesBetweenRoute(HttpContextBase ctx)
        {
            string from = GetQueryValue(ctx.Request.Query.Elements, "from");
            string to = GetQueryValue(ctx.Request.Query.Elements, "to");

            Guid fromGuid;
            Guid toGuid;

            if (String.IsNullOrEmpty(from) || String.IsNullOrEmpty(to))
            {
                _Logging.Warn(_Header + "either to or from is missing from query");
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Both 'to' and 'from' must be supplied in the query.")));
                return;
            }

            if (!Guid.TryParse(from, out fromGuid)
                || !Guid.TryParse(to, out toGuid))
            {
                _Logging.Warn(_Header + "supplied GUIDs are not parseable");
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Supplied GUIDs in query are not parseable.")));
                return;
            }

            List<Edge> edges = _LiteGraph.GetEdgesBetween(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                fromGuid,
                toGuid).ToList();

            await ctx.Response.Send(_Serializer.SerializeJson(edges, true));
        }

        private async Task EdgeSearchRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await EdgeReadManyRoute(ctx);
                return;
            }

            try
            {
                SearchRequest req = _Serializer.DeserializeJson<SearchRequest>(ctx.Request.DataAsString);
                req.GraphGUID = Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]);

                SearchResult resp = new SearchResult();
                resp.Edges = _LiteGraph.ReadEdges(Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]), req.Expr, req.Ordering).ToList();
                await ctx.Response.Send(_Serializer.SerializeJson(resp, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task EdgeReadRoute(HttpContextBase ctx)
        {
            Edge edge = _LiteGraph.ReadEdge(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["edgeGuid"]));

            if (edge == null)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.NotFound), true));
            }
            else
            {
                await ctx.Response.Send(_Serializer.SerializeJson(edge, true));
            }
        }

        private async Task EdgeExistsRoute(HttpContextBase ctx)
        {
            Edge edge = _LiteGraph.ReadEdge(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["edgeGuid"]));

            if (edge == null)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send();
            }
            else
            {
                await ctx.Response.Send();
            }
        }

        private async Task EdgeUpdateRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NoRequestBody(ctx);
                return;
            }

            try
            {
                Edge edge = _Serializer.DeserializeJson<Edge>(ctx.Request.DataAsString);
                edge.GraphGUID = Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]);
                edge = _LiteGraph.UpdateEdge(edge);
                await ctx.Response.Send(_Serializer.SerializeJson(edge, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        private async Task EdgeDeleteRoute(HttpContextBase ctx)
        {
            _LiteGraph.DeleteEdge(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["edgeGuid"]));

            ctx.Response.StatusCode = 204;
            await ctx.Response.Send();
        }

        #endregion

        #region Routes-and-Traversal

        private async Task EdgesFromNodeRoute(HttpContextBase ctx)
        {
            List<Edge> edges = _LiteGraph.GetEdgesFrom(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]),
                null,
                EnumerationOrderEnum.CreatedDescending).ToList();

            await ctx.Response.Send(_Serializer.SerializeJson(edges, true));
        }

        private async Task EdgesToNodeRoute(HttpContextBase ctx)
        {
            List<Edge> edges = _LiteGraph.GetEdgesTo(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]),
                null,
                EnumerationOrderEnum.CreatedDescending).ToList();

            await ctx.Response.Send(_Serializer.SerializeJson(edges, true));
        }

        private async Task AllEdgesToNodeRoute(HttpContextBase ctx)
        {
            List<Edge> edgesFrom = _LiteGraph.GetEdgesFrom(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]),
                null,
                EnumerationOrderEnum.CreatedDescending).ToList();

            List<Edge> edgesTo = _LiteGraph.GetEdgesTo(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]),
                null,
                EnumerationOrderEnum.CreatedDescending).ToList();

            List<Edge> edges = new List<Edge>();
            edges.AddRange(edgesFrom);
            edges.AddRange(edgesTo);
            edges = edges.DistinctBy(e => e.GUID).ToList();

            await ctx.Response.Send(_Serializer.SerializeJson(edges, true));
        }

        private async Task NodeChildrenRoute(HttpContextBase ctx)
        {
            List<Node> nodes = _LiteGraph.GetChildren(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]),
                null,
                EnumerationOrderEnum.CreatedDescending).ToList();
            await ctx.Response.Send(_Serializer.SerializeJson(nodes, true));
        }

        private async Task NodeParentsRoute(HttpContextBase ctx)
        {
            List<Node> nodes = _LiteGraph.GetParents(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]),
                null,
                EnumerationOrderEnum.CreatedDescending).ToList();
            await ctx.Response.Send(_Serializer.SerializeJson(nodes, true));
        }

        private async Task NodeNeighborsRoute(HttpContextBase ctx)
        {
            List<Node> nodes = _LiteGraph.GetNeighbors(
                Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]),
                Guid.Parse(ctx.Request.Url.Parameters["nodeGuid"]),
                null,
                null,
                EnumerationOrderEnum.CreatedDescending).ToList();
            await ctx.Response.Send(_Serializer.SerializeJson(nodes, true));
        }

        private async Task GetRoutesRoute(HttpContextBase ctx)
        {
            if (String.IsNullOrEmpty(ctx.Request.DataAsString))
            {
                await NoRequestBody(ctx);
                return;
            }

            try
            {
                RouteResponse resp = new RouteResponse();
                RouteRequest req = _Serializer.DeserializeJson<RouteRequest>(ctx.Request.DataAsString);
                req.Graph = Guid.Parse(ctx.Request.Url.Parameters["graphGuid"]);
                List<RouteDetail> routes = _LiteGraph.GetRoutes(
                    SearchTypeEnum.DepthFirstSearch,
                    req.Graph,
                    req.From,
                    req.To,
                    req.EdgeFilter,
                    req.NodeFilter).ToList();

                routes = routes.OrderBy(r => r.TotalCost).ToList();
                resp.Routes = routes;
                resp.Timestamp.End = DateTime.UtcNow;
                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(resp, true));
            }
            catch (ArgumentException e)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
            }
        }

        #endregion

        #endregion

        #region Private-Methods

        private string GetQueryValue(NameValueCollection nvc, string key)
        {
            if (nvc != null && nvc.AllKeys.Contains(key)) return nvc[key];
            return null;
        }

        #endregion

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
