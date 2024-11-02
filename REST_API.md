# REST API for LiteGraph

## Data Structures

### Graph
```
{
    "GUID": "00000000-0000-0000-0000-000000000000",
    "Name": "My test graph",
    "CreatedUtc": "2024-07-01 15:43:06.991834"
}
```

### Node
```
{
    "GUID": "11111111-1111-1111-1111-111111111111",
    "GraphGUID": "00000000-0000-0000-0000-000000000000",
    "Name": "My test node",
    "Data": {
        "Hello": "World"
    },
    "CreatedUtc": "2024-07-01 15:43:06.991834"
}
```

### Edge
```
{
    "GUID": "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF",
    "GraphGUID": "00000000-0000-0000-0000-000000000000",
    "Name": "My test edge",
    "From": "11111111-1111-1111-1111-111111111111",
    "To": "22222222-2222-2222-2222-222222222222",
    "Cost": 10,
    "Data": {
        "Hello": "World"
    },
    "CreatedUtc": "2024-07-01 15:43:06.991834"
}
```

### Route Request
```
{
    "Graph": "00000000-0000-0000-0000-000000000000",
    "From": "11111111-1111-1111-1111-111111111111",
    "To": "22222222-2222-2222-2222-222222222222",
    "NodeFilter": null,
    "EdgeFilter": null,
}
```

### Existence Request
```
{
    "Nodes": [
        "[guid1]",
        "[guid2]",
        ...
    ],
    "Edges": [
        "[guid1]",
        "[guid2]",
        ...
    ],
    "EdgesBetween": [
        {
            "From": "[fromguid]",
            "To": "[toguid]"
        },
        ...
    ]
}
```

### Existence Result
```
{
    "ExistingNodes": [
        "[guid1]",
        "[guid2]",
        ...
    ],
    "MissingNodes": [
        "[guid1]",
        "[guid2]",
        ...
    ],
    "ExistingEdges": [
        "[guid1]",
        "[guid2]",
        ...
    ],
    "MissingEdges": [
        "[guid1]",
        "[guid2]",
        ...
    ],
    "ExistingEdgesBetween": [
        {
            "From": "[fromguid]",
            "To": "[toguid]"
        },
        ...
    ],
    "MissingEdgesBetween": [
        {
            "From": "[fromguid]",
            "To": "[toguid]"
        },
        ...
    ]
}
```

## General APIs

| API                   | Method | URL                                      |
|-----------------------|--------|------------------------------------------|
| Validate connectivity | HEAD   | /                                        |

## Graph APIs

| API                | Method | URL                                      |
|--------------------|--------|------------------------------------------|
| Create             | PUT    | /v1.0/graphs                             |
| Read               | GET    | /v1.0/graphs/[guid]                      |
| Read many          | GET    | /v1.0/graphs                             |
| Update             | PUT    | /v1.0/graphs/[guid]                      |
| Delete             | DELETE | /v1.0/graphs/[guid]                      |
| Delete w/ cascade  | DELETE | /v1.0/graphs/[guid]                      |
| Exists             | HEAD   | /v1.0/graphs/[guid]                      |
| Search             | POST   | /v1.0/graphs/search                      |
| Render as GEXF     | GET    | /v1.0/graphs/[guid]/export/gexf?incldata |
| Batch existence    | POST   | /v1.0/graphs/[guid]/existence            |

## Node APIs

| API             | Method | URL                                |
|-----------------|--------|------------------------------------|
| Create          | PUT    | /v1.0/graphs/[guid]/nodes          |
| Read            | GET    | /v1.0/graphs/[guid]/nodes/[guid]   |
| Read many       | GET    | /v1.0/graphs/[guid]/nodes          |
| Update          | PUT    | /v1.0/graphs/[guid]/nodes/[guid]   |
| Delete all      | DELETE | /v1.0/graphs/[guid]/nodes/all      |
| Delete multiple | DELETE | /v1.0/graphs/[guid]/nodes/multiple |
| Delete          | DELETE | /v1.0/graphs/[guid]/nodes/[guid]   |
| Exists          | HEAD   | /v1.0/graphs/[guid]/nodes/[guid]   |
| Search          | POST   | /v1.0/graphs/[guid]/nodes/search   |

## Edge APIs

| API             | Method | URL                                       |
|-----------------|--------|-------------------------------------------|
| Create          | PUT    | /v1.0/graphs/[guid]/edges                 |
| Read            | GET    | /v1.0/graphs/[guid]/edges/[guid]          |
| Read many       | GET    | /v1.0/graphs/[guid]/edges                 |
| Update          | PUT    | /v1.0/graphs/[guid]/edges/[guid]          |
| Delete all      | DELETE | /v1.0/graphs/[guid]/edges/[guid]/all      |
| Delete multiple | DELETE | /v1.0/graphs/[guid]/edges/[guid]/multiple |
| Delete          | DELETE | /v1.0/graphs/[guid]/edges/[guid]          |
| Exists          | HEAD   | /v1.0/graphs/[guid]/edges/[guid]          |
| Search          | POST   | /v1.0/graphs/[guid]/edges/search          |

## Traversal and Networking

| API                            | Method | URL                                          |
|--------------------------------|--------|----------------------------------------------|
| Get edges from a node          | GET    | /v1.0/graphs/[guid]/nodes/[guid]/edges/from  |
| Get edges to a node            | GET    | /v1.0/graphs/[guid]/nodes/[guid]/edges/to    |
| Get edges connected to a node  | GET    | /v1.0/graphs/[guid]/nodes/[guid]/edges       |
| Get node neighbors             | GET    | /v1.0/graphs/[guid]/nodes/[guid]/neighbors   |
| Get node parents               | GET    | /v1.0/graphs/[guid]/nodes/[guid]/parents     |
| Get node children              | GET    | /v1.0/graphs/[guid]/nodes/[guid]/children    |
| Get routes between nodes       | POST   | /v1.0/graphs/[guid]/routes                   |
