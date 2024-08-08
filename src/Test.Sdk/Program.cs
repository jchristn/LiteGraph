namespace Test.Sdk
{
    using System;
    using ExpressionTree;
    using GetSomeInput;
    using LiteGraph.Sdk;

    public static class Program
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.

        private static bool _Debug = true;
        private static bool _RunForever = true;
        private static string _Endpoint = "http://localhost:8701";
        private static LiteGraphSdk _Sdk = null;

        public static void Main(string[] args)
        {
            _Sdk = new LiteGraphSdk(Inputty.GetString("Endpoint:", _Endpoint, false));
            if (_Debug) _Sdk.Logger = Logger;

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [?/help]:", null, false);

                switch (userInput)
                {
                    case "?":
                        Menu();
                        break;
                    case "q":
                        _RunForever = false;
                        break;
                    case "cls":
                        Console.Clear();
                        break;

                    case "graph exists":
                        GraphExists();
                        break;
                    case "graph create":
                        GraphCreate();
                        break;
                    case "graph update":
                        GraphUpdate();
                        break;
                    case "graph all":
                        GraphAll();
                        break;
                    case "graph read":
                        GraphRead();
                        break;
                    case "graph delete":
                        GraphDelete();
                        break;
                    case "graph search":
                        GraphSearch();
                        break;

                    case "node exists":
                        NodeExists();
                        break;
                    case "node create":
                        NodeCreate();
                        break;
                    case "node update":
                        NodeUpdate();
                        break;
                    case "node all":
                        NodeAll();
                        break;
                    case "node read":
                        NodeRead();
                        break;
                    case "node delete":
                        NodeDelete();
                        break;
                    case "node search":
                        NodeSearch();
                        break;

                    case "node edges":
                        NodeEdges();
                        break;
                    case "node parents":
                        NodeParents();
                        break;
                    case "node children":
                        NodeChildren();
                        break;

                    case "edge exists":
                        EdgeExists();
                        break;
                    case "edge create":
                        EdgeCreate();
                        break;
                    case "edge update":
                        EdgeUpdate();
                        break;
                    case "edge all":
                        EdgeAll();
                        break;
                    case "edge read":
                        EdgeRead();
                        break;
                    case "edge delete":
                        EdgeDelete();
                        break;
                    case "edge search":
                        EdgeSearch();
                        break;

                    case "edges from":
                        EdgesFrom();
                        break;
                    case "edges to":
                        EdgesTo();
                        break;

                    case "route":
                        Route();
                        break;
                }

                Console.WriteLine("");
            }
        }

        private static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  ?               help, this menu");
            Console.WriteLine("  q               quit");
            Console.WriteLine("  cls             clear the screen");
            Console.WriteLine("");
            Console.WriteLine("Graph commands:");
            Console.WriteLine("  graph create   graph update   graph all      graph read");
            Console.WriteLine("  graph update   graph delete   graph exists   graph search");
            Console.WriteLine("");
            Console.WriteLine("Node commands:");
            Console.WriteLine("  node create    node update    node all       node read");
            Console.WriteLine("  node update    node delete    node search");
            Console.WriteLine("  node edges     node parents   node children");
            Console.WriteLine("");
            Console.WriteLine("Edge commands:");
            Console.WriteLine("  edge create    edge update    edge all       edge read");
            Console.WriteLine("  edge update    edge delete    edges from     edges to");
            Console.WriteLine("  edge search");
            Console.WriteLine("");
            Console.WriteLine("Routing commands:");
            Console.WriteLine("  route");
            Console.WriteLine("");
        }

        private static void Logger(SeverityEnum sev, string msg)
        {
            Console.WriteLine(sev.ToString() + " " + msg);
        }

        private static string GetName()
        {
            return Inputty.GetString("Name:", null, false);
        }

        private static object GetData()
        {
            string val = GetJson();
            if (!String.IsNullOrEmpty(val)) return Serializer.DeserializeJson<object>(val);
            return null;
        }

        private static string GetJson()
        {
            return Inputty.GetString("JSON:", null, true);
        }

        private static Guid GetGuid(string prompt)
        {
            return Inputty.GetGuid(prompt, default(Guid));
        }

        private static bool GetBoolean(string prompt)
        {
            return Inputty.GetBoolean(prompt, true);
        }

        private static SearchRequest BuildSearchRequest()
        {
            Expr expr = GetExpression();
            if (expr == null) return null;

            SearchRequest req = new SearchRequest();
            req.GraphGUID = Inputty.GetGuid("Graph GUID:", default(Guid));
            req.Expr = expr;
            return req;
        }

        static Expr GetExpression()
        {
            Console.WriteLine("");
            Console.WriteLine("Example expressions:");

            Expr e1 = new Expr("Age", OperatorEnum.GreaterThan, 38);
            e1.PrependAnd("Hobby.Name", OperatorEnum.Equals, "BJJ");
            Console.WriteLine(Serializer.SerializeJson(e1, false));

            Expr e2 = new Expr("Mbps", OperatorEnum.GreaterThan, 250);
            Console.WriteLine(Serializer.SerializeJson(e2, false));
            Console.WriteLine("");

            string json = Inputty.GetString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return null;

            Expr expr = Serializer.DeserializeJson<Expr>(json);
            Console.WriteLine("");
            Console.WriteLine("Using expression: " + expr.ToString());
            Console.WriteLine("");
            return expr;
        }

        private static void EnumerateResult(object obj)
        {
            Console.WriteLine("");
            Console.Write("Result: ");
            if (obj == null) Console.WriteLine("(null)");
            else Console.WriteLine(Environment.NewLine + Serializer.SerializeJson(obj, true));
            Console.WriteLine("");
        }

        #region Graph

        private static void GraphExists()
        {
            EnumerateResult(_Sdk.GraphExists(GetGuid("GUID:")).Result);
        }

        private static void GraphCreate()
        {
            EnumerateResult(_Sdk.CreateGraph(GetGuid("Graph GUID:"), GetName(), GetData()).Result);
        }

        private static void GraphAll()
        {
            EnumerateResult(_Sdk.ReadGraphs().Result);
        }

        private static void GraphRead()
        {
            EnumerateResult(_Sdk.ReadGraph(GetGuid("GUID:")).Result);
        }

        private static void GraphUpdate()
        {
            EnumerateResult(_Sdk.UpdateGraph(Serializer.DeserializeJson<Graph>(GetJson())).Result);
        }

        private static void GraphDelete()
        {
            _Sdk.DeleteGraph(GetGuid("GUID:"), GetBoolean("Force:")).Wait();
        }

        private static void GraphSearch()
        {
            SearchRequest req = BuildSearchRequest();
            if (req == null) return;
            EnumerateResult(_Sdk.SearchGraphs(req).Result);
        }

        #endregion

        #region Node

        private static void NodeExists()
        {
            EnumerateResult(
                _Sdk.NodeExists(
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeCreate()
        {
            EnumerateResult(
                _Sdk.CreateNode(
                    Serializer.DeserializeJson<Node>(GetJson())
                )
                .Result);
        }

        private static void NodeAll()
        {
            EnumerateResult(_Sdk.ReadNodes(GetGuid("Graph GUID:")).Result);
        }

        private static void NodeRead()
        {
            EnumerateResult(
                _Sdk.ReadNode(
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeUpdate()
        {
            EnumerateResult(
                _Sdk.UpdateNode(
                    Serializer.DeserializeJson<Node>(GetJson())
                )
                .Result);
        }

        private static void NodeDelete()
        {
            _Sdk.DeleteNode(
                GetGuid("Graph GUID:"), 
                GetGuid("Node GUID:")
                )
                .Wait();
        }
        
        private static void NodeSearch()
        {
            SearchRequest req = BuildSearchRequest();
            if (req == null) return;
            EnumerateResult(_Sdk.SearchNodes(req).Result);
        }

        private static void NodeEdges()
        {
            EnumerateResult(
                _Sdk.GetAllNodeEdges(
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeParents()
        {
            EnumerateResult(
                _Sdk.GetParentsFromNode(
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeChildren()
        {
            EnumerateResult(
                _Sdk.GetChildrenFromNode(
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        #endregion

        #region Edge

        private static void EdgeExists()
        {
            EnumerateResult(
                _Sdk.EdgeExists(
                    GetGuid("Graph GUID:"),
                    GetGuid("Edge GUID:")
                )
                .Result);
        }

        private static void EdgeCreate()
        {
            EnumerateResult(
                _Sdk.CreateEdge(
                    Serializer.DeserializeJson<Edge>(GetJson())
                )
                .Result);
        }

        private static void EdgeAll()
        {
            EnumerateResult(_Sdk.ReadEdges(GetGuid("Graph GUID:")).Result);
        }

        private static void EdgeRead()
        {
            EnumerateResult(
                _Sdk.ReadEdge(
                    GetGuid("Graph GUID:"),
                    GetGuid("Edge GUID:")
                )
                .Result);
        }

        private static void EdgeUpdate()
        {
            EnumerateResult(
                _Sdk.UpdateEdge(
                    Serializer.DeserializeJson<Edge>(GetJson())
                )
                .Result);
        }

        private static void EdgeDelete()
        {
            _Sdk.DeleteEdge(
                GetGuid("Graph GUID:"),
                GetGuid("Edge GUID:")
                )
                .Wait();
        }

        private static void EdgeSearch()
        {
            SearchRequest req = BuildSearchRequest();
            if (req == null) return;
            EnumerateResult(_Sdk.SearchEdges(req).Result);
        }

        private static void EdgesFrom()
        {
            _Sdk.GetEdgesFromNode(
                GetGuid("Graph GUID:"),
                GetGuid("Node GUID:")
                )
                .Wait();
        }

        private static void EdgesTo()
        {
            _Sdk.GetEdgesToNode(
                GetGuid("Graph GUID:"),
                GetGuid("Edge GUID:")
                )
                .Wait();
        }

        #endregion

        #region Route

        private static void Route()
        {
            EnumerateResult(
                _Sdk.GetRoutes(
                    GetGuid("Graph GUID:"),
                    GetGuid("From node GUID:"),
                    GetGuid("To node GUID:")
                )
                .Result);
        }

        #endregion

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier
    }
}