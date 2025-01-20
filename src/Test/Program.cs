namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Linq;
    using ExpressionTree;
    using GetSomeInput;
    using LiteGraph;
    using LiteGraph.Helpers;

    class Program
    {
        static bool _RunForever = true;
        static bool _Debug = false;
        static LiteGraphClient _Client = new LiteGraphClient();
        static Guid _TenantGuid = Guid.Parse("00000000-0000-0000-0000-000000000000");
        static Guid _GraphGuid = Guid.Parse("00000000-0000-0000-0000-000000000000");

        static void Main(string[] args)
        {
            _Client.Logging.MinimumSeverity = 0;
            _Client.Logging.Logger = Logger;
            _Client.Logging.LogQueries = _Debug;
            _Client.Logging.LogResults = _Debug;

            _Client.InitializeRepository();

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [? for help]:", null, false);

                if (userInput.Equals("?")) Menu();
                else if (userInput.Equals("q")) _RunForever = false;
                else if (userInput.Equals("cls")) Console.Clear();
                else if (userInput.Equals("debug")) ToggleDebug();
                else if (userInput.Equals("tenant")) SetTenant();
                else if (userInput.Equals("graph")) SetGraph();
                else if (userInput.Equals("load1")) LoadGraph1();
                else if (userInput.Equals("load2")) LoadGraph2();
                else if (userInput.Equals("route")) FindRoutes();
                else if (userInput.Equals("test1-1")) Test1_1();
                else if (userInput.Equals("test1-2")) Test1_2();
                else if (userInput.Equals("test1-3")) Test1_3();
                else if (userInput.Equals("test1-4")) Test1_4();
                else if (userInput.Equals("test2-1")) Test2_1();
                else
                {
                    string[] parts = userInput.Split(new char[] { ' ' });

                    if (parts.Length == 2)
                    {
                        if (parts[0].Equals("tenant")
                            || parts[0].Equals("graph")
                            || parts[0].Equals("user")
                            || parts[0].Equals("cred")
                            || parts[0].Equals("label")
                            || parts[0].Equals("tag")
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
            Console.WriteLine("  debug           enable or disable debug (enabled: " + _Debug + ")");
            Console.WriteLine("");
            Console.WriteLine("  tenant          set the tenant GUID (currently " + _TenantGuid + ")");
            Console.WriteLine("  graph           set the graph GUID (currently " + _GraphGuid + ")");
            Console.WriteLine("  load1           load sample graph 1");
            Console.WriteLine("  load2           load sample graph 2");
            Console.WriteLine("  route           find routes between two nodes");
            Console.WriteLine("");
            Console.WriteLine("  test1-1         using sample graph 1, validate retrieval by labels");
            Console.WriteLine("  test1-2         using sample graph 1, validate retrieval by tags");
            Console.WriteLine("  test1-3         using sample graph 1, validate retrieval by labels and tags");
            Console.WriteLine("  test1-4         using sample graph 1, validate retrieval by vectors");
            Console.WriteLine("  test2-1         using sample graph 2, validate node retrieval by properties");
            Console.WriteLine("");
            Console.WriteLine("  [type] [cmd]    execute a command against a given type");
            Console.WriteLine("  where:");
            Console.WriteLine("    [type] : tenant graph node edge user cred");
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

        static void ToggleDebug()
        {
            _Debug = !_Debug;
            _Client.Logging.LogQueries = _Debug;
            _Client.Logging.LogResults = _Debug;
        }

        static void SetTenant()
        {
            _TenantGuid = Inputty.GetGuid("Tenant GUID:", _TenantGuid);
        }

        static void SetGraph()
        {
            _GraphGuid = Inputty.GetGuid("Graph GUID:", _GraphGuid);
        }

        #region Graph-1

        static void LoadGraph1()
        {
            #region Tenant

            TenantMetadata tenant = _Client.CreateTenant(Guid.NewGuid(), "Test tenant");

            #endregion

            #region Labels

            List<string> labelsGraph = new List<string>
            {
                "graph"
            };

            List<string> labelsOdd = new List<string>
            {
                "odd"
            };

            List<string> labelsEven = new List<string>
            {
                "even"
            };

            List<string> labelsNode = new List<string>
            {
                "node"
            };

            List<string> labelsEdge = new List<string>
            {
                "edge"
            };

            List<float> embeddings1 = new List<float> { 0.1f, 0.2f, 0.3f };
            List<float> embeddings2 = new List<float> { 0.05f, -0.25f, 0.45f };
            List<float> embeddings3 = new List<float> { -0.2f, 0.3f, -0.4f };
            List<float> embeddings4 = new List<float> { 0.25f, -0.5f, -0.75f };

            #endregion

            #region Tags

            NameValueCollection tagsGraph = new NameValueCollection();
            tagsGraph.Add("type", "graph");

            NameValueCollection tagsNode = new NameValueCollection();
            tagsNode.Add("type", "node");

            NameValueCollection tagsEdge = new NameValueCollection();
            tagsEdge.Add("type", "edge");

            NameValueCollection tagsEven = new NameValueCollection();
            tagsEven.Add("isEven", "true");

            NameValueCollection tagsOdd = new NameValueCollection();
            tagsOdd.Add("isEven", "false");

            #endregion

            #region Graph

            Guid graphGuid = Guid.NewGuid();

            List<VectorMetadata> graphVectors = new List<VectorMetadata>
            {
                new VectorMetadata
                {
                    TenantGUID = tenant.GUID,
                    GraphGUID = graphGuid,
                    Model = "testmodel",
                    Dimensionality = 3,
                    Content = "testcontent",
                    Vectors = embeddings1                    
                }
            };

            Console.WriteLine("| Creating graph with GUID " + graphGuid);

            Graph graph = _Client.CreateGraph(
                tenant.GUID,
                graphGuid,
                "Sample Graph 1",
                labelsGraph,
                tagsGraph,
                graphVectors,
                new GraphMetadata { Description = "This is my sample graph #2" });

            #endregion

            #region Nodes

            Guid node1Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 1 " + node1Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n1 = _Client.CreateNode(new Node
            {
                GUID = node1Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "1",
                Labels = StringHelpers.Combine(labelsOdd, labelsNode),
                Tags = NvcHelpers.Combine(tagsOdd, tagsNode),
                Vectors = new List<VectorMetadata> 
                { 
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node1Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings2
                    }
                }
            });

            Guid node2Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 2 " + node2Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n2 = _Client.CreateNode(new Node
            {
                GUID = node2Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "2",
                Labels = StringHelpers.Combine(labelsEven, labelsNode),
                Tags = NvcHelpers.Combine(tagsEven, tagsNode),
                Vectors = new List<VectorMetadata>
                {
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node2Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings3
                    }
                }
            });

            Guid node3Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 3 " + node3Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n3 = _Client.CreateNode(new Node
            {
                GUID = node3Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "3",
                Labels = StringHelpers.Combine(labelsOdd, labelsNode),
                Tags = NvcHelpers.Combine(tagsOdd, tagsNode),
                Vectors = new List<VectorMetadata>
                {
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node3Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings4
                    }
                }
            });

            Guid node4Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 4 " + node4Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n4 = _Client.CreateNode(new Node
            {
                GUID = node4Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "4",
                Labels = StringHelpers.Combine(labelsEven, labelsNode),
                Tags = NvcHelpers.Combine(tagsEven, tagsNode),
                Vectors = new List<VectorMetadata>
                {
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node4Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings1
                    }
                }
            });

            Guid node5Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 5 " + node5Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n5 = _Client.CreateNode(new Node
            {
                GUID = node5Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "5",
                Labels = StringHelpers.Combine(labelsOdd, labelsNode),
                Tags = NvcHelpers.Combine(tagsOdd, tagsNode),
                Vectors = new List<VectorMetadata>
                {
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node5Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings2
                    }
                }
            });

            Guid node6Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 6 " + node6Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n6 = _Client.CreateNode(new Node
            {
                GUID = node6Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "6",
                Labels = StringHelpers.Combine(labelsEven, labelsNode),
                Tags = NvcHelpers.Combine(tagsEven, tagsNode),
                Vectors = new List<VectorMetadata>
                {
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node6Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings3
                    }
                }
            });

            Guid node7Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 7 " + node7Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n7 = _Client.CreateNode(new Node
            {
                GUID = node7Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "7",
                Labels = StringHelpers.Combine(labelsOdd, labelsNode),
                Tags = NvcHelpers.Combine(tagsOdd, tagsNode),
                Vectors = new List<VectorMetadata>
                {
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node7Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings4
                    }
                }
            });

            Guid node8Guid = Guid.NewGuid();
            Console.WriteLine("| Creating node 8 " + node8Guid + " in tenant " + tenant.GUID + " graph " + graph.GUID);

            Node n8 = _Client.CreateNode(new Node
            {
                GUID = node8Guid,
                TenantGUID = tenant.GUID,
                GraphGUID = graph.GUID,
                Name = "8",
                Labels = StringHelpers.Combine(labelsEven, labelsNode),
                Tags = NvcHelpers.Combine(tagsEven, tagsNode),
                Vectors = new List<VectorMetadata>
                {
                    new VectorMetadata
                    {
                        TenantGUID = tenant.GUID,
                        GraphGUID = graph.GUID,
                        NodeGUID = node8Guid,
                        Model = "testmodel",
                        Dimensionality = 3,
                        Content = "testcontent",
                        Vectors = embeddings1
                    }
                }
            });

            #endregion

            #region Edges

            Edge e1 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n1,
                n4,
                "1 to 4",
                1,
                StringHelpers.Combine(labelsOdd, labelsEdge),
                NvcHelpers.Combine(tagsOdd, tagsEdge));

            Edge e2 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n1,
                n5,
                "1 to 5",
                2,
                StringHelpers.Combine(labelsEven, labelsEdge),
                NvcHelpers.Combine(tagsEven, tagsEdge));

            Edge e3 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n2,
                n4,
                "2 to 4",
                3,
                StringHelpers.Combine(labelsOdd, labelsEdge),
                NvcHelpers.Combine(tagsOdd, tagsEdge));

            Edge e4 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n2,
                n5,
                "2 to 5",
                4,
                StringHelpers.Combine(labelsEven, labelsEdge),
                NvcHelpers.Combine(tagsEven, tagsEdge));

            Edge e5 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n3,
                n4,
                "3 to 4",
                5,
                StringHelpers.Combine(labelsOdd, labelsEdge),
                NvcHelpers.Combine(tagsOdd, tagsEdge));

            Edge e6 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n3,
                n5,
                "3 to 5",
                6,
                StringHelpers.Combine(labelsEven, labelsEdge),
                NvcHelpers.Combine(tagsEven, tagsEdge));

            Edge e7 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n4,
                n6,
                "4 to 6",
                7,
                StringHelpers.Combine(labelsOdd, labelsEdge),
                NvcHelpers.Combine(tagsOdd, tagsEdge));

            Edge e8 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n4,
                n7,
                "4 to 7",
                8,
                StringHelpers.Combine(labelsEven, labelsEdge),
                NvcHelpers.Combine(tagsEven, tagsEdge));

            Edge e9 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n4,
                n8,
                "4 to 8",
                9,
                StringHelpers.Combine(labelsOdd, labelsEdge),
                NvcHelpers.Combine(tagsOdd, tagsEdge));

            Edge e10 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n5,
                n6,
                "5 to 6",
                10,
                StringHelpers.Combine(labelsEven, labelsEdge),
                NvcHelpers.Combine(tagsEven, tagsEdge));

            Edge e11 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n5,
                n7,
                "5 to 7",
                11,
                StringHelpers.Combine(labelsOdd, labelsEdge),
                NvcHelpers.Combine(tagsOdd, tagsEdge));

            Edge e12 = _Client.CreateEdge(
                tenant.GUID,
                graph.GUID,
                n5,
                n8,
                "5 to 8",
                12,
                StringHelpers.Combine(labelsEven, labelsEdge),
                NvcHelpers.Combine(tagsEven, tagsEdge));

            #endregion
        }

        static void Test1_1()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);

            Console.WriteLine("");
            Console.WriteLine("Retrieving graphs where label = 'graph'");

            List<string> labelGraph = new List<string>
            {
                "graph"
            };

            foreach (Graph graph in _Client.ReadGraphs(tenantGuid, labelGraph))
                Console.WriteLine("| " + graph.GUID + ": " + graph.Name);

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes with labels 'node' and 'even'");

            List<string> labelEvenNodes = new List<string>
            {
                "node",
                "even"
            };

            foreach (Node node in _Client.ReadNodes(tenantGuid, graphGuid, labelEvenNodes))
                Console.WriteLine("| " + node.GUID + ": " + node.Name);

            Console.WriteLine("");
            Console.WriteLine("Retrieving edges with labels 'edge' and 'odd'");

            List<string> labelOddEdges = new List<string>
            {
                "edge",
                "odd"
            };

            foreach (Edge edge in _Client.ReadEdges(tenantGuid, graphGuid, labelOddEdges))
                Console.WriteLine("| " + edge.GUID + ": " + edge.Name);

            Console.WriteLine("");
        }

        static void Test1_2()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);

            Console.WriteLine("");
            Console.WriteLine("Retrieving graphs where tag 'type' = 'graph'");

            NameValueCollection tagsGraph = new NameValueCollection();
            tagsGraph.Add("type", "graph");

            foreach (Graph graph in _Client.ReadGraphs(tenantGuid, null, tagsGraph))
                Console.WriteLine("| " + graph.GUID + ": " + graph.Name);

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes where tag 'type' = 'node' and 'isEven' = 'true'");

            NameValueCollection tagsEvenNodes = new NameValueCollection();
            tagsEvenNodes.Add("type", "node");
            tagsEvenNodes.Add("isEven", "true");

            foreach (Node node in _Client.ReadNodes(tenantGuid, graphGuid, null, tagsEvenNodes))
                Console.WriteLine("| " + node.GUID + ": " + node.Name);

            Console.WriteLine("");
            Console.WriteLine("Retrieving edges where tag 'type' = 'edge' and 'isEven' = 'false'");

            NameValueCollection tagsOddEdges = new NameValueCollection();
            tagsOddEdges.Add("type", "edge");
            tagsOddEdges.Add("isEven", "false");

            foreach (Edge edge in _Client.ReadEdges(tenantGuid, graphGuid, null, tagsOddEdges))
                Console.WriteLine("| " + edge.GUID + ": " + edge.Name);

            Console.WriteLine("");
        }

        static void Test1_3()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);

            Console.WriteLine("");
            Console.WriteLine("Retrieving graphs where label = 'graph', and tag 'type' = 'graph'");

            List<string> labelGraph = new List<string>
            {
                "graph"
            };

            NameValueCollection tagsGraph = new NameValueCollection();
            tagsGraph.Add("type", "graph");

            foreach (Graph graph in _Client.ReadGraphs(tenantGuid, labelGraph, tagsGraph))
                Console.WriteLine("| " + graph.GUID + ": " + graph.Name);

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes where labels 'node' and 'even' are present, and tag 'type' = 'node' and 'isEven' = 'true'");

            List<string> labelEvenNodes = new List<string>
            {
                "node",
                "even"
            };

            NameValueCollection tagsEvenNodes = new NameValueCollection();
            tagsEvenNodes.Add("type", "node");
            tagsEvenNodes.Add("isEven", "true");

            foreach (Node node in _Client.ReadNodes(tenantGuid, graphGuid, labelEvenNodes, tagsEvenNodes))
                Console.WriteLine("| " + node.GUID + ": " + node.Name);

            Console.WriteLine("");
            Console.WriteLine("Retrieving edges where labels 'edge' and 'odd' are present, and tag 'type' = 'edge' and 'isEven' = 'false'");

            List<string> labelOddEdges = new List<string>
            {
                "edge",
                "odd"
            };

            NameValueCollection tagsOddEdges = new NameValueCollection();
            tagsOddEdges.Add("type", "edge");
            tagsOddEdges.Add("isEven", "false");

            foreach (Edge edge in _Client.ReadEdges(tenantGuid, graphGuid, labelOddEdges, tagsOddEdges))
                Console.WriteLine("| " + edge.GUID + ": " + edge.Name);

            Console.WriteLine("");
        }

        static void Test1_4()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);

            #region Cosine-Similarity

            VectorSearchRequest searchReqCosineSim = new VectorSearchRequest
            {
                TenantGUID = _TenantGuid,
                GraphGUID = _GraphGuid,
                Domain = VectorSearchDomainEnum.Node,
                SearchType = VectorSearchTypeEnum.CosineSimilarity,
                Embeddings = new List<float> { 0.1f, 0.2f, 0.3f }
            };

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes by cosine similarity to embeddings [ 0.1, 0.2, 0.3 ]");

            foreach (VectorSearchResult result in _Client.SearchVectors(searchReqCosineSim).OrderByDescending(p => p.Score))
            {
                Console.WriteLine("| Node " + result.Node.GUID + " " + result.Node.Name + ": score " + result.Score);
            }

            #endregion

            #region Cosine-Distance

            VectorSearchRequest searchReqCosineDis = new VectorSearchRequest
            {
                TenantGUID = _TenantGuid,
                GraphGUID = _GraphGuid,
                Domain = VectorSearchDomainEnum.Node,
                SearchType = VectorSearchTypeEnum.CosineDistance,
                Embeddings = new List<float> { 0.1f, 0.2f, 0.3f }
            };

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes by cosine distance from embeddings [ 0.1, 0.2, 0.3 ]");

            foreach (VectorSearchResult result in _Client.SearchVectors(searchReqCosineDis).OrderBy(p => p.Distance))
            {
                Console.WriteLine("| Node " + result.Node.GUID + " " + result.Node.Name + ": distance " + result.Distance);
            }

            #endregion

            #region Euclidian-Similarity

            VectorSearchRequest searchReqEucSim = new VectorSearchRequest
            {
                TenantGUID = _TenantGuid,
                GraphGUID = _GraphGuid,
                Domain = VectorSearchDomainEnum.Node,
                SearchType = VectorSearchTypeEnum.EuclidianSimilarity,
                Embeddings = new List<float> { 0.1f, 0.2f, 0.3f }
            };

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes by Euclidian similarity to embeddings [ 0.1, 0.2, 0.3 ]");

            foreach (VectorSearchResult result in _Client.SearchVectors(searchReqEucSim).OrderByDescending(p => p.Score))
            {
                Console.WriteLine("| Node " + result.Node.GUID + " " + result.Node.Name + ": score " + result.Score);
            }

            #endregion

            #region Euclidian-Distance

            VectorSearchRequest searchReqEucDis = new VectorSearchRequest
            {
                TenantGUID = _TenantGuid,
                GraphGUID = _GraphGuid,
                Domain = VectorSearchDomainEnum.Node,
                SearchType = VectorSearchTypeEnum.EuclidianDistance,
                Embeddings = new List<float> { 0.1f, 0.2f, 0.3f }
            };

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes by Euclidian distance from embeddings [ 0.1, 0.2, 0.3 ]");

            foreach (VectorSearchResult result in _Client.SearchVectors(searchReqEucDis).OrderBy(p => p.Distance))
            {
                Console.WriteLine("| Node " + result.Node.GUID + " " + result.Node.Name + ": distance " + result.Distance);
            }

            #endregion

            #region Inner-Product

            VectorSearchRequest searchReqDp = new VectorSearchRequest
            {
                TenantGUID = _TenantGuid,
                GraphGUID = _GraphGuid,
                Domain = VectorSearchDomainEnum.Node,
                SearchType = VectorSearchTypeEnum.DotProduct,
                Embeddings = new List<float> { 0.1f, 0.2f, 0.3f }
            };

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes by dot product with embeddings [ 0.1, 0.2, 0.3 ]");

            foreach (VectorSearchResult result in _Client.SearchVectors(searchReqDp).OrderByDescending(p => p.InnerProduct))
            {
                Console.WriteLine("| Node " + result.Node.GUID + " " + result.Node.Name + ": inner product " + result.InnerProduct);
            }

            #endregion

            Console.WriteLine("");
        }

        #endregion

        #region Graph-2

        static void LoadGraph2()
        {
            #region Tenant

            TenantMetadata tenant = _Client.CreateTenant(Guid.NewGuid(), "Test tenant");

            #endregion

            #region Graph

            Graph graph = _Client.CreateGraph(
                tenant.GUID, 
                Guid.NewGuid(), 
                "Sample Graph 2", 
                null, 
                null, 
                null,
                new GraphMetadata { Description = "This is my sample graph #2" });

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

            Node joelNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Joel", Data = joel });
            Node yipNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Yip", Data = yip });
            Node keithNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Keith", Data = keith });
            Node alexNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Alex", Data = alex });
            Node blakeNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Blake", Data = blake });

            Node xfiNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Xfinity", Data = xfi });
            Node starlinkNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Starlink", Data = starlink });
            Node attNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "AT&T", Data = att });

            Node internetNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Internet", Data = internet });

            Node equinixNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Equinix", Data = equinix });
            Node awsNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "AWS", Data = aws });
            Node azureNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Azure", Data = azure });
            Node digitalOceanNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "DigitalOcean", Data = digitalOcean });
            Node rackspaceNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Rackspace", Data = rackspace });

            Node ccpNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Control Plane", Data = ccp });
            Node websiteNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Website", Data = website });
            Node adNode = _Client.CreateNode(new Node { TenantGUID = tenant.GUID, GraphGUID = graph.GUID, Name = "Active Directory", Data = ad });

            #endregion

            #region Edges

            Edge joelXfiEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, joelNode, xfiNode, "Joel to Xfinity");
            Edge joelStarlinkEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, joelNode, starlinkNode, "Joel to Starlink");
            Edge yipXfiEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, yipNode, xfiNode, "Yip to Xfinity");
            Edge keithStarlinkEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, keithNode, starlinkNode, "Keith to Starlink");
            Edge keithXfiEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, keithNode, xfiNode, "Keith to Xfinity");
            Edge keithAttEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, keithNode, attNode, "Keith to AT&T");
            Edge alexAttEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, alexNode, attNode, "Alex to AT&T");
            Edge blakeAttEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, blakeNode, attNode, "Blake to AT&T");

            Edge xfiInternetEdge1 = _Client.CreateEdge(tenant.GUID, graph.GUID, xfiNode, internetNode, "Xfinity to Internet 1");
            Edge xfiInternetEdge2 = _Client.CreateEdge(tenant.GUID, graph.GUID, xfiNode, internetNode, "Xfinity to Internet 2");
            Edge starlinkInternetEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, starlinkNode, internetNode, "Starlink to Internet");
            Edge attInternetEdge1 = _Client.CreateEdge(tenant.GUID, graph.GUID, attNode, internetNode, "AT&T to Internet 1");
            Edge attInternetEdge2 = _Client.CreateEdge(tenant.GUID, graph.GUID, attNode, internetNode, "AT&T to Internet 2");

            Edge internetEquinixEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, internetNode, equinixNode, "Internet to Equinix");
            Edge internetAwsEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, internetNode, awsNode, "Internet to AWS");
            Edge internetAzureEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, internetNode, azureNode, "Internet to Azure");

            Edge equinixDoEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, equinixNode, digitalOceanNode, "Equinix to DigitalOcean");
            Edge equinixAwsEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, equinixNode, awsNode, "Equinix to AWS");
            Edge equinixRackspaceEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, equinixNode, rackspaceNode, "Equinix to Rackspace");

            Edge awsWebsiteEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, awsNode, websiteNode, "AWS to Website");

            Edge azureAdEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, azureNode, adNode, "Azure to Active Directory");

            Edge doCcpEdge = _Client.CreateEdge(tenant.GUID, graph.GUID, digitalOceanNode, ccpNode, "DigitalOcean to Control Plane");

            #endregion
        }

        static void Test2_1()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Expr e1 = new Expr("$.Name", OperatorEnum.Equals, "Joel");
            Expr e2 = new Expr("$.Age", OperatorEnum.GreaterThan, 38);

            Console.WriteLine("");
            Console.WriteLine("Retrieving nodes where Name = 'Joel'");
            foreach (Node node in _Client.ReadNodes(tenantGuid, graphGuid, null, null, e1))
            {
                // Console.WriteLine(node.Data.ToString());
                Console.WriteLine(_Client.ConvertData<Person>(node.Data).ToString());
            }

            Console.WriteLine("");
            Console.WriteLine("Retrieve nodes where Age >= 38");
            foreach (Node node in _Client.ReadNodes(tenantGuid, graphGuid, null, null, e2))
            {
                // Console.WriteLine(node.Data.ToString());
                Console.WriteLine(_Client.ConvertData<Person>(node.Data).ToString());
            }

            Console.WriteLine("");
        }

        #endregion

        #region Primitives

        static void FindRoutes()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Guid fromGuid = Inputty.GetGuid("From GUID  :", default(Guid));
            Guid toGuid = Inputty.GetGuid("To GUID    :", default(Guid));
            object obj = _Client.GetRoutes(SearchTypeEnum.DepthFirstSearch, tenantGuid, graphGuid, fromGuid, toGuid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void Create(string str)
        {
            object obj = null;
            string json = null;

            if (str.Equals("tenant"))
            {
                obj = _Client.CreateTenant(Guid.NewGuid(), Inputty.GetString("Name:", null, false));
            }
            else if (str.Equals("user"))
            {
                obj = _Client.CreateUser(
                    Inputty.GetGuid("Tenant GUID:", _TenantGuid),
                    Guid.NewGuid(),
                    Inputty.GetString("First name:", null, false),
                    Inputty.GetString("Last name:", null, false),
                    Inputty.GetString("Email:", null, false),
                    Inputty.GetString("Password:", null, false));
            }
            else if (str.Equals("cred"))
            {
                obj = _Client.CreateCredential(
                    Inputty.GetGuid("Tenant GUID:", _TenantGuid),
                    Inputty.GetGuid("User GUID:", default(Guid)),
                    Guid.NewGuid(),
                    Inputty.GetString("First name:", null, false));
            }
            else if (str.Equals("graph"))
            {
                obj = _Client.CreateGraph(
                    Inputty.GetGuid("Tenant GUID:", _TenantGuid),
                    Guid.NewGuid(),
                    Inputty.GetString("Name:", null, false));
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
            if (str.Equals("tenant"))
            {
                obj = _Client.ReadTenants();
            }
            else if (str.Equals("user"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                obj = _Client.ReadUsers(tenantGuid);
            }
            else if (str.Equals("cred"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                obj = _Client.ReadCredentials(tenantGuid);
            }
            else if (str.Equals("graph"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                obj = _Client.ReadGraphs(tenantGuid);
            }
            else if (str.Equals("node"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                obj = _Client.ReadNodes(tenantGuid, graphGuid);
            }
            else if (str.Equals("edge"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                obj = _Client.ReadEdges(tenantGuid, graphGuid);
            }

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void Read(string str)
        {
            object obj = null;

            if (str.Equals("tenant"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                obj = _Client.ReadTenant(tenantGuid);
            }
            else if (str.Equals("user"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid userGuid =   Inputty.GetGuid("GUID        :", default(Guid));
                obj = _Client.ReadUser(tenantGuid, userGuid);
            }
            else if (str.Equals("cred"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid credGuid = Inputty.GetGuid("GUID        :", default(Guid));
                obj = _Client.ReadCredential(tenantGuid, credGuid);
            }
            else if (str.Equals("graph"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                obj = _Client.ReadGraph(tenantGuid, graphGuid);
            }
            else if (str.Equals("node"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
                obj = _Client.ReadNode(tenantGuid, graphGuid, guid);
            }
            else if (str.Equals("edge"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                Guid guid = Inputty.GetGuid("Edge GUID   :", default(Guid));
                obj = _Client.ReadEdge(tenantGuid, graphGuid, guid);
            }

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void Exists(string str)
        {
            bool exists = false;

            if (str.Equals("tenant"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                exists = _Client.ExistsTenant(tenantGuid);
            }
            else if (str.Equals("user"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid userGuid = Inputty.GetGuid("GUID        :", default(Guid));
                exists = _Client.ExistsUser(tenantGuid, userGuid);
            }
            else if (str.Equals("cred"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid credGuid = Inputty.GetGuid("GUID        :", default(Guid));
                exists = _Client.ExistsCredential(tenantGuid, credGuid);
            }
            else if (str.Equals("graph"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                exists = _Client.ExistsGraph(tenantGuid, graphGuid);
            }
            else if (str.Equals("node"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
                exists = _Client.ExistsNode(tenantGuid, graphGuid, guid);
            }
            else if (str.Equals("edge"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                Guid guid = Inputty.GetGuid("Edge GUID   :", default(Guid));
                exists = _Client.ExistsEdge(tenantGuid, graphGuid, guid);
            }

            Console.WriteLine("Exists: " + exists);
        }

        static void Update(string str)
        {
            object obj = null;
            string json = Inputty.GetString("JSON:", null, false);

            if (str.Equals("graph"))
            {
                obj = _Client.UpdateTenant(Serializer.DeserializeJson<TenantMetadata>(json));
            }
            else if (str.Equals("graph"))
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
            if (str.Equals("tenant"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                bool force = Inputty.GetBoolean("Force       :", true);
                _Client.DeleteTenant(tenantGuid, force);
            }
            else if (str.Equals("user"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid userGuid = Inputty.GetGuid("GUID        :", default(Guid));
                _Client.DeleteUser(tenantGuid, userGuid);
            }
            else if (str.Equals("cred"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid credGuid = Inputty.GetGuid("GUID        :", default(Guid));
                _Client.DeleteCredential(tenantGuid, credGuid);
            }
            else if (str.Equals("graph"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                bool force = Inputty.GetBoolean("Force       :", true);
                _Client.DeleteGraph(tenantGuid, graphGuid, force);
            }
            else if (str.Equals("node"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
                _Client.DeleteNode(tenantGuid, graphGuid, guid);
            }
            else if (str.Equals("edge"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                Guid guid = Inputty.GetGuid("Edge GUID   :", default(Guid));
                _Client.DeleteEdge(tenantGuid, graphGuid, guid);
            }
        }

        static void Search(string str)
        {
            if (!str.Equals("graph") && !str.Equals("node") && !str.Equals("edge")) return;

            Expr expr = GetExpression();
            string resultJson = null;

            if (str.Equals("graph"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                IEnumerable<Graph> graphResult = _Client.ReadGraphs(tenantGuid, null, null, expr, EnumerationOrderEnum.CreatedDescending);
                if (graphResult != null) resultJson = Serializer.SerializeJson(graphResult.ToList());
            }
            else if (str.Equals("node"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                IEnumerable<Node> nodeResult = _Client.ReadNodes(tenantGuid, graphGuid, null, null, expr, EnumerationOrderEnum.CreatedDescending);
                if (nodeResult != null) resultJson = Serializer.SerializeJson(nodeResult.ToList());
            }
            else if (str.Equals("edge"))
            {
                Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
                Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
                IEnumerable<Edge> edgeResult = _Client.ReadEdges(tenantGuid, graphGuid, null, null, expr, EnumerationOrderEnum.CreatedDescending);
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
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
            object obj = _Client.GetEdgesTo(tenantGuid, graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeEdgesFrom()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
            object obj = _Client.GetEdgesFrom(tenantGuid, graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeEdgesBetween()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Guid fromGuid = Inputty.GetGuid("From GUID   :", default(Guid));
            Guid toGuid = Inputty.GetGuid("To GUID     :", default(Guid));
            object obj = _Client.GetEdgesBetween(tenantGuid, graphGuid, fromGuid, toGuid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeParents()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
            object obj = _Client.GetParents(tenantGuid, graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeChildren()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
            object obj = _Client.GetChildren(tenantGuid, graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        static void NodeNeighbors()
        {
            Guid tenantGuid = Inputty.GetGuid("Tenant GUID :", _TenantGuid);
            Guid graphGuid = Inputty.GetGuid("Graph GUID  :", _GraphGuid);
            Guid guid = Inputty.GetGuid("Node GUID   :", default(Guid));
            object obj = _Client.GetNeighbors(tenantGuid, graphGuid, guid);

            if (obj != null)
                Console.WriteLine(Serializer.SerializeJson(obj, true));
        }

        #endregion
    }
}