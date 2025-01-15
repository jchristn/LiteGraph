# REST API for LiteGraph

## Data Structures

### Tenant
```
{
    "GUID": "00000000-0000-0000-0000-000000000000",
    "Name": "Default tenant",
    "Active": true,
    "CreatedUtc": "2024-12-27T22:09:09.410802Z",
    "LastUpdateUtc": "2024-12-27T22:09:09.410168Z"
}
```

### User
```
{
    "GUID": "00000000-0000-0000-0000-000000000000",
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
    "FirstName": "Default",
    "LastName": "User",
    "Email": "default@user.com",
    "Password": "password",
    "Active": true,
    "CreatedUtc": "2024-12-27T22:09:09.446911Z",
    "LastUpdateUtc": "2024-12-27T22:09:09.446777Z"
}
```

### Credential
```
{
    "GUID": "00000000-0000-0000-0000-000000000000",
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
    "UserGUID": "00000000-0000-0000-0000-000000000000",
    "Name": "Default credential",
    "BearerToken": "default",
    "Active": true,
    "CreatedUtc": "2024-12-27T22:09:09.468134Z",
    "LastUpdateUtc": "2024-12-27T22:09:09.467977Z"
}
```

### Label
```
{
    "GUID": "738d4956-a833-429a-9531-c99336638617",
    "TenantGUID": "ba1dc0a6-372d-47ee-aea5-75e7dbbbd175",
    "GraphGUID": "97826e1a-d0c1-4884-820a-bfda74b3be33",
    "EdgeGUID": "971da046-8234-4627-8ae8-e062311874c8",
    "Label": "edge",
    "CreatedUtc": "2025-01-08T23:28:05.312128Z",
    "LastUpdateUtc": "2025-01-08T23:28:05.312128Z"
}
```

### Tag
```
{
    "GUID": "00000000-0000-0000-0000-000000000000",
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
    "GraphGUID": "00000000-0000-0000-0000-000000000000",
    "NodeGUID": "00000000-0000-0000-0000-000000000000",
    "EdgeGUID": "00000000-0000-0000-0000-000000000000",
    "Key": "mykey",
    "Value": "myvalue",
    "CreatedUtc": "2024-12-27T22:14:36.459901Z",
    "LastUpdateUtc": "2024-12-27T22:14:36.459902Z"
}
```

### Vector
```
{
    "GUID": "00000000-0000-0000-0000-000000000000",
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
    "GraphGUID": "00000000-0000-0000-0000-000000000000",
    "NodeGUID": "00000000-0000-0000-0000-000000000000",
    "EdgeGUID": "00000000-0000-0000-0000-000000000000",
    "Model": "testmodel",
    "Dimensionality": 3,
    "Content": "test content",
    "Vectors": [ 0.05, -0.25, 0.45 ],
    "CreatedUtc": "2025-01-15T10:41:13.243174Z",
    "LastUpdateUtc": "2025-01-15T10:41:13.243188Z"
}
```

### Graph
```
{
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
    "GUID": "00000000-0000-0000-0000-000000000000",
    "Name": "My test graph",
    "Tags": {
        "Key": "Value"
    },
    "Data": {
        "Hello": "World"
    },
    "CreatedUtc": "2024-07-01 15:43:06.991834"
}
```

### Node
```
{
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
    "GUID": "11111111-1111-1111-1111-111111111111",
    "GraphGUID": "00000000-0000-0000-0000-000000000000",
    "Name": "My test node",
    "Tags": {
        "Key": "Value"
    },
    "Data": {
        "Hello": "World"
    },
    "CreatedUtc": "2024-07-01 15:43:06.991834"
}
```

### Edge
```
{
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
    "GUID": "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF",
    "GraphGUID": "00000000-0000-0000-0000-000000000000",
    "Name": "My test edge",
    "From": "11111111-1111-1111-1111-111111111111",
    "To": "22222222-2222-2222-2222-222222222222",
    "Cost": 10,
    "Tags": {
        "Key": "Value"
    },
    "Data": {
        "Hello": "World"
    },
    "CreatedUtc": "2024-07-01 15:43:06.991834"
}
```

