﻿namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Linq;
    using ExpressionTree;
    using GetSomeInput;
    using LiteGraph;

    class Program
    {
        static bool _RunForever = true;
        static LiteGraphClient _Client = new LiteGraphClient();
        static Guid _GraphGuid = default(Guid);

        static void Main(string[] args)
        {
            _Client.Logging.MinimumSeverity = 0;
            _Client.Logging.Logger = Logger;
            _Client.Logging.LogQueries = true;
            _Client.Logging.LogResults = true;

            _Client.InitializeRepository();

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [? for help]:", null, false);

                if (userInput.Equals("?")) Menu();
                else if (userInput.Equals("q")) _RunForever = false;
                else if (userInput.Equals("cls")) Console.Clear();
                else if (userInput.Equals("load1")) LoadGraph1();
                else if (userInput.Equals("load2")) LoadGraph2();
                else if (userInput.Equals("route")) FindRoutes();
                else if (userInput.Equals("test2")) Test2();
                else
                {
                    string[] parts = userInput.Split(new char[] { ' ' });

                    if (parts.Length == 2)
                    {
                        if (parts[0].Equals("graph")
                            || parts[0].Equals("node")
                            || parts[0].Equals("edge"))
                        {
                            if (parts[1].Equals("create")) Create(parts[0]);
                            else if (parts[1].Equals("all")) All(parts[0]);
                            else if (parts[1].Equals("read")) Read(parts[0]);
                            else if (parts[1].Equals("exists")) Exists(parts[0]);
                            else if (parts[1].Equals("update")) Update(parts[0]);
                            else if (parts[1].Equals("delete")) Delete(parts[0]);
                            else if (parts[1].Equals("search")) Search(parts[0]);

                            if (parts[0].Equals("node"))
                            {
                                if (parts[1].Equals("edgesto")) NodeEdgesTo();
                                else if (parts[1].Equals("edgesfrom")) NodeEdgesFrom();
                                else if (parts[1].Equals("edgesbetween")) NodeEdgesBetween();
                                else if (parts[1].Equals("parents")) NodeParents();
                                else if (parts[1].Equals("children")) NodeChildren();
                                else if (parts[1].Equals("neighbors")) NodeNeighbors();
                            }
                        }
                    }
                }
            }
        }

        static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  ?               help, this menu");
            Console.WriteLine("  q               quit");
            Console.WriteLine("  cls             clear the screen");
            Console.WriteLine("");
            Console.WriteLine("  load1           load sample graph 1");
            Console.WriteLine("  load2           load sample graph 2");
            Console.WriteLine("  route           find routes between two nodes");
            Console.WriteLine("  test2           validate node retrieval by properties");
            Console.WriteLine("");
            Console.WriteLine("  [type] [cmd]    execute a command against a given type");
            Console.WriteLine("  where:");
            Console.WriteLine("    [type] : graph node edge");
            Console.WriteLine("    [cmd]  : create all read exists update delete search");
            Console.WriteLine("");
            Console.WriteLine("  For node operations, additional commands are available");
            Console.WriteLine("    edgesto    edgesfrom   edgesbetween");
            Console.WriteLine("    parents    children    neighbors");
            Console.WriteLine("");
        }

        static void Logger(SeverityEnum sev, string msg)
        {
            if (!String.IsNullOrEmpty(msg))
            {
                Console.WriteLine(sev.ToString() + " " + msg);
            }
        }

        static void Test2()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
            Expr e1 = new Expr("$.Name", OperatorEnum.Equals, "Joel");
            Expr e2 = new Expr("$.Age", OperatorEnum.GreaterThan, 38);

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes where Name = 'Joel'");
            foreach (Node node in _Client.ReadNodes(graphGuid, e1))
            {
                // Console.WriteLine(node.Data.ToString());
                Console.WriteLine(_Client.ConvertData<Person>(node.Data).ToString());
            }

            Console.WriteLine("");
            Console.WriteLine("Retrieve nodes where Age >= 38"); 
            foreach (Node node in _Client.ReadNodes(graphGuid, e2))
            {
                // Console.WriteLine(node.Data.ToString());
                Console.WriteLine(_Client.ConvertData<Person>(node.Data).ToString());
            }

            Console.WriteLine("");
        }

        static void LoadGraph1()
        {
            #region Graph

            Graph graph = _Client.CreateGraph(Guid.NewGuid(), "Sample Graph 1", new GraphMetadata { Description = "This is my sample graph #2" });

            #endregion

            #region Nodes

            Node n1 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "1" });
            Node n2 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "2" });
            Node n3 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "3" });
            Node n4 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "4" });
            Node n5 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "5" });
            Node n6 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "6" });
            Node n7 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "7" });
            Node n8 = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "8" });

            #endregion

            #region Edges

            Edge e1 = _Client.CreateEdge(graph.GUID, n1, n4, "1 to 4");
            Edge e2 = _Client.CreateEdge(graph.GUID, n1, n5, "1 to 5");
            Edge e3 = _Client.CreateEdge(graph.GUID, n2, n4, "2 to 4");
            Edge e4 = _Client.CreateEdge(graph.GUID, n2, n5, "2 to 5");
            Edge e5 = _Client.CreateEdge(graph.GUID, n3, n4, "3 to 4");
            Edge e6 = _Client.CreateEdge(graph.GUID, n3, n5, "3 to 5");
            Edge e7 = _Client.CreateEdge(graph.GUID, n4, n6, "4 to 6");
            Edge e8 = _Client.CreateEdge(graph.GUID, n4, n7, "4 to 7");
            Edge e9 = _Client.CreateEdge(graph.GUID, n4, n8, "4 to 8");
            Edge e10 = _Client.CreateEdge(graph.GUID, n5, n6, "5 to 6");
            Edge e11 = _Client.CreateEdge(graph.GUID, n5, n7, "5 to 7");
            Edge e12 = _Client.CreateEdge(graph.GUID, n5, n8, "5 to 8");

            #endregion
        }

        static void LoadGraph2()
        {
            #region Graph

            Graph graph = _Client.CreateGraph(Guid.NewGuid(), "Sample Graph 2", new GraphMetadata { Description = "This is my sample graph #2" });

            #endregion

            #region Objects

            Person joel = new Person { Name = "Joel", Age = 47, Hobby = new Hobby { Name = "BJJ", HoursPerWeek = 8 } };
            Person yip = new Person { Name = "Yip", Age = 39, Hobby = new Hobby { Name = "Law", HoursPerWeek = 40 } };
            Person keith = new Person { Name = "Keith", Age = 48, Hobby = new Hobby { Name = "Planes", HoursPerWeek = 10 } };
            Person alex = new Person { Name = "Alex", Age = 34, Hobby = new Hobby { Name = "Art", HoursPerWeek = 10 } };
            Person blake = new Person { Name = "Blake", Age = 34, Hobby = new Hobby { Name = "Music", HoursPerWeek = 20 } };

            ISP xfi = new ISP { Name = "Xfinity", Mbps = 1000 };
            ISP starlink = new ISP { Name = "Starlink", Mbps = 100 };
            ISP att = new ISP { Name = "AT&T", Mbps = 500 };

            Internet internet = new Internet();

            HostingProvider equinix = new HostingProvider { Name = "Equinix" };
            HostingProvider aws = new HostingProvider { Name = "Amazon Web Services" };
            HostingProvider azure = new HostingProvider { Name = "Microsoft Azure" };
            HostingProvider digitalOcean = new HostingProvider { Name = "DigitalOcean" };
            HostingProvider rackspace = new HostingProvider { Name = "Rackspace" };

            Application ccp = new Application { Name = "Cloud Control Plane" };
            Application website = new Application { Name = "Website" };
            Application ad = new Application { Name = "Active Directory" };

            #endregion

            #region Nodes

            Node joelNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Joel", Data = joel });
            Node yipNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Yip", Data = yip });
            Node keithNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Keith", Data = keith });
            Node alexNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Alex", Data = alex });
            Node blakeNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Blake", Data = blake });

            Node xfiNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Xfinity", Data = xfi });
            Node starlinkNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Starlink", Data = starlink });
            Node attNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "AT&T", Data = att });

            Node internetNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Internet", Data = internet });

            Node equinixNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Equinix", Data = equinix });
            Node awsNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "AWS", Data = aws });
            Node azureNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Azure", Data = azure });
            Node digitalOceanNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "DigitalOcean", Data = digitalOcean });
            Node rackspaceNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Rackspace", Data = rackspace });

            Node ccpNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Control Plane", Data = ccp });
            Node websiteNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Website", Data = website });
            Node adNode = _Client.CreateNode(new Node { GraphGUID = graph.GUID, Name = "Active Directory", Data = ad });

            #endregion

            #region Edges

            Edge joelXfiEdge = _Client.CreateEdge(graph.GUID, joelNode, xfiNode, "Joel to Xfinity");
            Edge joelStarlinkEdge = _Client.CreateEdge(graph.GUID, joelNode, starlinkNode, "Joel to Starlink");
            Edge yipXfiEdge = _Client.CreateEdge(graph.GUID, yipNode, xfiNode, "Yip to Xfinity");
            Edge keithStarlinkEdge = _Client.CreateEdge(graph.GUID, keithNode, starlinkNode, "Keith to Starlink");
            Edge keithXfiEdge = _Client.CreateEdge(graph.GUID, keithNode, xfiNode, "Keith to Xfinity");
            Edge keithAttEdge = _Client.CreateEdge(graph.GUID, keithNode, attNode, "Keith to AT&T");
            Edge alexAttEdge = _Client.CreateEdge(graph.GUID, alexNode, attNode, "Alex to AT&T");
            Edge blakeAttEdge = _Client.CreateEdge(graph.GUID, blakeNode, attNode, "Blake to AT&T");

            Edge xfiInternetEdge1 = _Client.CreateEdge(graph.GUID, xfiNode, internetNode, "Xfinity to Internet 1");
            Edge xfiInternetEdge2 = _Client.CreateEdge(graph.GUID, xfiNode, internetNode, "Xfinity to Internet 2");
            Edge starlinkInternetEdge = _Client.CreateEdge(graph.GUID, starlinkNode, internetNode, "Starlink to Internet");
            Edge attInternetEdge1 = _Client.CreateEdge(graph.GUID, attNode, internetNode, "AT&T to Internet 1");
            Edge attInternetEdge2 = _Client.CreateEdge(graph.GUID, attNode, internetNode, "AT&T to Internet 2");

            Edge internetEquinixEdge = _Client.CreateEdge(graph.GUID, internetNode, equinixNode, "Internet to Equinix");
            Edge internetAwsEdge = _Client.CreateEdge(graph.GUID, internetNode, awsNode, "Internet to AWS");
            Edge internetAzureEdge = _Client.CreateEdge(graph.GUID, internetNode, azureNode, "Internet to Azure");

            Edge equinixDoEdge = _Client.CreateEdge(graph.GUID, equinixNode, digitalOceanNode, "Equinix to DigitalOcean");
            Edge equinixAwsEdge = _Client.CreateEdge(graph.GUID, equinixNode, awsNode, "Equinix to AWS");
            Edge equinixRackspaceEdge = _Client.CreateEdge(graph.GUID, equinixNode, rackspaceNode, "Equinix to Rackspace");

            Edge awsWebsiteEdge = _Client.CreateEdge(graph.GUID, awsNode, websiteNode, "AWS to Website");

            Edge azureAdEdge = _Client.CreateEdge(graph.GUID, azureNode, adNode, "Azure to Active Directory");

            Edge doCcpEdge = _Client.CreateEdge(graph.GUID, digitalOceanNode, ccpNode, "DigitalOcean to Control Plane");

            #endregion
        }

        static void FindRoutes()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID :", _GraphGuid);
            Guid fromGuid  = Inputty.GetGuid("From GUID  :", default(Guid));
            Guid toGuid    = Inputty.GetGuid("To GUID    :", default(Guid));
            object obj = _Client.GetRoutes(SearchTypeEnum.DepthFirstSearch, graphGuid, fromGuid, toGuid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void Create(string str)
        {
            object obj = null;
            string json = null;

            if (str.Equals("graph"))
            {
                obj = _Client.CreateGraph(Guid.NewGuid(), Inputty.GetString("Name:", null, false));
            }
            else if (str.Equals("node"))
            {
                json = Inputty.GetString("JSON:", null, false);
                obj = _Client.CreateNode(Serializer.DeserializeJson<Node>(json));
            }
            else if (str.Equals("edge"))
            {
                json = Inputty.GetString("JSON:", null, false);
                obj = _Client.CreateEdge(Serializer.DeserializeJson<Edge>(json));
            }

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void All(string str)
        {
            object obj = null;
            if (str.Equals("graph")) obj = _Client.ReadGraphs();
            else if (str.Equals("node"))
            {
                Guid guid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
                obj = _Client.ReadNodes(guid);
            }
            else if (str.Equals("edge"))
            {
                Guid guid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
                obj = _Client.ReadEdges(guid);
            }

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void Read(string str)
        {
            object obj = null;
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);

            if (str.Equals("graph")) obj = _Client.ReadGraph(graphGuid);
            else if (str.Equals("node"))
            {
                Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
                obj = _Client.ReadNode(graphGuid, guid);
            }
            else if (str.Equals("edge"))
            {
                Guid guid = Inputty.GetGuid("Edge GUID :", default(Guid));
                obj = _Client.ReadEdge(graphGuid, guid);
            }

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void Exists(string str)
        {
            bool exists = false;
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);

            if (str.Equals("graph")) exists = _Client.ExistsGraph(graphGuid);
            else if (str.Equals("node"))
            {
                Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
                exists = _Client.ExistsNode(graphGuid, guid);
            }
            else if (str.Equals("edge"))
            {
                Guid guid = Inputty.GetGuid("Edge GUID :", default(Guid));
                exists = _Client.ExistsEdge(graphGuid, guid);
            }

            Console.WriteLine("Exists: " + exists);
        }

        static void Update(string str)
        {
            object obj = null;
            string json = Inputty.GetString("JSON:", null, false);

            if (str.Equals("graph"))
            {
                obj = _Client.UpdateGraph(Serializer.DeserializeJson<Graph>(json));
            }
            else if (str.Equals("node"))
            {
                obj = _Client.CreateNode(Serializer.DeserializeJson<Node>(json));
            }
            else if (str.Equals("edge"))
            {
                obj = _Client.CreateEdge(Serializer.DeserializeJson<Edge>(json));
            }

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void Delete(string str)
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);

            if (str.Equals("graph"))
            {
                bool force = Inputty.GetBoolean("Force     :", true);
                _Client.DeleteGraph(graphGuid, force);
            }
            else if (str.Equals("node"))
            {
                Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
                _Client.DeleteNode(graphGuid, guid);
            }
            else if (str.Equals("edge"))
            {
                Guid guid = Inputty.GetGuid("Edge GUID :", default(Guid));
                _Client.DeleteEdge(graphGuid, guid);
            }
        }

        static void Search(string str)
        {
            if (!str.Equals("graph") && !str.Equals("node") && !str.Equals("edge")) return;

            Expr expr = GetExpression();
            string resultJson = null;

            if (str.Equals("graph"))
            {
                IEnumerable<Graph> graphResult = _Client.ReadGraphs(expr, EnumerationOrderEnum.CreatedDescending);
                if (graphResult != null) resultJson = Serializer.SerializeJson(graphResult.ToList());
            }
            else if (str.Equals("node"))
            {
                Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
                IEnumerable<Node> nodeResult = _Client.ReadNodes(graphGuid, expr, EnumerationOrderEnum.CreatedDescending);
                if (nodeResult != null) resultJson = Serializer.SerializeJson(nodeResult.ToList());
            }
            else if (str.Equals("edge"))
            {
                Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
                IEnumerable<Edge> edgeResult = _Client.ReadEdges(graphGuid, expr, EnumerationOrderEnum.CreatedDescending);
                if (edgeResult != null) resultJson = Serializer.SerializeJson(edgeResult.ToList());
            }

            Console.WriteLine("");
            if (!String.IsNullOrEmpty(resultJson)) Console.WriteLine(resultJson);
            else Console.WriteLine("(null)");
            Console.WriteLine("");
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

        static void NodeEdgesTo()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
            object obj = _Client.GetEdgesTo(graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeEdgesFrom()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
            object obj = _Client.GetEdgesFrom(graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeEdgesBetween()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
            Guid fromGuid = Inputty.GetGuid ("From GUID :", default(Guid));
            Guid toGuid = Inputty.GetGuid   ("To GUID   :", default(Guid));
            object obj = _Client.GetEdgesBetween(graphGuid, fromGuid, toGuid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeParents()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
            object obj = _Client.GetParents(graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeChildren()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
            object obj = _Client.GetChildren(graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeNeighbors()
        {
            Guid graphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID :", default(Guid));
            object obj = _Client.GetNeighbors(graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }
    }
}