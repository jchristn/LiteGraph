namespace LiteGraph.GraphRepositories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;
    using ExpressionTree;
    using LiteGraph;
    using LiteGraph.Serialization;
    using Microsoft.Data.Sqlite;

    /// <summary>
    /// Graph repository base class.
    /// </summary>
    public abstract class GraphRepositoryBase
    {
        #region Public-Members

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LoggingSettings Logging
        {
            get
            {
                return _Logging;
            }
            set
            {
                if (value == null) value = new LoggingSettings();
                _Logging = value;
            }
        }

        /// <summary>
        /// Serialization helper.
        /// </summary>
        public ISerializer Serializer
        {
            get
            {
                return _Serializer;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Serializer));
                _Serializer = value;
            }
        }

        #endregion

        #region Private-Members

        private LoggingSettings _Logging = new LoggingSettings();
        private ISerializer _Serializer = new SerializationHelper();

        #endregion

        #region Public-Methods

        #region General

        /// <inheritdoc />
        public abstract void InitializeRepository();

        #endregion

        #region Tenants

        /// <summary>
        /// Create a tenant.
        /// </summary>
        /// <param name="tenant">Tenant.</param>
        /// <returns>Tenant.</returns>
        public abstract TenantMetadata CreateTenant(TenantMetadata tenant);

        /// <summary>
        /// Create a tenant.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Tenant.</returns>
        public abstract TenantMetadata CreateTenant(string name);

        /// <summary>
        /// Read tenants.
        /// </summary>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Tenants.</returns>
        public abstract IEnumerable<TenantMetadata> ReadTenants(
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending, 
            int skip = 0);

        /// <summary>
        /// Read a tenant by GUID.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <returns>Tenant.</returns>
        public abstract TenantMetadata ReadTenant(Guid guid);

        /// <summary>
        /// Update a tenant.
        /// </summary>
        /// <param name="tenant">Tenant.</param>
        /// <returns>Tenant.</returns>
        public abstract TenantMetadata UpdateTenant(TenantMetadata tenant);

        /// <summary>
        /// Delete a tenant.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="force">True to force deletion of users and credentials.</param>
        public abstract void DeleteTenant(Guid guid, bool force = false);

        /// <summary>
        /// Check if a tenant exists by GUID.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsTenant(Guid guid);

        #endregion

        #region Users

        /// <summary>
        /// Create a user.
        /// </summary>
        /// <param name="user">User.</param>
        /// <returns>User.</returns>
        public abstract UserMaster CreateUser(UserMaster user);

        /// <summary>
        /// Create a user.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="firstName">First name.</param>
        /// <param name="lastName">Last name.</param>
        /// <param name="email">Email.</param>
        /// <param name="password">Password.</param>
        /// <returns>User.</returns>
        public abstract UserMaster CreateUser(
            Guid tenantGuid,
            string firstName,
            string lastName,
            string email,
            string password);

        /// <summary>
        /// Read users.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="email">Email.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <returns>Users.</returns>
        public abstract IEnumerable<UserMaster> ReadUsers(
            Guid? tenantGuid,
            string email,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read a user by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>User.</returns>
        public abstract UserMaster ReadUser(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Read tenants associated with a given email address.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <returns>List of tenants.</returns>
        public abstract List<TenantMetadata> ReadUserTenants(string email);

        /// <summary>
        /// Read a user by email.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="email">Email.</param>
        /// <returns>User.</returns>
        public abstract UserMaster ReadUserByEmail(Guid tenantGuid, string email);

        /// <summary>
        /// Update a user.
        /// </summary>
        /// <param name="user">User.</param>
        /// <returns>User.</returns>
        public abstract UserMaster UpdateUser(UserMaster user);

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        public abstract void DeleteUser(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Delete users associated with a tenant.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        public abstract void DeleteUsers(Guid tenantGuid);

        /// <summary>
        /// Check if a user exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsUser(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Check if a user exists by email.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="email">Email.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsUserByEmail(Guid tenantGuid, string email);

        #endregion

        #region Credentials

        /// <summary>
        /// Create a credential.
        /// </summary>
        /// <param name="credential">Credential.</param>
        /// <returns>Credential.</returns>
        public abstract Credential CreateCredential(Credential credential);

        /// <summary>
        /// Create a credential.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="userGuid">User GUID.</param>
        /// <param name="name">Name.</param>
        /// <returns>Credential.</returns>
        public abstract Credential CreateCredential(
            Guid tenantGuid,
            Guid userGuid,
            string name);

        /// <summary>
        /// Read credentials.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="userGuid">User GUID.</param>
        /// <param name="bearerToken">Bearer token.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <returns>Credentials.</returns>
        public abstract IEnumerable<Credential> ReadCredentials(
            Guid? tenantGuid,
            Guid? userGuid,
            string bearerToken,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read a credential by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>Credential.</returns>
        public abstract Credential ReadCredential(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Read a credential by bearer token.
        /// </summary>
        /// <param name="bearerToken">Bearer token.</param>
        /// <returns>Credential.</returns>
        public abstract Credential ReadCredentialByBearerToken(string bearerToken);

        /// <summary>
        /// Update a credential.
        /// </summary>
        /// <param name="cred">Credential.</param>
        /// <returns>Credential.</returns>
        public abstract Credential UpdateCredential(Credential cred);

        /// <summary>
        /// Delete a credential.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        public abstract void DeleteCredential(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Delete credentials associated with a user.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="userGuid">User GUID.</param>
        public abstract void DeleteUserCredentials(Guid tenantGuid, Guid userGuid);

        /// <summary>
        /// Check if a credential exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsCredential(Guid tenantGuid, Guid guid);

        #endregion

        #region Labels

        /// <summary>
        /// Create a label.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <returns>Label.</returns>
        public abstract LabelMetadata CreateLabel(LabelMetadata label);

        /// <summary>
        /// Create a label.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="label">Label.</param>
        /// <returns>Label.</returns>
        public abstract LabelMetadata CreateLabel(
            Guid tenantGuid,
            Guid graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            string label);

        /// <summary>
        /// Create multiple labels.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <returns>Labels.</returns>
        public abstract List<LabelMetadata> CreateMultipleLabels(Guid tenantGuid, Guid graphGuid, List<LabelMetadata> labels);

        /// <summary>
        /// Read labels.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="label">Label.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <returns>Labels.</returns>
        public abstract IEnumerable<LabelMetadata> ReadLabels(
            Guid tenantGuid,
            Guid? graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            string label,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read a label by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>Label.</returns>
        public abstract LabelMetadata ReadLabel(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Update a label.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <returns>Label.</returns>
        public abstract LabelMetadata UpdateLabel(LabelMetadata label);

        /// <summary>
        /// Delete a label.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        public abstract void DeleteLabel(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Delete labels.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">Node GUIDs.</param>
        /// <param name="edgeGuids">Edge GUIDs.</param>
        public abstract void DeleteLabels(Guid tenantGuid, Guid? graphGuid, List<Guid> nodeGuids, List<Guid> edgeGuids);

        /// <summary>
        /// Check if a label exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsLabel(Guid tenantGuid, Guid guid);

        #endregion

        #region Tags

        /// <summary>
        /// Create a tag.
        /// </summary>
        /// <param name="tag">Tag.</param>
        /// <returns>Tag.</returns>
        public abstract TagMetadata CreateTag(TagMetadata tag);

        /// <summary>
        /// Create a tag.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>Tag.</returns>
        public abstract TagMetadata CreateTag(
            Guid tenantGuid,
            Guid graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            string key,
            string value);

        /// <summary>
        /// Create multiple tags.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="tags">Tags.</param>
        /// <returns>Tags.</returns>
        public abstract List<TagMetadata> CreateMultipleTags(Guid tenantGuid, Guid graphGuid, List<TagMetadata> tags);

        /// <summary>
        /// Read tags.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="key">Key.</param>
        /// <param name="val">Value.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <returns>Tags.</returns>
        public abstract IEnumerable<TagMetadata> ReadTags(
            Guid tenantGuid,
            Guid? graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            string key,
            string val,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read a tag by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>Tag.</returns>
        public abstract TagMetadata ReadTag(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Update a tag.
        /// </summary>
        /// <param name="tag">Tag.</param>
        /// <returns>Tag.</returns>
        public abstract TagMetadata UpdateTag(TagMetadata tag);

        /// <summary>
        /// Delete a tag.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        public abstract void DeleteTag(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Delete tags.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">Node GUIDs.</param>
        /// <param name="edgeGuids">Edge GUIDs.</param>
        public abstract void DeleteTags(Guid tenantGuid, Guid? graphGuid, List<Guid> nodeGuids, List<Guid> edgeGuids);

        /// <summary>
        /// Check if a tag exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsTag(Guid tenantGuid, Guid guid);

        #endregion

        #region Vectors

        /// <summary>
        /// Create a vector.
        /// </summary>
        /// <param name="vector">Vector.</param>
        /// <returns>Vector.</returns>
        public abstract VectorMetadata CreateVector(VectorMetadata vector);

        /// <summary>
        /// Create a vector.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="model">Model.</param>
        /// <param name="dimensionality">Dimensionality.</param>
        /// <param name="content">Content.</param>
        /// <param name="embeddings">Embeddings.</param>
        /// <returns>Vector.</returns>
        public abstract VectorMetadata CreateVector(
            Guid tenantGuid,
            Guid graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            string model,
            int dimensionality,
            string content,
            List<float> embeddings);

        /// <summary>
        /// Create multiple vectors.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="vectors">Vectors.</param>
        /// <returns>Vectors.</returns>
        public abstract List<VectorMetadata> CreateMultipleVectors(
            Guid tenantGuid, 
            Guid graphGuid, 
            List<VectorMetadata> vectors);

        /// <summary>
        /// Read vectors.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <returns>Vectors.</returns>
        public abstract IEnumerable<VectorMetadata> ReadVectors(
            Guid tenantGuid,
            Guid? graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read graph vectors.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <returns>Vectors.</returns>
        public abstract IEnumerable<VectorMetadata> ReadGraphVectors(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Read node vectors.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <returns>Vectors.</returns>
        public abstract IEnumerable<VectorMetadata> ReadNodeVectors(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Read edge vectors.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <returns>Vectors.</returns>
        public abstract IEnumerable<VectorMetadata> ReadEdgeVectors(Guid tenantGuid, Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Read a vector by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>Vector.</returns>
        public abstract VectorMetadata ReadVector(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Update a vector.
        /// </summary>
        /// <param name="vector">Vector.</param>
        /// <returns>Vector.</returns>
        public abstract VectorMetadata UpdateVector(VectorMetadata vector);

        /// <summary>
        /// Delete a vector.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        public abstract void DeleteVector(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Delete vectors.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">Node GUIDs.</param>
        /// <param name="edgeGuids">Edge GUIDs.</param>
        public abstract void DeleteVectors(Guid tenantGuid, Guid? graphGuid, List<Guid> nodeGuids, List<Guid> edgeGuids);

        /// <summary>
        /// Check if a vector exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsVector(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Search graph vectors.
        /// </summary>
        /// <param name="searchType">Vector search type.</param>
        /// <param name="vectors">Vectors.</param>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="filter">Filter.</param>
        /// <returns>Vector search results containing graphs.</returns>
        public abstract IEnumerable<VectorSearchResult> SearchGraphVectors(
            VectorSearchTypeEnum searchType,
            List<float> vectors,
            Guid tenantGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr filter = null);

        /// <summary>
        /// Search node vectors.
        /// </summary>
        /// <param name="searchType">Vector search type.</param>
        /// <param name="vectors">Vectors.</param>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="filter">Filter.</param>
        /// <returns>Vector search results containing nodes.</returns>
        public abstract IEnumerable<VectorSearchResult> SearchNodeVectors(
            VectorSearchTypeEnum searchType,
            List<float> vectors,
            Guid tenantGuid,
            Guid graphGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr filter = null);

        /// <summary>
        /// Search edge vectors.
        /// </summary>
        /// <param name="searchType">Vector search type.</param>
        /// <param name="vectors">Vectors.</param>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="filter">Filter.</param>
        /// <returns>Vector search results containing edges.</returns>
        public abstract IEnumerable<VectorSearchResult> SearchEdgeVectors(
            VectorSearchTypeEnum searchType,
            List<float> vectors,
            Guid tenantGuid,
            Guid graphGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr filter = null);

        #endregion

        #region Graphs

        /// <summary>
        /// Create a graph.
        /// </summary>
        /// <param name="graph">Graph.</param>
        /// <returns>Graph.</returns>
        public abstract Graph CreateGraph(Graph graph);

        /// <summary>
        /// Create a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="name">Unique name.</param>
        /// <param name="data">Data.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="vectors">Vectors.</param>
        /// <returns>Graph.</returns>
        public abstract Graph CreateGraph(
            Guid tenantGuid, 
            Guid guid, 
            string name, 
            object data = null, 
            List<string> labels = null,
            NameValueCollection tags = null,
            List<VectorMetadata> vectors = null);

        /// <summary>
        /// Read graphs.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags on which to match.</param>
        /// <param name="graphFilter">
        /// Graph filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Graphs.</returns>
        public abstract IEnumerable<Graph> ReadGraphs(
            Guid tenantGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr graphFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read a graph by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>Graph.</returns>
        public abstract Graph ReadGraph(Guid tenantGuid, Guid guid);

        /// <summary>
        /// Update a graph.
        /// </summary>
        /// <param name="graph">Graph.</param>
        /// <returns>Graph.</returns>
        public abstract Graph UpdateGraph(Graph graph);

        /// <summary>
        /// Delete a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="force">True to force deletion of nodes and edges.</param>
        public abstract void DeleteGraph(Guid tenantGuid, Guid guid, bool force = false);

        /// <summary>
        /// Delete graphs associated with a tenant.  Deletion is forceful.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        public abstract void DeleteGraphs(Guid tenantGuid);

        /// <summary>
        /// Delete all labels associated with a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteAllGraphLabels(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Delete labels for the graph object itself, leaving subordinate node and edge labels in place.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteGraphLabels(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Delete all tags associated with a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteAllGraphTags(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Delete tags for the graph object itself, leaving subordinate node and edge tags in place.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteGraphTags(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Delete all vectors associated with a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteAllGraphVectors(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Delete vectors for the graph object itself, leaving subordinate node and edge tags in place.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteGraphVectors(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Check if a graph exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsGraph(Guid tenantGuid, Guid guid);

        #endregion

        #region Nodes

        /// <summary>
        /// Create a node.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <returns>Node.</returns>
        public abstract Node CreateNode(Node node);

        /// <summary>
        /// Create a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="vectors">Vectors.</param>
        /// <returns>Node.</returns>
        public abstract Node CreateNode(
            Guid tenantGuid, 
            Guid graphGuid, 
            Guid guid, 
            string name, 
            object data = null, 
            List<string> labels = null,
            NameValueCollection tags = null,
            List<VectorMetadata> vectors = null);

        /// <summary>
        /// Create multiple nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodes">Nodes.</param>
        /// <returns>Nodes.</returns>
        public abstract List<Node> CreateMultipleNodes(Guid tenantGuid, Guid graphGuid, List<Node> nodes);

        /// <summary>
        /// Read nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="nodeFilter">
        /// Node filter expression for Data JSON body.  
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> ReadNodes(
            Guid tenantGuid,
            Guid graphGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <returns>Node.</returns>
        public abstract Node ReadNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Update node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Node.</returns>
        public abstract Node UpdateNode(Node node);

        /// <summary>
        /// Delete a node and all associated edges.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        public abstract void DeleteNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Delete all nodes from a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteNodes(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Delete multiple nodes from a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">Node GUIDs.</param>
        public abstract void DeleteNodes(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids);

        /// <summary>
        /// Delete node labels.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        public abstract void DeleteNodeLabels(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Delete node tags.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        public abstract void DeleteNodeTags(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Delete vectors for a node object.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        public abstract void DeleteNodeVectors(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Check existence of a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        #endregion

        #region Edges

        /// <summary>
        /// Create an edge between two nodes.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <returns>Edge.</returns>
        public abstract Edge CreateEdge(Edge edge);

        /// <summary>
        /// Create an edge between two nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="fromGuid">From GUID.</param>
        /// <param name="toGuid">To GUID.</param>
        /// <param name="name">Name.</param>
        /// <param name="cost">Cost.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="data">Data.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="vectors">Vectors.</param>
        /// <returns>Edge.</returns>
        public abstract Edge CreateEdge(
            Guid tenantGuid, 
            Guid graphGuid, 
            Guid guid, 
            Guid fromGuid, 
            Guid toGuid, 
            string name, 
            int cost = 0, 
            object data = null, 
            List<string> labels = null,
            NameValueCollection tags = null,
            List<VectorMetadata> vectors = null);

        /// <summary>
        /// Create multiple edges.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edges">Edges.</param>
        /// <returns>Edges.</returns>
        public abstract List<Edge> CreateMultipleEdges(Guid tenantGuid, Guid graphGuid, List<Edge> edges);

        /// <summary>
        /// Read edges.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> ReadEdges(
            Guid tenantGuid,
            Guid graphGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Read edge.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <returns>Edge.</returns>
        public abstract Edge ReadEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Update edge.
        /// </summary>
        /// <param name="edge">Edge.</param>
        /// <returns>Edge.</returns>
        public abstract Edge UpdateEdge(Edge edge);

        /// <summary>
        /// Delete edge.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        public abstract void DeleteEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Delete all edges from a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        public abstract void DeleteEdges(Guid tenantGuid, Guid graphGuid);

        /// <summary>
        /// Delete all edges associated with a given node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        public abstract void DeleteNodeEdges(Guid tenantGuid, Guid graphGuid, Guid nodeGuid);

        /// <summary>
        /// Delete all edges associated with a set of given nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">Node GUIDs.</param>
        public abstract void DeleteNodeEdges(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids);

        /// <summary>
        /// Delete multiple edges from a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuids">Edge GUIDs.</param>
        public abstract void DeleteEdges(Guid tenantGuid, Guid graphGuid, List<Guid> edgeGuids);

        /// <summary>
        /// Delete edge labels.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        public abstract void DeleteEdgeLabels(Guid tenantGuid, Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Delete edge tags.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        public abstract void DeleteEdgeTags(Guid tenantGuid, Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Delete vectors for an edge object.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        public abstract void DeleteEdgeVectors(Guid tenantGuid, Guid graphGuid, Guid edgeGuid);

        /// <summary>
        /// Check if an edge exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <returns>True if exists.</returns>
        public abstract bool ExistsEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid);

        #endregion

        #region Batch

        /// <summary>
        /// Batch existence request.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="req">Existence request.</param>
        /// <returns>Existence result.</returns>
        public abstract ExistenceResult BatchExistence(Guid tenantGuid, Guid graphGuid, ExistenceRequest req);

        #endregion

        #region Routes-and-Traversal

        /// <summary>
        /// Get nodes that have edges connecting to the specified node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> GetParents(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Get nodes to which the specified node has connecting edges connecting.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> GetChildren(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Get neighbors for a given node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Nodes.</returns>
        public abstract IEnumerable<Node> GetNeighbors(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Get routes between two nodes.
        /// </summary>
        /// <param name="searchType">Search type.</param>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="nodeFilter">
        /// Node filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <returns>Route details.</returns>
        public abstract IEnumerable<RouteDetail> GetRoutes(
            SearchTypeEnum searchType,
            Guid tenantGuid,
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null);

        /// <summary>
        /// Get edges connected to or initiated from a given node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags upon which to filter edges.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetConnectedEdges(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Get edges from a given node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags upon which to filter edges.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetEdgesFrom(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Get edges to a given node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags upon which to filter edges.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetEdgesTo(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        /// <summary>
        /// Get edges between two neighboring nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags upon which to filter edges.</param>
        /// <param name="edgeFilter">
        /// Edge filter expression for Data JSON body.
        /// Expression left terms must follow the form of Sqlite JSON paths.
        /// For example, to retrieve the 'Name' property, use '$.Name', OperatorEnum.Equals, '[name here]'.</param>
        /// <param name="order">Enumeration order.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <returns>Edges.</returns>
        public abstract IEnumerable<Edge> GetEdgesBetween(
            Guid tenantGuid,
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            List<string> labels = null,
            NameValueCollection tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0);

        #endregion

        #endregion

        #region Private-Methods

        #endregion
    }
}