### Route Request
```
{
    "TenantGUID": "00000000-0000-0000-0000-000000000000",
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

### Vector Search Request

```
{
    "GraphGUID": "00000000-0000-0000-0000-000000000000",
    "Domain": "Graph",
    "SearchType": "CosineSimilarity",
    "Labels": [],
    "Tags": {},
    "Expr": null,
    "Embeddings": [ 0.1, 0.2, 0.3 ]
}
```

Valid domains are `Graph` `Node` `Edge`
Valid search types are `CosineSimilarity` `CosineDistance` `EuclidianSimilarity` `EuclidianDistance` `DotProduct`

### Vector Search Result

```
[
    {
        "Score": 0.874456,
        "Distance": null,
        "InnerProduct": null,
        "Graph": { ... },
        "Node": { ... },
        "Edge": { ... }
    },
    ...
]
```

## General APIs

| API                   | Method | URL                                      |
|-----------------------|--------|------------------------------------------|
| Validate connectivity | HEAD   | /                                        |

## Tenant APIs

Tenant APIs require administrator bearer token authentication.

| API                | Method | URL                        |
|--------------------|--------|----------------------------|
| Create             | PUT    | /v1.0/tenants              |
| Read many          | GET    | /v1.0/tenants              |
| Read               | GET    | /v1.0/tenants/[guid]       |
| Update             | PUT    | /v1.0/tenants/[guid]       |
| Delete             | DELETE | /v1.0/tenants/[guid]       |
| Delete w/ cascade  | DELETE | /v1.0/tenants/[guid]?force |
| Exists             | HEAD   | /v1.0/tenants/[guid]       |

## User APIs

User APIs require administrator bearer token authentication.

| API                | Method | URL                               |
|--------------------|--------|-----------------------------------|
| Create             | PUT    | /v1.0/tenants/[guid]/users        |
| Read many          | GET    | /v1.0/tenants/[guid]/users        |
| Read               | GET    | /v1.0/tenants/[guid]/users/[guid] |
| Update             | PUT    | /v1.0/tenants/[guid]/users/[guid] |
| Delete             | DELETE | /v1.0/tenants/[guid]/users/[guid] |
| Exists             | HEAD   | /v1.0/tenants/[guid]/users/[guid] |

## Credential APIs

Credential APIs require administrator bearer token authentication.

| API                | Method | URL                                     |
|--------------------|--------|-----------------------------------------|
| Create             | PUT    | /v1.0/tenants/[guid]/credentials        |
| Read many          | GET    | /v1.0/tenants/[guid]/credentials        |
| Read               | GET    | /v1.0/tenants/[guid]/credentials/[guid] |
| Update             | PUT    | /v1.0/tenants/[guid]/credentials/[guid] |
| Delete             | DELETE | /v1.0/tenants/[guid]/credentials/[guid] |
| Exists             | HEAD   | /v1.0/tenants/[guid]/credentials/[guid] |

## Label APIs

Label APIs require administrator bearer token authentication.

| API                | Method | URL                                |
|--------------------|--------|------------------------------------|
| Create             | PUT    | /v1.0/tenants/[guid]/labels        |
| Read many          | GET    | /v1.0/tenants/[guid]/labels        |
| Read               | GET    | /v1.0/tenants/[guid]/labels/[guid] |
| Update             | PUT    | /v1.0/tenants/[guid]/labels/[guid] |
| Delete             | DELETE | /v1.0/tenants/[guid]/labels/[guid] |
| Exists             | HEAD   | /v1.0/tenants/[guid]/labels/[guid] |

## Tag APIs

Tag APIs require administrator bearer token authentication.

| API                | Method | URL                              |
|--------------------|--------|----------------------------------|
| Create             | PUT    | /v1.0/tenants/[guid]/tags        |
| Read many          | GET    | /v1.0/tenants/[guid]/tags        |
| Read               | GET    | /v1.0/tenants/[guid]/tags/[guid] |
| Update             | PUT    | /v1.0/tenants/[guid]/tags/[guid] |
| Delete             | DELETE | /v1.0/tenants/[guid]/tags/[guid] |
| Exists             | HEAD   | /v1.0/tenants/[guid]/tags/[guid] |

## Vector APIs

Vector APIs require administrator bearer token authentication, aside from the vector search API.

| API                | Method | URL                                 |
|--------------------|--------|-------------------------------------|
| Create             | PUT    | /v1.0/tenants/[guid]/vectors        |
| Read many          | GET    | /v1.0/tenants/[guid]/vectors        |
| Read               | GET    | /v1.0/tenants/[guid]/vectors/[guid] |
| Update             | PUT    | /v1.0/tenants/[guid]/vectors/[guid] |
| Delete             | DELETE | /v1.0/tenants/[guid]/vectors/[guid] |
| Exists             | HEAD   | /v1.0/tenants/[guid]/vectors/[guid] |
| Search             | POST   | /v1.0/tenants/[guid]/vectors        |

## Graph APIs

| API                | Method | URL                                                     |
|--------------------|--------|---------------------------------------------------------|
| Create             | PUT    | /v1.0/tenants/[guid]/graphs                             |
| Read               | GET    | /v1.0/tenants/[guid]/graphs/[guid]                      |
| Read many          | GET    | /v1.0/tenants/[guid]/graphs                             |
| Update             | PUT    | /v1.0/tenants/[guid]/graphs/[guid]                      |
| Delete             | DELETE | /v1.0/tenants/[guid]/graphs/[guid]                      |
| Delete w/ cascade  | DELETE | /v1.0/tenants/[guid]/graphs/[guid]?force                |
| Exists             | HEAD   | /v1.0/tenants/[guid]/graphs/[guid]                      |
| Search             | POST   | /v1.0/tenants/[guid]/graphs/search                      |
| Render as GEXF     | GET    | /v1.0/tenants/[guid]/graphs/[guid]/export/gexf?incldata |
| Batch existence    | POST   | /v1.0/tenants/[guid]/graphs/[guid]/existence            |

## Node APIs

| API             | Method | URL                                               |
|-----------------|--------|---------------------------------------------------|
| Create          | PUT    | /v1.0/tenants/[guid]/graphs/[guid]/nodes          |
| Read            | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]   |
| Read many       | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes          |
| Update          | PUT    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]   |
| Delete all      | DELETE | /v1.0/tenants/[guid]/graphs/[guid]/nodes/all      |
| Delete multiple | DELETE | /v1.0/tenants/[guid]/graphs/[guid]/nodes/multiple |
| Delete          | DELETE | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]   |
| Exists          | HEAD   | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]   |
| Search          | POST   | /v1.0/tenants/[guid]/graphs/[guid]/nodes/search   |

## Edge APIs

| API             | Method | URL                                                      |
|-----------------|--------|----------------------------------------------------------|
| Create          | PUT    | /v1.0/tenants/[guid]/graphs/[guid]/edges                 |
| Read            | GET    | /v1.0/tenants/[guid]/graphs/[guid]/edges/[guid]          |
| Read many       | GET    | /v1.0/tenants/[guid]/graphs/[guid]/edges                 |
| Update          | PUT    | /v1.0/tenants/[guid]/graphs/[guid]/edges/[guid]          |
| Delete all      | DELETE | /v1.0/tenants/[guid]/graphs/[guid]/edges/[guid]/all      |
| Delete multiple | DELETE | /v1.0/tenants/[guid]/graphs/[guid]/edges/[guid]/multiple |
| Delete          | DELETE | /v1.0/tenants/[guid]/graphs/[guid]/edges/[guid]          |
| Exists          | HEAD   | /v1.0/tenants/[guid]/graphs/[guid]/edges/[guid]          |
| Search          | POST   | /v1.0/tenants/[guid]/graphs/[guid]/edges/search          |

## Traversal and Networking

| API                            | Method | URL                                                         |
|--------------------------------|--------|-------------------------------------------------------------|
| Get edges from a node          | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]/edges/from  |
| Get edges to a node            | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]/edges/to    |
| Get edges connected to a node  | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]/edges       |
| Get node neighbors             | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]/neighbors   |
| Get node parents               | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]/parents     |
| Get node children              | GET    | /v1.0/tenants/[guid]/graphs/[guid]/nodes/[guid]/children    |
| Get routes between nodes       | POST   | /v1.0/tenants/[guid]/graphs/[guid]/routes                   |
