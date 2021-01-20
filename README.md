![alt tag](https://github.com/jchristn/indexengine/blob/master/assets/icon.png)

# LiteGraph

[![NuGet Version](https://img.shields.io/nuget/v/LiteGraph.svg?style=flat)](https://www.nuget.org/packages/LiteGraph/) [![NuGet](https://img.shields.io/nuget/dt/LiteGraph.svg)](https://www.nuget.org/packages/LiteGraph) 

LiteGraph is a lightweight graph database built using Sqlite with support for exporting to GEXF.

## New in v1.0.0.1

- Initial release
- Add nodes and edges with properties for each
- Find routes between nodes using specified filters
- Export the graph to GEXF for use with Gephi (https://github.com/gephi/gephi)

## Important

- Node JSON must contain three properties
  - GUID - globally unique identifier, using property name as set in the ```NodeGuidProperty``` field (default ```guid```)
  - Name - name of the node, using property name as set in the ```NodeNameProperty``` field (default ```name```)
  - Type - type of the node, using property name as set in the ```NodeTypeProperty``` field (default ```type```)

- Edge JSON must contain two properties
  - GUID - globally unique identifier, using property name as set in the ```EdgeGuidProperty``` field (default ```guid```)
  - Type - type of the node, using property name as set in the ```EdgeTypeProperty``` field (default ```type```)
 
## Simple Example

Refer to the ```Test``` project for a full example.

```csharp
using LiteGraph;

LiteGraphClient graph = new LiteGraphClient("litegraph.db");

// Add two people
graph.AddNode("{'guid':'joel','name':'Joel','type':'person','first':'Joel','city':'San Jose'}");
graph.AddNode("{'guid':'maria','name':'Maria','type':'person','first':'Maria','city':'San Jose'}");

// Add a relationship
// First parameter is the from GUID, second is the to GUID
graph.AddEdge("joel", "maria", "{'guid':'relationship1','type':'loves','data':'whatever else you want'}");

// Find a route
RouteFinder rf = new RouteFinder(graph);
rf.FromGuid = "joel";
rf.ToGuid = "maria";
GraphResult result = rf.Find();

// Export to GEXF
GexfWriter.Write(graph, "litegraph.gexf");
```

## Version History

Please refer to ```CHANGELOG.md``` for version history.

