![alt tag](https://github.com/jchristn/LiteGraph/blob/master/assets/favicon.png)

# LiteGraph

[![NuGet Version](https://img.shields.io/nuget/v/LiteGraph.svg?style=flat)](https://www.nuget.org/packages/LiteGraph/) [![NuGet](https://img.shields.io/nuget/dt/LiteGraph.svg)](https://www.nuget.org/packages/LiteGraph) 

LiteGraph is a lightweight graph database built using Sqlite with support for exporting to GEXF.

## New in v2.0.0

- Major overhaul, refactor, and breaking changes
- Integrated webserver and RESTful API
- Extensibility through base repository class
- Hierarchical expression support while filtering over graph, node, and edge data objects
- Removal of property constraints on nodes and edges

## Bugs, Feedback, or Enhancement Requests

Please feel free to start an issue or a discussion!

## Simple Example

Refer to the ```Test``` project for a full example.

```csharp
using LiteGraph;

LiteGraphClient graph = new LiteGraphClient(); // using Sqlite file litegraph.db
LiteGraphClient graph = new LiteGraphClient(
  new SqliteRepository("mygraph.db")
  ); // use a specific file

// Create a graph
Graph graph = graph.CreateGraph("mygraph");

// Create nodes
Node node1 = graph.CreateNode(graph.GUID, new Node { Name = "node1" });
Node node2 = graph.CreateNode(graph.GUID, new Node { Name = "node2" });
Node node3 = graph.CreateNode(graph.GUID, new Node { Name = "node3" });

// Create edges
Edge edge1 = graph.CreateEdge(graph.GUID, node1.GUID, node2.GUID, "Node 1 to node 2");
Edge edge2 = graph.CreateEdge(graph.GUID, node2.GUID, node3.GUID, "Node 2 to node 3");

// Find routes
foreach (RouteDetail route in graph.GetRoutes(
  SearchTypeEnum.DepthFirstSearch,
  graph.GUID,
  node1.GUID,
  node2.GUID))
{
  Console.WriteLine(...);
}

// Export to GEXF file
graph.ExportGraphToGexfFile(graph.GUID, "mygraph.gexf");
```

## Working with Object Data

The `Data` property can be attached to any `Graph`, `Node`, or `Edge` object.  The value must be serializable to JSON.  This value is retrieved when reading objects, and filters can be created to retrieve only objects that have matches based on elements in the object stored in `Data`.  Refer to [ExpressionTree](https://github.com/jchristn/ExpressionTree/) for information on how to craft expressions.

```csharp
using ExpressionTree;

class Person 
{
  public string Name { get; set; } = null;
  public int Age { get; set; } = 0;
  public string City { get; set; } = "San Jose";
}

Person person1 = new Person { Name = "Joel", Age = 47, City = "San Jose" };
graph.CreateNode(graph.GUID, new Node { Name = "Joel", Data = person1 });

Expr expr = new Expr 
{
  "Left": "City",
  "Operator": "Equals",
  "Right": "San Jose"
};

foreach (Node node in graph.ReadNodes(graph.GUID, expr))
{
  Console.WriteLine(...);
}
```

## REST API

LiteGraph includes a project called `LiteGraph.Server` which allows you to deploy a RESTful front-end for LiteGraph.  Refer to `REST_API.md` and also the Postman collection in the root of this repository for details.  By default, LiteGraph.Server listens on `http://localhost:8701` and is only accessible to `localhost`.  Modify the `litegraph.json` file to change settings including hostname and port.

```csharp
$ cd LiteGraph.Server/bin/Debug/net8.0
$ dotnet LiteGraph.Server.dll

  _ _ _                          _
 | (_) |_ ___ __ _ _ _ __ _ _ __| |_
 | | |  _/ -_) _` | '_/ _` | '_ \ ' \
 |_|_|\__\___\__, |_| \__,_| .__/_||_|
             |___/         |_|

 LiteGraph Server
 (c)2024 Joel Christner

Using settings file './litegraph.json'
Settings file './litegraph.json' does not exist, creating
Initializing logging
| syslog://127.0.0.1:514
2024-07-17 20:56:13 INSPIRON-14 1 Debug [LiteGraphServer] logging initialized
2024-07-17 20:56:13 INSPIRON-14 1 Debug [RestServiceHandler] starting REST server on http://localhost:8701/

Important!
| Configured to listen on localhost; LiteGraph will not be externally accessible
| Modify ./litegraph.json to change the REST listener hostname

2024-07-17 20:56:13 INSPIRON-14 1 Info [LiteGraphServer] starting at 7/17/2024 8:56:13 PM using process ID 3256
```

## Running in Docker

A Docker image is available in Docker Hub under `jchristn/litegraph:v2.0.0`.  Use the Docker Compose start (`compose-up.sh` and `compose-up.bat`) and stop (`compose-down.sh` and `compose-down.bat`) scripts in the `Docker` directory if you wish to run within Docker Compose.  Ensure that you have a valid database file (e.g. `litegraph.db`) and configuration file (e.g. `litegraph.json`) exposed into your container.

## Version History

Please refer to ```CHANGELOG.md``` for version history.

