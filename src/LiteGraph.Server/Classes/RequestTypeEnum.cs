namespace LiteGraph.Server.Classes
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Request type.
    /// </summary>
    public enum RequestTypeEnum
    {
        #region General

        /// <summary>
        /// Root
        /// </summary>
        [EnumMember(Value = "Root")]
        Root,
        /// <summary>
        /// Loopback
        /// </summary>
        [EnumMember(Value = "Loopback")]
        Loopback,
        /// <summary>
        /// Favicon
        /// </summary>
        [EnumMember(Value = "Favicon")]
        Favicon,
        /// <summary>
        /// Unknown
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,

        #endregion

        #region Tenants

        /// <summary>
        /// TenantCreate
        /// </summary>
        [EnumMember(Value = "TenantCreate")]
        TenantCreate,
        /// <summary>
        /// TenantDelete
        /// </summary>
        [EnumMember(Value = "TenantDelete")]
        TenantDelete,
        /// <summary>
        /// TenantExists
        /// </summary>
        [EnumMember(Value = "TenantExists")]
        TenantExists,
        /// <summary>
        /// TenantRead
        /// </summary>
        [EnumMember(Value = "TenantRead")]
        TenantRead,
        /// <summary>
        /// TenantReadMany
        /// </summary>
        [EnumMember(Value = "TenantReadMany")]
        TenantReadMany,
        /// <summary>
        /// TenantUpdate
        /// </summary>
        [EnumMember(Value = "TenantUpdate")]
        TenantUpdate,

        #endregion
        #region Users

        /// <summary>
        /// UserCreate
        /// </summary>
        [EnumMember(Value = "UserCreate")]
        UserCreate,
        /// <summary>
        /// UserDelete
        /// </summary>
        [EnumMember(Value = "UserDelete")]
        UserDelete,
        /// <summary>
        /// UserExists
        /// </summary>
        [EnumMember(Value = "UserExists")]
        UserExists,
        /// <summary>
        /// UserRead
        /// </summary>
        [EnumMember(Value = "UserRead")]
        UserRead,
        /// <summary>
        /// UserReadMany
        /// </summary>
        [EnumMember(Value = "UserReadMany")]
        UserReadMany,
        /// <summary>
        /// UserUpdate
        /// </summary>
        [EnumMember(Value = "UserUpdate")]
        UserUpdate,

        #endregion

        #region Credentials

        /// <summary>
        /// CredentialCreate
        /// </summary>
        [EnumMember(Value = "CredentialCreate")]
        CredentialCreate,
        /// <summary>
        /// CredentialDelete
        /// </summary>
        [EnumMember(Value = "CredentialDelete")]
        CredentialDelete,
        /// <summary>
        /// CredentialExists
        /// </summary>
        [EnumMember(Value = "CredentialExists")]
        CredentialExists,
        /// <summary>
        /// CredentialRead
        /// </summary>
        [EnumMember(Value = "CredentialRead")]
        CredentialRead,
        /// <summary>
        /// CredentialReadMany
        /// </summary>
        [EnumMember(Value = "CredentialReadMany")]
        CredentialReadMany,
        /// <summary>
        /// CredentialUpdate
        /// </summary>
        [EnumMember(Value = "CredentialUpdate")]
        CredentialUpdate,

        #endregion

        #region Tags

        /// <summary>
        /// TagCreate
        /// </summary>
        [EnumMember(Value = "TagCreate")]
        TagCreate,
        /// <summary>
        /// TagDelete
        /// </summary>
        [EnumMember(Value = "TagDelete")]
        TagDelete,
        /// <summary>
        /// TagExists
        /// </summary>
        [EnumMember(Value = "TagExists")]
        TagExists,
        /// <summary>
        /// TagRead
        /// </summary>
        [EnumMember(Value = "TagRead")]
        TagRead,
        /// <summary>
        /// TagReadMany
        /// </summary>
        [EnumMember(Value = "TagReadMany")]
        TagReadMany,
        /// <summary>
        /// TagUpdate
        /// </summary>
        [EnumMember(Value = "TagUpdate")]
        TagUpdate,

        #endregion

        #region Graphs

        /// <summary>
        /// GraphCreate
        /// </summary>
        [EnumMember(Value = "GraphCreate")]
        GraphCreate,
        /// <summary>
        /// GraphDelete
        /// </summary>
        [EnumMember(Value = "GraphDelete")]
        GraphDelete,
        /// <summary>
        /// GraphExistence
        /// </summary>
        [EnumMember(Value = "GraphExistence")]
        GraphExistence,
        /// <summary>
        /// GraphExists
        /// </summary>
        [EnumMember(Value = "GraphExists")]
        GraphExists,
        /// <summary>
        /// GraphExport
        /// </summary>
        [EnumMember(Value = "GraphExport")]
        GraphExport,
        /// <summary>
        /// GraphRead
        /// </summary>
        [EnumMember(Value = "GraphRead")]
        GraphRead,
        /// <summary>
        /// GraphReadMany
        /// </summary>
        [EnumMember(Value = "GraphReadMany")]
        GraphReadMany,
        /// <summary>
        /// GraphSearch
        /// </summary>
        [EnumMember(Value = "GraphSearch")]
        GraphSearch,
        /// <summary>
        /// GraphUpdate
        /// </summary>
        [EnumMember(Value = "GraphUpdate")]
        GraphUpdate,

        #endregion

        #region Nodes

        /// <summary>
        /// NodeCreate
        /// </summary>
        [EnumMember(Value = "NodeCreate")]
        NodeCreate,
        /// <summary>
        /// NodeCreateMultiple
        /// </summary>
        [EnumMember(Value = "NodeCreateMultiple")]
        NodeCreateMultiple,
        /// <summary>
        /// NodeDelete
        /// </summary>
        [EnumMember(Value = "NodeDelete")]
        NodeDelete,
        /// <summary>
        /// NodeDeleteAll
        /// </summary>
        [EnumMember(Value = "NodeDeleteAll")]
        NodeDeleteAll,
        /// <summary>
        /// NodeDeleteMultiple
        /// </summary>
        [EnumMember(Value = "NodeDeleteMultiple")]
        NodeDeleteMultiple,
        /// <summary>
        /// NodeExists
        /// </summary>
        [EnumMember(Value = "NodeExists")]
        NodeExists,
        /// <summary>
        /// NodeRead
        /// </summary>
        [EnumMember(Value = "NodeRead")]
        NodeRead,
        /// <summary>
        /// NodeReadMany
        /// </summary>
        [EnumMember(Value = "NodeReadMany")]
        NodeReadMany,
        /// <summary>
        /// NodeSearch
        /// </summary>
        [EnumMember(Value = "NodeSearch")]
        NodeSearch,
        /// <summary>
        /// NodeUpdate
        /// </summary>
        [EnumMember(Value = "NodeUpdate")]
        NodeUpdate,

        #endregion

        #region Edges

        /// <summary>
        /// EdgeBetween
        /// </summary>
        [EnumMember(Value = "EdgeBetween")]
        EdgeBetween,
        /// <summary>
        /// EdgeCreate
        /// </summary>
        [EnumMember(Value = "EdgeCreate")]
        EdgeCreate,
        /// <summary>
        /// EdgeCreateMultiple
        /// </summary>
        [EnumMember(Value = "EdgeCreateMultiple")]
        EdgeCreateMultiple,
        /// <summary>
        /// EdgeDelete
        /// </summary>
        [EnumMember(Value = "EdgeDelete")]
        EdgeDelete,
        /// <summary>
        /// EdgeDeleteAll
        /// </summary>
        [EnumMember(Value = "EdgeDeleteAll")]
        EdgeDeleteAll,
        /// <summary>
        /// EdgeDeleteMultiple
        /// </summary>
        [EnumMember(Value = "EdgeDeleteMultiple")]
        EdgeDeleteMultiple,
        /// <summary>
        /// EdgeExists
        /// </summary>
        [EnumMember(Value = "EdgeExists")]
        EdgeExists,
        /// <summary>
        /// EdgeRead
        /// </summary>
        [EnumMember(Value = "EdgeRead")]
        EdgeRead,
        /// <summary>
        /// EdgeReadMany
        /// </summary>
        [EnumMember(Value = "EdgeReadMany")]
        EdgeReadMany,
        /// <summary>
        /// EdgeSearch
        /// </summary>
        [EnumMember(Value = "EdgeSearch")]
        EdgeSearch,
        /// <summary>
        /// EdgeUpdate
        /// </summary>
        [EnumMember(Value = "EdgeUpdate")]
        EdgeUpdate,

        #endregion

        #region Topology

        /// <summary>
        /// EdgesFromNode
        /// </summary>
        [EnumMember(Value = "EdgesFromNode")]
        EdgesFromNode,
        /// <summary>
        /// EdgesToNode
        /// </summary>
        [EnumMember(Value = "EdgesToNode")]
        EdgesToNode,
        /// <summary>
        /// AllEdgesToNode
        /// </summary>
        [EnumMember(Value = "AllEdgesToNode")]
        AllEdgesToNode,
        /// <summary>
        /// NodeChildren
        /// </summary>
        [EnumMember(Value = "NodeChildren")]
        NodeChildren,
        /// <summary>
        /// NodeParents
        /// </summary>
        [EnumMember(Value = "NodeParents")]
        NodeParents,
        /// <summary>
        /// NodeNeighbors
        /// </summary>
        [EnumMember(Value = "NodeNeighbors")]
        NodeNeighbors,
        /// <summary>
        /// GetRoutes
        /// </summary>
        [EnumMember(Value = "GetRoutes")]
        GetRoutes

        #endregion
    }
}
