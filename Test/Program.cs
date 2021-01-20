using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using LiteGraph;

namespace Test
{
    class Program
    {
        static bool _RunForever = true;
        static LiteGraphClient _Graph = null;
        static Formatting _JsonFormatting = Formatting.Indented;

        static void Main(string[] args)
        {
            _Graph = new LiteGraphClient(InputString("Filename:", "litegraph.db", false));
            _Graph.Logger.LogMethod = Console.WriteLine;

            while (_RunForever)
            {
                string userInput = InputString("Command [? for help]:", null, false);

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
                    case "event on":
                        EventsOn();
                        break;
                    case "event off":
                        EventsOff();
                        break;
                    case "default1":
                        LoadDefault1Graph();
                        break;
                    case "default2":
                        LoadDefault2Graph();
                        break;
                    case "debug queries":
                        _Graph.Logger.LogQueries = !_Graph.Logger.LogQueries;
                        break;
                    case "debug results":
                        _Graph.Logger.LogResults = !_Graph.Logger.LogResults;
                        break;
                    case "formatting":
                        if (_JsonFormatting == Formatting.Indented) _JsonFormatting = Formatting.None;
                        else _JsonFormatting = Formatting.Indented;
                        break;
                    case "add node":
                        AddNode();
                        break;
                    case "all nodes":
                        AllNodes();
                        break;
                    case "get node":
                        GetNode();
                        break;
                    case "get desc":
                        GetDescendants();
                        break;
                    case "update node":
                        UpdateNode();
                        break;
                    case "add edge":
                        AddEdge();
                        break;
                    case "all edges":
                        AllEdges();
                        break;
                    case "node edges":
                        NodeEdges();
                        break;
                    case "neighbors":
                        Neighbors();
                        break;
                    case "get edge":
                        GetEdge();
                        break;
                    case "update edge":
                        UpdateEdge();
                        break;
                    case "search nodes":
                        SearchNodes();
                        break;
                    case "search edges":
                        SearchEdges();
                        break;
                    case "route":
                        FindRoutes();
                        break;
                    case "export":
                        ExportToGexf();
                        break;
                }
            }
        }

        static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        static List<SearchFilter> InputSearchFilter()
        {
            Console.WriteLine("Press ENTER on field with empty value to exit");
            List<SearchFilter> ret = new List<SearchFilter>();
            while (true)
            {
                string field = InputString("Field:", null, true);
                if (String.IsNullOrEmpty(field)) break;
                SearchCondition condition = (SearchCondition)(Enum.Parse(typeof(SearchCondition), InputString("Condition:", "Equals", false)));
                string value = InputString("Value:", null, true);
                SearchFilter sf = new SearchFilter(field, condition, value);
                ret.Add(sf);
            }
            return ret;
        }

