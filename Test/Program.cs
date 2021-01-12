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
                    case "default":
                        LoadDefaultGraph();
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
            Console.WriteLine("  default         load default nodes and edges (see documentation)");
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

        static void LoadDefaultGraph()
        {
            // People
            _Graph.AddNode("{'guid':'joel','type':'person','first':'Joel','city':'San Jose'}");
            _Graph.AddNode("{'guid':'maria','type':'person','first':'Maria','city':'San Jose'}");
            _Graph.AddNode("{'guid':'jason','type':'person','first':'Jason','city':'San Jose'}");
            _Graph.AddNode("{'guid':'scott','type':'person','first':'Scott','city':'Chicago'}");
            _Graph.AddNode("{'guid':'may','type':'person','first':'May','city':'New York City'}");
            _Graph.AddNode("{'guid':'matt','type':'person','first':'Matt','city':'Raleigh'}");
            _Graph.AddNode("{'guid':'bob','type':'person','first':'Bob','city':'Asheville'}");

            // Things
            _Graph.AddNode("{'guid':'car1','type':'car','make':'Toyota','model':'Highlander'}");
            _Graph.AddNode("{'guid':'car2','type':'car','make':'Volkswagen','model':'Jetta'}");
            _Graph.AddNode("{'guid':'car3','type':'car','make':'Mercedes','model':'SUV'}");
            _Graph.AddNode("{'guid':'guitar','type':'instrument','make':'Jackson','model':'Soloist'}");
            _Graph.AddNode("{'guid':'piano','type':'instrument','make':'Yamaha','model':'Keyboard'}");
            _Graph.AddNode("{'guid':'house','type':'house','desc':'Super duper house'}");

            // Relationships
            _Graph.AddEdge("joel", "house", "{'guid':'r1','type':'lives_in','data':'foo'}");
            _Graph.AddEdge("maria", "house", "{'guid':'r2','type':'lives_in','data':'bar'}");
            _Graph.AddEdge("jason", "house", "{'guid':'r3','type':'lives_in','data':'baz'}");
            _Graph.AddEdge("joel", "scott", "{'guid':'r4','type':'friends_with','data':'foo'}");
            _Graph.AddEdge("maria", "may", "{'guid':'r5','type':'friends_with','data':'bar'}");
            _Graph.AddEdge("joel", "matt", "{'guid':'r6','type':'worked_with','data':'baz'}");
            _Graph.AddEdge("matt", "bob", "{'guid':'r7','type':'worked_with','data':'foo'}");
            _Graph.AddEdge("jason", "maria", "{'guid':'r8','type':'is_child_of','data':'bar'}");
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
            Console.WriteLine("Supplied JSON must contain the property '" + _Graph.EdgeGuidProperty + "'");
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
