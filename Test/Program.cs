using System;
using System.Collections.Generic;
using LiteGraph;

namespace Test
{
    class Program
    {
        static bool _RunForever = true;
        static LiteGraphClient _Graph = null;

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
                    case "debug queries":
                        _Graph.Logger.LogQueries = !_Graph.Logger.LogQueries;
                        break;
                    case "debug results":
                        _Graph.Logger.LogResults = !_Graph.Logger.LogResults;
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

        static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  ?               help, this menu");
            Console.WriteLine("  q               quit");
            Console.WriteLine("  cls             clear the screen");
            Console.WriteLine("  debug queries   toggle query debug, currently: " + _Graph.Logger.LogQueries);
            Console.WriteLine("  debug results   toggle results debug, currently: " + _Graph.Logger.LogResults);
            Console.WriteLine("  all nodes       retrieve all nodes from the graph");
            Console.WriteLine("  add node        add a node to the graph");
            Console.WriteLine("  get node        retrieve a node by GUID");
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

        static void AddNode()
        {
            Console.WriteLine("Supplied JSON must contain the property '" + _Graph.NodeGuidProperty + "'");
            string json = InputString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return;
            _Graph.AddNode(json);
        }

        static void AllNodes()
        {
            Console.WriteLine("Nodes:" + Environment.NewLine + _Graph.GetAllNodes());
        }

        static void GetNode()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            string json = _Graph.GetNode(guid);
            Console.WriteLine("Result:" + Environment.NewLine + json);
        }

        static void UpdateNode()
        {
            string json = InputString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return;
            _Graph.UpdateNode(json);
        }

        static void AllEdges()
        {
            Console.WriteLine("Edges:" + Environment.NewLine + _Graph.GetAllEdges());
        }

        static void AddEdge()
        {
            string fromGuid = InputString("From GUID:", null, true);
            string toGuid = InputString("  To GUID:", null, true);
            if (String.IsNullOrEmpty(fromGuid) || String.IsNullOrEmpty(toGuid)) return;
            Console.WriteLine("Supplied JSON must contain the property '" + _Graph.EdgeGuidProperty + "'");
            string json = InputString("     JSON:", null, true);
            _Graph.AddEdge(fromGuid, toGuid, json);
        }

        static void NodeEdges()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            string json = _Graph.GetEdges(guid);
            Console.WriteLine("Results:" + Environment.NewLine + json);
        }

        static void Neighbors()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            string json = _Graph.GetNeighbors(guid);
            Console.WriteLine("Results:" + Environment.NewLine + json);
        }

        static void GetEdge()
        {
            string guid = InputString("GUID:", null, true);
            if (String.IsNullOrEmpty(guid)) return;
            string json = _Graph.GetEdge(guid);
            Console.WriteLine("Result:" + Environment.NewLine + json);
        }

        static void UpdateEdge()
        {
            string json = InputString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return;
            _Graph.UpdateEdge(json);
        }

        static void SearchNodes()
        {
            List<string> guids = InputStringList("GUID:", true);
            List<SearchFilter> filters = InputSearchFilter();
            string json = _Graph.SearchNodes(guids, filters);
            Console.WriteLine("Results:" + Environment.NewLine + json);
        }

        static void SearchEdges()
        {
            List<string> guids = InputStringList("GUID:", true);
            List<SearchFilter> filters = InputSearchFilter();
            string json = _Graph.SearchEdges(guids, filters);
            Console.WriteLine("Results:" + Environment.NewLine + json);
        }
    }
}