        static List<string> InputStringList(string question, bool allowEmpty)
        {
            List<string> ret = new List<string>();

            while (true)
            {
                Console.Write(question);

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (ret.Count < 1 && !allowEmpty) continue;
                    return ret;
                }

                ret.Add(userInput);
            }
        }

        static void Enumerate(GraphResult r)
        {
            Console.WriteLine(Serialize(r));
        }

        static string Serialize(object o)
        {
            if (o == null) return null;
            return JsonConvert.SerializeObject(
                o,
                _JsonFormatting,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );
        }

        static string Serialize(object o, bool pretty = true)
        {
            if (o == null) return null;
            if (pretty)
            {
                return JsonConvert.SerializeObject(
                   o,
                   Formatting.Indented,
                   new JsonSerializerSettings
                   {
                       NullValueHandling = NullValueHandling.Ignore
                   }
               );
            }
            else
            {
                return JsonConvert.SerializeObject(
                    o,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }
                );
            }
        }

        static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  ?               help, this menu");
            Console.WriteLine("  q               quit");
            Console.WriteLine("  cls             clear the screen");
            Console.WriteLine("  default1        load default1 nodes and edges (see documentation)");
            Console.WriteLine("  default2        load default2 nodes and edges (see documentation)");
            Console.WriteLine("  event on        enable event messages");
            Console.WriteLine("  event off       disable event messages");
            Console.WriteLine("  debug queries   toggle query debug, currently: " + _Graph.Logger.LogQueries);
            Console.WriteLine("  debug results   toggle results debug, currently: " + _Graph.Logger.LogResults);
            Console.WriteLine("  formatting      toggle JSON formatting, currently: " + _JsonFormatting.ToString());
            Console.WriteLine("  all nodes       retrieve all nodes from the graph");
            Console.WriteLine("  add node        add a node to the graph");
            Console.WriteLine("  get node        retrieve a node by GUID");
            Console.WriteLine("  get desc        retrieve a node's descendants");
            Console.WriteLine("  update node     update a node");
            Console.WriteLine("  all edges       retrieve all edges from the graph");
            Console.WriteLine("  add edge        add an edge between two nodes");
            Console.WriteLine("  node edges      retrieve all edges to or from a given node");
            Console.WriteLine("  neighbors       retrieve a node's neighbors");
            Console.WriteLine("  get edge        retrieve an edge by GUID");
            Console.WriteLine("  update edge     update an edge");
            Console.WriteLine("  search nodes    search nodes using supplied filters");
            Console.WriteLine("  search edges    search edges using supplied filters");
            Console.WriteLine("  route           find routes between two nodes");
            Console.WriteLine("  export          export to a GEXF file");
            Console.WriteLine("");
        }

        static void EventsOn()
        {
            _Graph.Events.NodeAdded += NodeAdded;
            _Graph.Events.NodeUpdated += NodeUpdated;
            _Graph.Events.NodeRemoved += NodeRemoved;
            _Graph.Events.EdgeAdded += EdgeAdded;
            _Graph.Events.EdgeUpdated += EdgeUpdated;
            _Graph.Events.EdgeRemoved += EdgeRemoved;
        }

        static void EventsOff()
        {
            _Graph.Events = null;
        }

        static void LoadDefault1Graph()
        {
            // People
            _Graph.AddNode("{'guid':'joel','name':'Joel','type':'person','first':'Joel','city':'San Jose'}");
            _Graph.AddNode("{'guid':'maria','name':'Maria','type':'person','first':'Maria','city':'San Jose'}");
            _Graph.AddNode("{'guid':'jason','name':'Jason','type':'person','first':'Jason','city':'San Jose'}");
            _Graph.AddNode("{'guid':'scott','name':'Scott','type':'person','first':'Scott','city':'Chicago'}");
            _Graph.AddNode("{'guid':'may','name':'May','type':'person','first':'May','city':'New York City'}");
            _Graph.AddNode("{'guid':'matt','name':'Matt','type':'person','first':'Matt','city':'Raleigh'}");
            _Graph.AddNode("{'guid':'bob','name':'Bob','type':'person','first':'Bob','city':'Asheville'}");

            // Things
            _Graph.AddNode("{'guid':'car1','name':'Highlander','type':'car','make':'Toyota','model':'Highlander'}");
            _Graph.AddNode("{'guid':'car2','name':'Jetta','type':'car','make':'Volkswagen','model':'Jetta'}");
            _Graph.AddNode("{'guid':'car3','name':'SUV','type':'car','make':'Mercedes','model':'SUV'}");
            _Graph.AddNode("{'guid':'guitar','name':'Soloist','type':'instrument','make':'Jackson','model':'Soloist'}");
            _Graph.AddNode("{'guid':'piano','name':'Keyboard','type':'instrument','make':'Yamaha','model':'Keyboard'}");
            _Graph.AddNode("{'guid':'house','name':'House','type':'house','desc':'Super duper house'}");

            // Relationships
            _Graph.AddEdge("joel", "house", "{'guid':'r1','type':'lives_in','data':'foo'}");
            _Graph.AddEdge("joel", "car1", "{'guid':'r2','type':'drives','data':'bar'}");
            _Graph.AddEdge("joel", "guitar", "{'guid':'r3','type':'plays','data':'baz'}");
            _Graph.AddEdge("maria", "house", "{'guid':'r4','type':'lives_in','data':'foo'}");
            _Graph.AddEdge("maria", "car2", "{'guid':'r5','type':'drives','data':'bar'}");
            _Graph.AddEdge("jason", "house", "{'guid':'r6','type':'lives_in','data':'baz'}");
            _Graph.AddEdge("joel", "scott", "{'guid':'r7','type':'friends_with','data':'foo'}");
            _Graph.AddEdge("maria", "may", "{'guid':'r8','type':'friends_with','data':'bar'}");
            _Graph.AddEdge("joel", "matt", "{'guid':'r9','type':'worked_with','data':'baz'}");
            _Graph.AddEdge("matt", "bob", "{'guid':'r10','type':'worked_with','data':'foo'}");
            _Graph.AddEdge("jason", "maria", "{'guid':'r11','type':'is_child_of','data':'bar'}");
            _Graph.AddEdge("car1", "car3", "{'guid':'r12','type':'is_a','data':'baz'}");
            _Graph.AddEdge("maria", "piano", "{'guid':'r13','type':'plays','data':'foo'}");
        }

        static void LoadDefault2Graph()
        {
            // Nodes
            _Graph.AddNode("{'guid':'a','name':'a','type':'node','data':'node_a'}");
            _Graph.AddNode("{'guid':'b','name':'b','type':'node','data':'node_b'}");
            _Graph.AddNode("{'guid':'c','name':'c','type':'node','data':'node_c'}");
            _Graph.AddNode("{'guid':'d','name':'d','type':'node','data':'node_d'}");
            _Graph.AddNode("{'guid':'e','name':'e','type':'node','data':'node_e'}");
            _Graph.AddNode("{'guid':'f','name':'f','type':'node','data':'node_f'}");
            _Graph.AddNode("{'guid':'g','name':'g','type':'node','data':'node_g'}");
            _Graph.AddNode("{'guid':'h','name':'h','type':'node','data':'node_h'}");
            _Graph.AddNode("{'guid':'i','name':'i','type':'node','data':'node_i'}");
            _Graph.AddNode("{'guid':'j','name':'j','type':'node','data':'node_j'}");
            _Graph.AddNode("{'guid':'k','name':'k','type':'node','data':'node_k'}");
            _Graph.AddNode("{'guid':'l','name':'l','type':'node','data':'node_l'}");

            // Edges
            _Graph.AddEdge("a", "b", "{'guid':'a_b','type':'edge','data':'edge_a_b'}");
            _Graph.AddEdge("a", "c", "{'guid':'a_c','type':'edge','data':'edge_a_c'}");
            _Graph.AddEdge("a", "d", "{'guid':'a_d','type':'edge','data':'edge_a_d'}");
            _Graph.AddEdge("b", "e", "{'guid':'b_e','type':'edge','data':'edge_b_e'}");
            _Graph.AddEdge("b", "f", "{'guid':'b_f','type':'edge','data':'edge_b_f'}");
            _Graph.AddEdge("c", "e", "{'guid':'c_e','type':'edge','data':'edge_c_e'}");
            _Graph.AddEdge("c", "g", "{'guid':'c_g','type':'edge','data':'edge_c_g'}");
            _Graph.AddEdge("d", "f", "{'guid':'d_f','type':'edge','data':'edge_d_f'}");
            _Graph.AddEdge("d", "g", "{'guid':'d_g','type':'edge','data':'edge_d_g'}");
            _Graph.AddEdge("e", "h", "{'guid':'e_h','type':'edge','data':'edge_e_h'}");
            _Graph.AddEdge("e", "i", "{'guid':'e_i','type':'edge','data':'edge_e_i'}");
            _Graph.AddEdge("e", "j", "{'guid':'e_j','type':'edge','data':'edge_e_j'}");
            _Graph.AddEdge("f", "h", "{'guid':'f_h','type':'edge','data':'edge_f_h'}");
            _Graph.AddEdge("f", "i", "{'guid':'f_i','type':'edge','data':'edge_f_i'}");
            _Graph.AddEdge("f", "j", "{'guid':'f_j','type':'edge','data':'edge_f_j'}");
            _Graph.AddEdge("g", "h", "{'guid':'g_h','type':'edge','data':'edge_g_h'}");
            _Graph.AddEdge("g", "i", "{'guid':'g_i','type':'edge','data':'edge_g_i'}");
            _Graph.AddEdge("g", "j", "{'guid':'g_j','type':'edge','data':'edge_g_j'}");
            _Graph.AddEdge("h", "l", "{'guid':'h_l','type':'edge','data':'edge_h_l'}");
            _Graph.AddEdge("i", "k", "{'guid':'i_k','type':'edge','data':'edge_i_k'}");
            _Graph.AddEdge("j", "k", "{'guid':'j_k','type':'edge','data':'edge_j_k'}");
        }

        static void AddNode()
        {
            Console.WriteLine("Supplied JSON must contain the property '" + _Graph.NodeGuidProperty + "'");
            string json = InputString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return;
            Enumerate(_Graph.AddNode(json));
        }

        static void AllNodes()
        {
            Enumerate(_Graph.GetAllNodes());
        }

        static void GetNode()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            Enumerate(_Graph.GetNode(guid));
        }

        static void GetDescendants()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            List<string> types = InputStringList("Types:", true);
            Enumerate(_Graph.GetDescendants(guid, types));
        }

        static void UpdateNode()
        {
            string json = InputString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return;
            Enumerate(_Graph.UpdateNode(json));
        }

        static void AllEdges()
        {
            Enumerate(_Graph.GetAllEdges());
        }

        static void AddEdge()
        {
            string fromGuid = InputString("From GUID:", null, true);
            string toGuid = InputString("  To GUID:", null, true);
            if (String.IsNullOrEmpty(fromGuid) || String.IsNullOrEmpty(toGuid)) return;
            Console.WriteLine("Supplied JSON must contain the properties '" + _Graph.EdgeGuidProperty + "' and '" + _Graph.EdgeTypeProperty + "'");
            string json = InputString("     JSON:", null, true);
            Enumerate(_Graph.AddEdge(fromGuid, toGuid, json));
        }

        static void NodeEdges()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            Enumerate(_Graph.GetEdges(guid));
        }

        static void Neighbors()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            Enumerate(_Graph.GetNeighbors(guid));
        }

        static void GetEdge()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            Enumerate(_Graph.GetEdge(guid));
        }

        static void UpdateEdge()
        {
            string json = InputString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return;
            Enumerate(_Graph.UpdateEdge(json));
        }

        static void SearchNodes()
        {
            List<string> guids = InputStringList("GUID:", true);
            List<string> types = InputStringList("Type:", true);
            List<SearchFilter> filters = InputSearchFilter();
            Enumerate(_Graph.SearchNodes(guids, types, filters));
        }

        static void SearchEdges()
        {
            List<string> guids = InputStringList("GUID:", true);
            List<string> types = InputStringList("Type:", true);
            List<SearchFilter> filters = InputSearchFilter();
            Enumerate(_Graph.SearchEdges(guids, types, filters));
        }

        static void FindRoutes()
        {
            string fromGuid = InputString("From GUID:", null, true);
            if (String.IsNullOrEmpty(fromGuid)) return;
            string toGuid = InputString("To GUID:", null, true);
            if (String.IsNullOrEmpty(toGuid)) return;

            RouteFinder finder = new RouteFinder(_Graph);
            finder.FromGuid = fromGuid;
            finder.ToGuid = toGuid;

            Enumerate(finder.Find());
        }

        static void ExportToGexf()
        {
            string file = InputString("Output file:", "litegraph.gexf", false);
            GexfWriter.Write(_Graph, file);
        }

        private static void NodeAdded(object sender, NodeEventArgs args)
        {
            Console.WriteLine("[NodeAdded] " + args.GUID + " " + args.NodeType + " " + args.CreatedUtc.ToString() + ": " + Serialize(args.Properties, false));
        }

        private static void NodeUpdated(object sender, NodeEventArgs args)
        {
            Console.WriteLine("[NodeUpdated] " + args.GUID + " " + args.NodeType + " " + args.CreatedUtc.ToString() + ": " + Serialize(args.Properties, false));
        }

        private static void NodeRemoved(object sender, NodeEventArgs args)
        {
            Console.WriteLine("[NodeRemoved] " + args.GUID + " " + args.NodeType + " " + args.CreatedUtc.ToString() + ": " + Serialize(args.Properties, false));
        }

        private static void EdgeAdded(object sender, EdgeEventArgs args)
        {
            Console.WriteLine("[EdgeAdded] " + args.GUID + " " + args.EdgeType + " (" + args.FromGUID + " -> " + args.ToGUID + ") " + args.CreatedUtc.ToString() + ": " + Serialize(args.Properties, false));
        }

        private static void EdgeUpdated(object sender, EdgeEventArgs args)
        {
            Console.WriteLine("[EdgeUpdated] " + args.GUID + " " + args.EdgeType + " (" + args.FromGUID + " -> " + args.ToGUID + ") " + args.CreatedUtc.ToString() + ": " + Serialize(args.Properties, false));
        }

        private static void EdgeRemoved(object sender, EdgeEventArgs args)
        {
            Console.WriteLine("[EdgeRemoved] " + args.GUID + " " + args.EdgeType + " (" + args.FromGUID + " -> " + args.ToGUID + ") " + args.CreatedUtc.ToString() + ": " + Serialize(args.Properties, false));
        }
    }
}
