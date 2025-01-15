namespace LiteGraph.Sdk
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// LiteGraph SDK. 
    /// </summary>
    public class LiteGraphSdk : SdkBase
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="endpoint">Endpoint URL.</param>
        /// <param name="bearerToken">Bearer token.</param>
        public LiteGraphSdk(
            string endpoint = "http://localhost:8701/",
            string bearerToken = "default") : base(endpoint, bearerToken)
        {

        }

        #endregion

        #region Public-Methods

        #region General-Routes

        #endregion

        #region Tenants

        /// <summary>
        /// Check if a tenant exists by GUID.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> TenantExists(Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a tenant.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="name">Name.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tenant.</returns>
        public async Task<TenantMetadata> CreateTenant(Guid guid, string name, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            string url = Endpoint + "v1.0/tenants";
            return await PutCreate<TenantMetadata>(url, new TenantMetadata { GUID = guid, Name = name }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read tenants.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tenants.</returns>
        public async Task<IEnumerable<TenantMetadata>> ReadTenants(CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants";
            return await GetMany<TenantMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read tenant.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tenant.</returns>
        public async Task<TenantMetadata> ReadTenant(Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + guid;
            return await Get<TenantMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a tenant.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenant">Tenant.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tenant.</returns>
        public async Task<TenantMetadata> UpdateTenant(TenantMetadata tenant, CancellationToken token = default)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            string url = Endpoint + "v1.0/tenants/" + tenant.GUID;
            return await PutUpdate<TenantMetadata>(url, tenant, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a tenant.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="force">Force recursive deletion of subordinate objects.  The request will otherwise fail if nodes exist in the graph.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteTenant(Guid guid, bool force = false, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + guid;
            if (force) url += "?force";
            await Delete(url, token).ConfigureAwait(false);
        }

        #endregion

        #region Users

        /// <summary>
        /// Check if a user exists by GUID.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary> 
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> UserExists(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/users/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a user.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="user">User.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>User.</returns>
        public async Task<UserMaster> CreateUser(Guid tenantGuid, UserMaster user, CancellationToken token = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/users";
            return await PutCreate<UserMaster>(url, user, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read users.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Users.</returns>
        public async Task<IEnumerable<UserMaster>> ReadUsers(Guid tenantGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/users";
            return await GetMany<UserMaster>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read user.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>User.</returns>
        public async Task<UserMaster> ReadUser(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/users/" + guid;
            return await Get<UserMaster>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a user.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="user">User.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>User.</returns>
        public async Task<UserMaster> UpdateUser(Guid tenantGuid, UserMaster user, CancellationToken token = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/users/" + user.GUID;
            return await PutUpdate<UserMaster>(url, user, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a user.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteUser(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/users/" + guid;
            await Delete(url, token).ConfigureAwait(false);
        }

        #endregion

        #region Credentials

        /// <summary>
        /// Check if a credential exists by GUID.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary> 
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> CredentialExists(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/credentials/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a credential.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="credential">Credential.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Credential.</returns>
        public async Task<Credential> CreateCredential(Guid tenantGuid, Credential credential, CancellationToken token = default)
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/credentials";
            return await PutCreate<Credential>(url, credential, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read credentials.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Credentials.</returns>
        public async Task<IEnumerable<Credential>> ReadCredentials(Guid tenantGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/credentials";
            return await GetMany<Credential>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read credential.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Credential.</returns>
        public async Task<Credential> ReadCredential(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/credentials/" + guid;
            return await Get<Credential>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a credential.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="credential">Credential.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Credential.</returns>
        public async Task<Credential> UpdateCredential(Guid tenantGuid, Credential credential, CancellationToken token = default)
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/credentials/" + credential.GUID;
            return await PutUpdate<Credential>(url, credential, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a credential.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteCredential(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/credentials/" + guid;
            await Delete(url, token).ConfigureAwait(false);
        }

        #endregion

        #region Labels

        /// <summary>
        /// Check if a label exists by GUID.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary> 
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> LabelExists(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/labels/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a label.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="label">Label.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Label.</returns>
        public async Task<LabelMetadata> CreateLabel(Guid tenantGuid, LabelMetadata label, CancellationToken token = default)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/labels";
            return await PutCreate<LabelMetadata>(url, label, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read labels.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Labels.</returns>
        public async Task<IEnumerable<LabelMetadata>> ReadLabels(Guid tenantGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/labels";
            return await GetMany<LabelMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read label.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Label.</returns>
        public async Task<LabelMetadata> ReadLabel(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/labels/" + guid;
            return await Get<LabelMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a label.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="label">Label.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Label.</returns>
        public async Task<LabelMetadata> UpdateLabel(Guid tenantGuid, LabelMetadata label, CancellationToken token = default)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/labels/" + label.GUID;
            return await PutUpdate<LabelMetadata>(url, label, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a label.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteLabel(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/labels/" + guid;
            await Delete(url, token).ConfigureAwait(false);
        }

        #endregion

        #region Tags

        /// <summary>
        /// Check if a tag exists by GUID.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary> 
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> TagExists(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/tags/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a tag.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="tag">Tag.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tag.</returns>
        public async Task<TagMetadata> CreateTag(Guid tenantGuid, TagMetadata tag, CancellationToken token = default)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/tags";
            return await PutCreate<TagMetadata>(url, tag, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read tags.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tags.</returns>
        public async Task<IEnumerable<TagMetadata>> ReadTags(Guid tenantGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/tags";
            return await GetMany<TagMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read tag.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tag.</returns>
        public async Task<TagMetadata> ReadTag(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/tags/" + guid;
            return await Get<TagMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a tag.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="tag">Tag.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tag.</returns>
        public async Task<TagMetadata> UpdateTag(Guid tenantGuid, TagMetadata tag, CancellationToken token = default)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/tags/" + tag.GUID;
            return await PutUpdate<TagMetadata>(url, tag, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a tag.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteTag(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/tags/" + guid;
            await Delete(url, token).ConfigureAwait(false);
        }

        #endregion

        #region Vectors

        /// <summary>
        /// Check if a vector exists by GUID.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary> 
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> VectorExists(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/vectors/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a vector.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="vector">Vector.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Vector.</returns>
        public async Task<VectorMetadata> CreateVector(Guid tenantGuid, VectorMetadata vector, CancellationToken token = default)
        {
            if (vector == null) throw new ArgumentNullException(nameof(vector));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/vectors";
            return await PutCreate<VectorMetadata>(url, vector, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read vectors.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Vectors.</returns>
        public async Task<IEnumerable<VectorMetadata>> ReadVectors(Guid tenantGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/vectors";
            return await GetMany<VectorMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read vector.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Vector.</returns>
        public async Task<VectorMetadata> ReadVector(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/vectors/" + guid;
            return await Get<VectorMetadata>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a vector.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="vector">Vector.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Vector.</returns>
        public async Task<VectorMetadata> UpdateVector(Guid tenantGuid, VectorMetadata vector, CancellationToken token = default)
        {
            if (vector == null) throw new ArgumentNullException(nameof(vector));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/vectors/" + vector.GUID;
            return await PutUpdate<VectorMetadata>(url, vector, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a vector.
        /// This is an administrative API, requring use of the admin bearer token.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteVector(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/vectors/" + guid;
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Search vectors.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="searchReq">Search request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Vector search result.</returns>
        public async Task<List<VectorSearchResult>> SearchVectors(Guid tenantGuid, Guid? graphGuid, VectorSearchRequest searchReq, CancellationToken token = default)
        {
            if (searchReq == null) throw new ArgumentNullException(nameof(searchReq));

            searchReq.TenantGUID = tenantGuid;
            searchReq.GraphGUID = graphGuid;

            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/vectors";
            string json = Serializer.SerializeJson(searchReq, true);
            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(json), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                List<VectorSearchResult> result = Serializer.DeserializeJson<List<VectorSearchResult>>(Encoding.UTF8.GetString(bytes));
                return result;
            }

            return null;
        }

        #endregion

        #region Graph

        /// <summary>
        /// Check if a graph exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> GraphExists(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="name">Name.</param>
        /// <param name="labels">Labels.</param>
        /// <param name="tags">Tags.</param>
        /// <param name="data">Data.</param>
        /// <param name="vectors">Vectors.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graph.</returns>
        public async Task<Graph> CreateGraph(
            Guid tenantGuid, 
            Guid guid, 
            string name, 
            List<string> labels = null,
            NameValueCollection tags = null,
            object data = null, 
            List<VectorMetadata> vectors = null,
            CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs";
            return await PutCreate<Graph>(url, new Graph 
            { 
                GUID = guid, 
                Name = name, 
                Labels = labels, 
                Tags = tags, 
                Data = data,
                Vectors = vectors
            }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read graphs.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graphs.</returns>
        public async Task<IEnumerable<Graph>> ReadGraphs(Guid tenantGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs";
            return await GetMany<Graph>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Search graphs.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="searchReq">Search request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Search result.</returns>
        public async Task<SearchResult> SearchGraphs(Guid tenantGuid, SearchRequest searchReq, CancellationToken token = default)
        {
            if (searchReq == null) throw new ArgumentNullException(nameof(searchReq));

            searchReq.TenantGUID = tenantGuid;

            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/search";
            string json = Serializer.SerializeJson(searchReq, true);
            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(json), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                SearchResult result = Serializer.DeserializeJson<SearchResult>(Encoding.UTF8.GetString(bytes));
                return result;
            }

            return null;
        }

        /// <summary>
        /// Read graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graph.</returns>
        public async Task<Graph> ReadGraph(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + guid;
            return await Get<Graph>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graph">Graph.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Graph.</returns>
        public async Task<Graph> UpdateGraph(Guid tenantGuid, Graph graph, CancellationToken token = default)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graph.GUID;
            return await PutUpdate<Graph>(url, graph, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="force">Force recursive deletion of edges and nodes.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteGraph(Guid tenantGuid, Guid guid, bool force = false, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + guid;
            if (force) url += "?force";
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Export a graph to GEXF format.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>String containing GEXF XML data.</returns>
        public async Task<string> ExportGraphToGexf(Guid tenantGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + guid + "/export/gexf";
            byte[] bytes = await Get(url, token).ConfigureAwait(false);
            if (bytes != null && bytes.Length > 0) return Encoding.UTF8.GetString(bytes);
            return null;
        }

        /// <summary>
        /// Execute a batch existence request.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="req">Existence request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Existence result.</returns>
        public async Task<ExistenceResult> BatchExistence(Guid tenantGuid, Guid graphGuid, ExistenceRequest req, CancellationToken token = default)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/existence";
            return await Post<ExistenceRequest, ExistenceResult>(url, req, token).ConfigureAwait(false);
        }

        #endregion

        #region Node

        /// <summary>
        /// Check if a node exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> NodeExists(Guid tenantGuid, Guid graphGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create multiple nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodes">Nodes.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<List<Node>> CreateNodes(Guid tenantGuid, Guid graphGuid, List<Node> nodes, CancellationToken token = default)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (nodes.Count < 1) return new List<Node>();
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/multiple";
            return await PutCreate<List<Node>>(url, nodes, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="node">Node.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Node.</returns>
        public async Task<Node> CreateNode(Guid tenantGuid, Guid graphGuid, Node node, CancellationToken token = default)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes";
            return await PutCreate<Node>(url, node, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> ReadNodes(Guid tenantGuid, Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Search nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="searchReq">Search request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Search result.</returns>
        public async Task<SearchResult> SearchNodes(Guid tenantGuid, Guid graphGuid, SearchRequest searchReq, CancellationToken token = default)
        {
            if (searchReq == null) throw new ArgumentNullException(nameof(searchReq));

            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/search";

            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(Serializer.SerializeJson(searchReq, true)), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                SearchResult result = Serializer.DeserializeJson<SearchResult>(Encoding.UTF8.GetString(bytes));
                return result;
            }

            return null;
        }

        /// <summary>
        /// Read a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Node.</returns>
        public async Task<Node> ReadNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid;
            return await Get<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="node">Node.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Node.</returns>
        public async Task<Node> UpdateNode(Guid tenantGuid, Guid graphGuid, Node node, CancellationToken token = default)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + node.GUID;
            return await PutUpdate<Node>(url, node, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid;
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete all nodes within a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteNodes(Guid tenantGuid, Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/all";
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete multiple nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuids">List of node GUIDs.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteNodes(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids, CancellationToken token = default)
        {
            if (nodeGuids == null) throw new ArgumentNullException(nameof(nodeGuids));
            if (nodeGuids.Count < 1) return;
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/multiple";
            await Delete<List<Guid>>(url, nodeGuids, token).ConfigureAwait(false);
        }

        #endregion

        #region Edge

        /// <summary>
        /// Check if an edge exists by GUID.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> EdgeExists(Guid tenantGuid, Guid graphGuid, Guid guid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/" + guid;
            return await Head(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create multiple edges.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edges">Edges.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<List<Edge>> CreateEdges(Guid tenantGuid, Guid graphGuid, List<Edge> edges, CancellationToken token = default)
        {
            if (edges == null) throw new ArgumentNullException(nameof(edges));
            if (edges.Count < 1) return new List<Edge>();
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/multiple";
            return await PutCreate<List<Edge>>(url, edges, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create an edge.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edge">Edge.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edge.</returns>
        public async Task<Edge> CreateEdge(Guid tenantGuid, Guid graphGuid, Edge edge, CancellationToken token = default)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges";
            return await PutCreate<Edge>(url, edge, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read edges.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> ReadEdges(Guid tenantGuid, Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Search edges.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="searchReq">Search request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Search result.</returns>
        public async Task<SearchResult> SearchEdges(Guid tenantGuid, Guid graphGuid, SearchRequest searchReq, CancellationToken token = default)
        {
            if (searchReq == null) throw new ArgumentNullException(nameof(searchReq));

            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/search";

            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(Serializer.SerializeJson(searchReq, true)), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                SearchResult result = Serializer.DeserializeJson<SearchResult>(Encoding.UTF8.GetString(bytes));
                return result;
            }

            return null;
        }

        /// <summary>
        /// Read an edge.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid"></param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edge.</returns>
        public async Task<Edge> ReadEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/" + edgeGuid;
            return await Get<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Update an edge.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edge">Edge.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edge.</returns>
        public async Task<Edge> UpdateEdge(Guid tenantGuid, Guid graphGuid, Edge edge, CancellationToken token = default)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/" + edge.GUID;
            return await PutUpdate<Edge>(url, edge, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete an edge.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuid"></param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/" + edgeGuid;
            await Delete(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete multiple edges within a graph.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="edgeGuids">Edge GUIDs.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteEdges(Guid tenantGuid, Guid graphGuid, List<Guid> edgeGuids, CancellationToken token = default)
        {
            if (edgeGuids == null) throw new ArgumentNullException(nameof(edgeGuids));
            if (edgeGuids.Count < 1) return;
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/multiple";
            await Delete<List<Guid>>(url, edgeGuids, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete all edges.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DeleteEdges(Guid tenantGuid, Guid graphGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/all";
            await Delete(url, token).ConfigureAwait(false);
        }

        #endregion

        #region Routes-and-Traversal

        /// <summary>
        /// Get edges from a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetEdgesFromNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/edges/from";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get edges to a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetEdgesToNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/edges/to";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get edges from a given node to a given node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetEdgesBetween(
            Guid tenantGuid,
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/edges/between?from=" + fromNodeGuid + "&to=" + toNodeGuid;
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all edges to or from a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Edges.</returns>
        public async Task<IEnumerable<Edge>> GetAllNodeEdges(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/edges";
            return await GetMany<Edge>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get child nodes from a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> GetChildrenFromNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/children";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get parent nodes from a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> GetParentsFromNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/parents";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get neighboring nodes from a node.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Nodes.</returns>
        public async Task<IEnumerable<Node>> GetNodeNeighbors(Guid tenantGuid, Guid graphGuid, Guid nodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/nodes/" + nodeGuid + "/neighbors";
            return await GetMany<Node>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get routes between two nodes.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="fromNodeGuid">From node GUID.</param>
        /// <param name="toNodeGuid">To node GUID.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Routes.</returns>
        public async Task<RouteResult> GetRoutes(Guid tenantGuid, Guid graphGuid, Guid fromNodeGuid, Guid toNodeGuid, CancellationToken token = default)
        {
            string url = Endpoint + "v1.0/tenants/" + tenantGuid + "/graphs/" + graphGuid + "/routes";
            
            RouteRequest req = new RouteRequest
            {
                Graph = graphGuid,
                From = fromNodeGuid,
                To = toNodeGuid
            };

            byte[] bytes = await PostRaw(url, Encoding.UTF8.GetBytes(Serializer.SerializeJson(req, true)), "application/json", token).ConfigureAwait(false);

            if (bytes != null && bytes.Length > 0)
            {
                RouteResult resp = Serializer.DeserializeJson<RouteResult>(Encoding.UTF8.GetString(bytes));
                return resp;
            }

            return null;
        }

        #endregion

        #endregion

        #region Private-Methods

        #endregion
    }
}
