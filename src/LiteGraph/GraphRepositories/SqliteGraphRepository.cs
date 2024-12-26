namespace LiteGraph.GraphRepositories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Caching;
    using ExpressionTree;
    using LiteGraph;
    using LiteGraph.Serialization;
    using Microsoft.Data.Sqlite;
    using PrettyId;

    /// <summary>
    /// Sqlite graph repository.
    /// </summary>
    public class SqliteGraphRepository : GraphRepositoryBase
    {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

        // Helpful references for Sqlite JSON:
        // https://stackoverflow.com/questions/33432421/sqlite-json1-example-for-json-extract-set
        // https://www.sqlite.org/json1.html

        #region Public-Members

        /// <summary>
        /// Sqlite database filename.
        /// </summary>
        public string Filename
        {
            get
            {
                return _Filename;
            }
        }

        /// <summary>
        /// Maximum supported statement length.
        /// Default for Sqlite is 1000000 (see https://www.sqlite.org/limits.html).
        /// </summary>
        public int MaxStatementLength
        {
            get
            {
                return _MaxStatementLength;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxStatementLength));
                _MaxStatementLength = value;
            }
        }

        /// <summary>
        /// Number of records to retrieve for object list retrieval.
        /// </summary>
        public int SelectBatchSize
        {
            get
            {
                return _SelectBatchSize;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(SelectBatchSize));
                _SelectBatchSize = value;
            }
        }

        /// <summary>
        /// Timestamp format.
        /// </summary>
        public string TimestampFormat
        {
            get
            {
                return _TimestampFormat;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(TimestampFormat));
                string test = DateTime.UtcNow.ToString(value);
                _TimestampFormat = value;
            }
        }

        /// <summary>
        /// True to enable a full-text index over node and edge data properties.
        /// </summary>
        public bool IndexData { get; set; } = true;

        #endregion

        #region Private-Members

        private string _Filename = "litegraph.db";
        private string _ConnectionString = "Data Source=litegraph.db;Pooling=false";
        private int _MaxStatementLength = 1000000; // https://www.sqlite.org/limits.html
        private int _SelectBatchSize = 100;
        private string _TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";
        private readonly object _QueryLock = new object();
        private readonly object _CreateLock = new object();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="filename">Sqlite database filename.</param>
        public SqliteGraphRepository(string filename = "litegraph.db")
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            _Filename = filename;
            _ConnectionString = "Data Source=" + filename + ";Pooling=false";
        }

        #endregion

        #region Public-Methods

        #region General

        /// <inheritdoc />
        public override void InitializeRepository()
        {
            CreateTablesAndIndices();
        }

        #endregion

        #region Tenants

        /// <inheritdoc />
        public override TenantMetadata CreateTenant(TenantMetadata tenant)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));

            string createQuery = InsertTenantQuery(tenant);
            DataTable createResult = null;
            TenantMetadata created = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
            }

            created = TenantFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override TenantMetadata CreateTenant(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            string createQuery = InsertTenantQuery(new TenantMetadata
            {
                Name = name,
                CreatedUtc = DateTime.UtcNow
            });

            DataTable createResult = null;
            TenantMetadata created = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
            }

            created = TenantFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override void DeleteTenant(Guid guid, bool force = false)
        {
            TenantMetadata tenant = ReadTenant(guid);
            if (tenant != null)
            {
                if (force)
                {
                    DeleteUsers(tenant.GUID);
                    DeleteGraphs(tenant.GUID);
                }

                Query(DeleteTenantQuery(guid), true);
            }
        }

        /// <inheritdoc />
        public override bool ExistsTenant(Guid guid)
        {
            if (ReadTenant(guid) != null) return true;
            return false;
        }

        /// <inheritdoc />
        public override TenantMetadata ReadTenant(Guid guid)
        {
            DataTable result = Query(SelectTenantQuery(guid));
            if (result != null && result.Rows.Count == 1) return TenantFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override IEnumerable<TenantMetadata> ReadTenants(EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending, int skip = 0)
        {
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectTenantsQuery(skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    yield return TenantFromDataRow(result.Rows[i]);
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override TenantMetadata UpdateTenant(TenantMetadata tenant)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            return TenantFromDataRow(Query(UpdateTenantQuery(tenant), true).Rows[0]);
        }

        #endregion

        #region Users

        /// <inheritdoc />
        public override UserMaster CreateUser(UserMaster user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            ValidateTenantExists(user.TenantGUID);

            TenantMetadata tenant = ReadTenant(user.TenantGUID);
            if (tenant == null) throw new KeyNotFoundException("The specified tenant could not be found.");

            string createQuery = InsertUserQuery(user);
            DataTable createResult = null;
            UserMaster created = null;
            UserMaster existing = null;

            lock (_CreateLock)
            {
                existing = ReadUserByEmail(user.TenantGUID, user.Email);
                if (existing != null) return existing;
                createResult = Query(createQuery, true);
            }

            created = UserFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override UserMaster CreateUser(Guid tenantGuid, string firstName, string lastName, string email, string password)
        {
            if (String.IsNullOrEmpty(firstName)) throw new ArgumentNullException(nameof(firstName));
            if (String.IsNullOrEmpty(lastName)) throw new ArgumentNullException(nameof(lastName));
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (String.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            ValidateTenantExists(tenantGuid);

            TenantMetadata tenant = ReadTenant(tenantGuid);
            if (tenant == null) throw new KeyNotFoundException("The specified tenant could not be found.");

            string createQuery = InsertUserQuery(new UserMaster
            {
                TenantGUID = tenantGuid,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                CreatedUtc = DateTime.UtcNow
            });

            DataTable createResult = null;
            UserMaster created = null;
            UserMaster existing = null;

            lock (_CreateLock)
            {
                existing = ReadUserByEmail(tenantGuid, email);
                if (existing != null) return existing;
                createResult = Query(createQuery, true);
            }

            created = UserFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override void DeleteUser(Guid tenantGuid, Guid guid)
        {
            UserMaster user = ReadUser(tenantGuid, guid);
            if (user != null)
            {
                DeleteCredentials(tenantGuid, guid);
                Query(DeleteUserQuery(tenantGuid, guid), true);
            }
        }

        /// <inheritdoc />
        public override void DeleteUsers(Guid tenantGuid)
        {
            List<UserMaster> users = ReadUsers(tenantGuid, null).ToList();
            if (users != null && users.Count > 0)
            {
                foreach (UserMaster user in users)
                {
                    DeleteUser(user.TenantGUID, user.GUID);
                }
            }
        }

        /// <inheritdoc />
        public override bool ExistsUser(Guid tenantGuid, Guid guid)
        {
            if (ReadUser(tenantGuid, guid) != null) return true;
            return false;
        }

        /// <inheritdoc />
        public override bool ExistsUserByEmail(Guid tenantGuid, string email)
        {
            if (ReadUserByEmail(tenantGuid, email) != null) return true;
            return false;
        }

        /// <inheritdoc />
        public override UserMaster ReadUser(Guid tenantGuid, Guid guid)
        {
            DataTable result = Query(SelectUserQuery(tenantGuid, guid));
            if (result != null && result.Rows.Count == 1) return UserFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override UserMaster ReadUserByEmail(Guid tenantGuid, string email)
        {
            DataTable result = Query(SelectUserQuery(tenantGuid, email));
            if (result != null && result.Rows.Count == 1) return UserFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override IEnumerable<UserMaster> ReadUsers(
            Guid? tenantGuid,
            string email,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectUsersQuery(tenantGuid, email, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    yield return UserFromDataRow(result.Rows[i]);
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override UserMaster UpdateUser(UserMaster user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            ValidateTenantExists(user.TenantGUID);
            return UserFromDataRow(Query(UpdateUserQuery(user), true).Rows[0]);
        }

        #endregion

        #region Credentials

        /// <inheritdoc />
        public override Credential CreateCredential(Credential cred)
        {
            if (cred == null) throw new ArgumentNullException(nameof(cred));

            ValidateTenantExists(cred.TenantGUID);
            ValidateUserExists(cred.TenantGUID, cred.UserGUID);

            TenantMetadata tenant = ReadTenant(cred.TenantGUID);
            if (tenant == null) throw new KeyNotFoundException("The specified tenant could not be found.");

            UserMaster user = ReadUser(cred.TenantGUID, cred.UserGUID);
            if (user == null) throw new KeyNotFoundException("The specified user could not be found.");

            string createQuery = InsertCredentialQuery(cred);
            DataTable createResult = null;
            Credential created = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
            }

            created = CredentialFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override Credential CreateCredential(
            Guid tenantGuid,
            Guid userGuid,
            string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            ValidateTenantExists(tenantGuid);
            ValidateUserExists(tenantGuid, userGuid);

            TenantMetadata tenant = ReadTenant(tenantGuid);
            if (tenant == null) throw new KeyNotFoundException("The specified tenant could not be found.");

            UserMaster user = ReadUser(tenantGuid, userGuid);
            if (user == null) throw new KeyNotFoundException("The specified user could not be found.");

            string createQuery = InsertCredentialQuery(new Credential
            {
                TenantGUID = tenantGuid,
                UserGUID = userGuid,
                Name = name,
                BearerToken = IdGenerator.Generate(64),
                CreatedUtc = DateTime.UtcNow
            });

            DataTable createResult = null;
            Credential created = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
            }

            created = CredentialFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override void DeleteCredential(Guid tenantGuid, Guid guid)
        {
            Credential cred = ReadCredential(tenantGuid, guid);
            if (cred != null)
            {
                Query(DeleteCredentialQuery(tenantGuid, guid), true);
            }
        }

        /// <inheritdoc />
        public override void DeleteCredentials(Guid tenantGuid, Guid userGuid)
        {
            List<Credential> creds = ReadCredentials(tenantGuid, userGuid, null).ToList();
            if (creds != null && creds.Count > 0)
            {
                foreach (Credential cred in creds) DeleteCredential(cred.TenantGUID, cred.GUID);
            }
        }

        /// <inheritdoc />
        public override bool ExistsCredential(Guid tenantGuid, Guid guid)
        {
            if (ReadCredential(tenantGuid, guid) != null) return true;
            return false;
        }

        /// <inheritdoc />
        public override Credential ReadCredential(Guid tenantGuid, Guid guid)
        {
            DataTable result = Query(SelectCredentialQuery(tenantGuid, guid));
            if (result != null && result.Rows.Count == 1) return CredentialFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override Credential ReadCredentialByBearerToken(string bearerToken)
        {
            DataTable result = Query(SelectCredentialQuery(bearerToken));
            if (result != null && result.Rows.Count == 1) return CredentialFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override IEnumerable<Credential> ReadCredentials(
            Guid? tenantGuid,
            Guid? userGuid,
            string bearerToken,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectCredentialsQuery(tenantGuid, userGuid, bearerToken, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    yield return CredentialFromDataRow(result.Rows[i]);
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override Credential UpdateCredential(Credential cred)
        {
            if (cred == null) throw new ArgumentNullException(nameof(cred));
            ValidateTenantExists(cred.TenantGUID);
            ValidateUserExists(cred.TenantGUID, cred.UserGUID);
            return CredentialFromDataRow(Query(UpdateCredentialQuery(cred), true).Rows[0]);
        }

        #endregion

        #region Tags

        /// <inheritdoc />
        public override TagMetadata CreateTag(TagMetadata tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            ValidateTenantExists(tag.TenantGUID);
            ValidateGraphExists(tag.TenantGUID, tag.GraphGUID);
            if (tag.NodeGUID != null) ValidateNodeExists(tag.TenantGUID, tag.GraphGUID, tag.NodeGUID.Value);
            if (tag.EdgeGUID != null) ValidateEdgeExists(tag.TenantGUID, tag.GraphGUID, tag.EdgeGUID.Value);

            string createQuery = InsertTagQuery(tag);

            DataTable createResult = null;
            TagMetadata created = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
            }

            created = TagFromDataRow(createResult.Rows[0]);
            return created;
        }

        /// <inheritdoc />
        public override TagMetadata CreateTag(Guid tenantGuid, Guid graphGuid, Guid? nodeGuid, Guid? edgeGuid, string key, string value)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return CreateTag(new TagMetadata { GUID = Guid.NewGuid(), TenantGUID = tenantGuid, GraphGUID = graphGuid, NodeGUID = nodeGuid, EdgeGUID = edgeGuid, Key = key, Value = value });
        }

        /// <inheritdoc />
        public override List<TagMetadata> CreateMultipleTags(Guid tenantGuid, Guid graphGuid, List<TagMetadata> tags)
        {
            if (tags == null || tags.Count < 1) return null;
            ValidateTenantExists(tenantGuid);
            ValidateGraphExists(tenantGuid, graphGuid);

            foreach (TagMetadata tag in tags)
            {
                tag.TenantGUID = tenantGuid;
                tag.GraphGUID = graphGuid;
            }

            string createQuery = InsertMultipleTagsQuery(tenantGuid, graphGuid, tags);
            string retrieveQuery = SelectMultipleNodesQuery(tenantGuid, tags.Select(n => n.GUID).ToList());
            DataTable createResult = null;
            DataTable retrieveResult = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
                retrieveResult = Query(retrieveQuery, true);
            }

            return TagsFromDataTable(retrieveResult);
        }

        /// <inheritdoc />
        public override void DeleteTag(Guid tenantGuid, Guid guid)
        {
            TagMetadata tag = ReadTag(tenantGuid, guid);
            if (tag != null)
            {
                Query(DeleteTagQuery(tenantGuid, guid), true);
            }
        }

        /// <inheritdoc />
        public override void DeleteTags(Guid tenantGuid, Guid? graphGuid, List<Guid> nodeGuids, List<Guid> edgeGuids)
        {
            Query(DeleteTagsQuery(tenantGuid, graphGuid, nodeGuids, edgeGuids));
        }

        /// <inheritdoc />
        public override bool ExistsTag(Guid tenantGuid, Guid guid)
        {
            if (ReadTag(tenantGuid, guid) != null) return true;
            return false;
        }

        /// <inheritdoc />
        public override TagMetadata ReadTag(Guid tenantGuid, Guid guid)
        {
            DataTable result = Query(SelectTagQuery(tenantGuid, guid));
            if (result != null && result.Rows.Count == 1) return TagFromDataRow(result.Rows[0]);
            return null;
        }

        /// <inheritdoc />
        public override IEnumerable<TagMetadata> ReadTags(
            Guid tenantGuid,
            Guid graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            string key,
            string val,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectTagsQuery(tenantGuid, graphGuid, nodeGuid, edgeGuid, key, val, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    yield return TagFromDataRow(result.Rows[i]);
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override TagMetadata UpdateTag(TagMetadata tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            ValidateTenantExists(tag.TenantGUID);
            ValidateGraphExists(tag.TenantGUID, tag.GraphGUID);
            if (tag.NodeGUID != null) ValidateNodeExists(tag.TenantGUID, tag.GraphGUID, tag.NodeGUID.Value);
            if (tag.EdgeGUID != null) ValidateEdgeExists(tag.TenantGUID, tag.GraphGUID, tag.EdgeGUID.Value);

            string updateQuery = UpdateTagQuery(tag);
            DataTable updateResult = null;
            TagMetadata updated = null;

            lock (_CreateLock)
            {
                updateResult = Query(updateQuery, true);
            }

            updated = TagFromDataRow(updateResult.Rows[0]);
            return updated;
        }

        #endregion

        #region Graphs

        /// <inheritdoc />
        public override Graph CreateGraph(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            ValidateTags(graph.Tags);
            ValidateTenantExists(graph.TenantGUID);

            string createQuery = InsertGraphQuery(graph);
            DataTable createResult = null;
            Graph created = null;
            Graph existing = null;

            lock (_CreateLock)
            {
                existing = ReadGraph(graph.TenantGUID, graph.GUID);
                if (existing != null) return existing;
                createResult = Query(createQuery, true);
            }

            created = GraphFromDataRow(createResult.Rows[0]);

            if (graph.Tags != null && graph.Tags is Dictionary<string, string> dict)
            {
                List<TagMetadata> tags = new List<TagMetadata>();
                foreach (KeyValuePair<string, string> tag in dict)
                {
                    tags.Add(new TagMetadata
                    {
                        TenantGUID = graph.TenantGUID,
                        GraphGUID = graph.GUID,
                        Key = tag.Key,
                        Value = tag.Value
                    });

                    CreateMultipleTags(graph.TenantGUID, graph.GUID, tags);
                }
            }

            created.Tags = graph.Tags;
            return created;
        }

        /// <inheritdoc />
        public override Graph CreateGraph(Guid tenantGuid, Guid guid, string name, object data = null, Dictionary<string, string> tags = null)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            return CreateGraph(new Graph
            {
                GUID = guid,
                TenantGUID = tenantGuid,
                Name = name,
                Tags = tags,
                Data = data
            });
        }

        /// <inheritdoc />
        public override IEnumerable<Graph> ReadGraphs(
            Guid tenantGuid,
            Dictionary<string, string> tags = null,
            Expr graphFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectGraphsQuery(tenantGuid, tags, graphFilter, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    Graph graph = GraphFromDataRow(result.Rows[i]);
                    List<TagMetadata> allTags = ReadTags(tenantGuid, graph.GUID, null, null, null, null).ToList();
                    if (allTags != null) graph.Tags = TagMetadata.ToDictionary(allTags);
                    yield return graph;
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override Graph ReadGraph(Guid tenantGuid, Guid guid)
        {
            DataTable result = Query(SelectGraphQuery(tenantGuid, guid));
            if (result != null && result.Rows.Count == 1)
            {
                Graph graph = GraphFromDataRow(result.Rows[0]);
                List<TagMetadata> tags = ReadTags(tenantGuid, guid, null, null, null, null).ToList();
                if (tags != null) graph.Tags = TagMetadata.ToDictionary(tags);
                return graph;
            }
            return null;
        }

        /// <inheritdoc />
        public override Graph UpdateGraph(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            ValidateTags(graph.Tags);
            ValidateTenantExists(graph.TenantGUID);
            ValidateGraphExists(graph.TenantGUID, graph.GUID);
            Graph updated = GraphFromDataRow(Query(UpdateGraphQuery(graph), true).Rows[0]);
            DeleteGraphTags(graph.TenantGUID, graph.GUID);
            if (graph.Tags != null)
            {
                CreateMultipleTags(
                    graph.TenantGUID, 
                    graph.GUID, 
                    TagMetadata.FromDictionary(
                        graph.TenantGUID,
                        graph.GUID,
                        null,
                        null,
                        graph.Tags as Dictionary<string, string>));
                updated.Tags = graph.Tags;
            }
            return updated;
        }

        /// <inheritdoc />
        public override void DeleteGraph(Guid tenantGuid, Guid graphGuid, bool force = false)
        {
            Graph graph = ReadGraph(tenantGuid, graphGuid);
            if (graph != null)
            {
                if (force)
                {
                    DeleteNodes(tenantGuid, graphGuid);
                }

                Query(DeleteGraphQuery(tenantGuid, graphGuid), true);
                DeleteTags(tenantGuid, graphGuid, null, null);
            }
        }

        /// <inheritdoc />
        public override void DeleteGraphs(Guid tenantGuid)
        {
            List<Graph> graphs = ReadGraphs(tenantGuid).ToList();
            if (graphs != null && graphs.Count > 0)
            {
                foreach (Graph graph in graphs)
                {
                    DeleteGraph(graph.TenantGUID, graph.GUID, true);
                }
            }

            DeleteTags(tenantGuid, null, null, null);
        }

        /// <inheritdoc />
        public override void DeleteGraphTags(Guid tenantGuid, Guid graphGuid)
        {
            Query(DeleteTagsForGraphOnlyQuery(tenantGuid, graphGuid));
        }

        /// <inheritdoc />
        public override bool ExistsGraph(Guid tenantGuid, Guid graphGuid)
        {
            if (ReadGraph(tenantGuid, graphGuid) != null) return true;
            return false;
        }

        #endregion

        #region Nodes

        /// <inheritdoc />
        public override Node CreateNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            ValidateTags(node.Tags);
            ValidateTenantExists(node.TenantGUID);
            ValidateGraphExists(node.TenantGUID, node.GraphGUID);

            string createQuery = InsertNodeQuery(node);
            DataTable createResult = null;
            Node created = null;
            Node existing = null;

            lock (_CreateLock)
            {
                existing = ReadNode(node.TenantGUID, node.GraphGUID, node.GUID);
                if (existing != null) return existing;
                createResult = Query(createQuery, true);
            }

            created = NodeFromDataRow(createResult.Rows[0]);

            if (node.Tags != null && node.Tags is Dictionary<string, string> dict)
            {
                List<TagMetadata> tags = new List<TagMetadata>();
                foreach (KeyValuePair<string, string> tag in dict)
                {
                    tags.Add(new TagMetadata
                    {
                        TenantGUID = node.TenantGUID,
                        GraphGUID = node.GraphGUID,
                        NodeGUID = node.GUID,
                        Key = tag.Key,
                        Value = tag.Value
                    });

                    CreateMultipleTags(node.TenantGUID, node.GUID, tags);
                }
            }

            created.Tags = node.Tags;
            return created;
        }

        /// <inheritdoc />
        public override Node CreateNode(Guid tenantGuid, Guid graphGuid, Guid guid, string name, object data = null, Dictionary<string, string> tags = null)
        {
            return CreateNode(new Node
            {
                GUID = guid,
                TenantGUID = tenantGuid,
                GraphGUID = graphGuid,
                Name = name,
                Data = data,
                Tags = tags
            });
        }

        /// <inheritdoc />
        public override List<Node> CreateMultipleNodes(Guid tenantGuid, Guid graphGuid, List<Node> nodes)
        {
            if (nodes == null || nodes.Count < 1) return null;
            ValidateTenantExists(tenantGuid);
            ValidateGraphExists(tenantGuid, graphGuid);

            List<TagMetadata> tags = new List<TagMetadata>();

            foreach (Node node in nodes)
            {
                ValidateTags(node.Tags);
                node.TenantGUID = tenantGuid;
                node.GraphGUID = graphGuid;
                if (node.Tags != null) tags.AddRange(TagMetadata.FromDictionary(tenantGuid, graphGuid, node.GUID, null, (node.Tags as Dictionary<string, string>)));
            }

            ExistenceRequest req = new ExistenceRequest
            {
                Nodes = nodes.Select(n => n.GUID).ToList()
            };

            ExistenceResult result = BatchExistence(tenantGuid, graphGuid, req);
            if (result.ExistingNodes != null && result.ExistingNodes.Count > 0)
                throw new InvalidOperationException("The requested node GUIDs already exist: " + string.Join(",", result.ExistingNodes));

            string createQuery = InsertMultipleNodesQuery(tenantGuid, nodes);
            string retrieveQuery = SelectMultipleNodesQuery(tenantGuid, nodes.Select(n => n.GUID).ToList());
            DataTable createResult = null;
            DataTable retrieveResult = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
                retrieveResult = Query(retrieveQuery, true);
            }

            List<Node> created = NodesFromDataTable(retrieveResult);

            if (tags != null && tags.Count > 0)
            {
                List<TagMetadata> createdTags = CreateMultipleTags(tenantGuid, graphGuid, tags);
                foreach (Node node in created)
                {
                    node.Tags = createdTags.Where(t =>
                               t.TenantGUID == tenantGuid &&
                               t.GraphGUID == graphGuid &&
                               t.NodeGUID == node.GUID)
                           .ToDictionary(
                               t => t.Key,
                               t => t.Value,
                               StringComparer.OrdinalIgnoreCase
                           );
                }
            }

            return created;
        }

        /// <inheritdoc />
        public override IEnumerable<Node> ReadNodes(
            Guid tenantGuid,
            Guid graphGuid,
            Dictionary<string, string> tags = null,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectNodesQuery(tenantGuid, graphGuid, tags, nodeFilter, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    Node node = NodeFromDataRow(result.Rows[i]);
                    List<TagMetadata> allTags = ReadTags(tenantGuid, graphGuid, node.GUID, null, null, null).ToList();
                    if (allTags != null) node.Tags = TagMetadata.ToDictionary(allTags);
                    yield return node;
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override Node ReadNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            DataTable result = Query(SelectNodeQuery(tenantGuid, graphGuid, nodeGuid));
            if (result != null && result.Rows.Count == 1)
            {
                Node node = NodeFromDataRow(result.Rows[0]);
                List<TagMetadata> tags = ReadTags(tenantGuid, graphGuid, nodeGuid, null, null, null).ToList();
                if (tags != null) node.Tags = TagMetadata.ToDictionary(tags);
                return node;
            }
            return null;
        }

        /// <inheritdoc />
        public override Node UpdateNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            ValidateTags(node.Tags);
            ValidateTenantExists(node.TenantGUID);
            ValidateGraphExists(node.TenantGUID, node.GraphGUID);
            Node updated = NodeFromDataRow(Query(UpdateNodeQuery(node), true).Rows[0]);
            DeleteNodeTags(node.TenantGUID, node.GraphGUID, node.GUID);
            if (node.Tags != null)
            {
                CreateMultipleTags(
                    node.TenantGUID,
                    node.GraphGUID,
                    TagMetadata.FromDictionary(
                        node.TenantGUID,
                        node.GraphGUID, 
                        node.GUID,
                        null,
                        node.Tags as Dictionary<string, string>));
                updated.Tags = node.Tags;
            }
            return updated;
        }

        /// <inheritdoc />
        public override void DeleteNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            Query(DeleteNodeQuery(tenantGuid, graphGuid, nodeGuid), true);
            DeleteTags(tenantGuid, graphGuid, new List<Guid> { nodeGuid }, null);
        }

        /// <inheritdoc />
        public override void DeleteNodes(Guid tenantGuid, Guid graphGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            Query(DeleteNodesQuery(tenantGuid, graphGuid), true);
            DeleteEdges(tenantGuid, graphGuid);
            DeleteTags(tenantGuid, graphGuid, null, null);
        }

        /// <inheritdoc />
        public override void DeleteNodes(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            if (nodeGuids == null || nodeGuids.Count < 1) return;
            ValidateGraphExists(tenantGuid, graphGuid);
            Query(DeleteNodesQuery(tenantGuid, graphGuid, nodeGuids), true);
            DeleteNodeEdges(tenantGuid, graphGuid, nodeGuids);
            DeleteTags(tenantGuid, graphGuid, nodeGuids, null);
        }

        /// <inheritdoc />
        public override void DeleteNodeEdges(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            DeleteNodeEdges(tenantGuid, graphGuid, new List<Guid> { nodeGuid });
        }

        /// <inheritdoc />
        public override void DeleteNodeEdges(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            if (nodeGuids == null || nodeGuids.Count < 1) return;
            ValidateGraphExists(tenantGuid, graphGuid);
            string query = DeleteNodeEdgesQuery(tenantGuid, graphGuid, nodeGuids);

            List<Edge> edges = new List<Edge>();
            foreach (Guid nodeGuid in nodeGuids)
            {
                List<Edge> nodeEdges = GetConnectedEdges(tenantGuid, graphGuid, nodeGuid).ToList();
                if (nodeEdges != null && nodeEdges.Count > 0) edges.AddRange(nodeEdges);
            }

            lock (_CreateLock)
                Query(query);

            DeleteTags(tenantGuid, graphGuid, null, edges.Select(e => e.GUID).ToList());
        }

        /// <inheritdoc />
        public override void DeleteNodeTags(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            DeleteTags(tenantGuid, graphGuid, new List<Guid> { nodeGuid }, null);
        }

        /// <inheritdoc />
        public override bool ExistsNode(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (ReadNode(tenantGuid, graphGuid, nodeGuid) != null) return true;
            return false;
        }

        /// <inheritdoc />
        public override IEnumerable<Node> GetParents(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable allEdges = Query(SelectConnectedEdgesQuery(tenantGuid, graphGuid, nodeGuid, null, edgeFilter, skip, order));
                if (allEdges == null || allEdges.Rows.Count < 1) break;

                for (int i = 0; i < allEdges.Rows.Count; i++)
                {
                    Edge edge = EdgeFromDataRow(allEdges.Rows[i]);
                    if (edge.To.Equals(nodeGuid))
                    {
                        Node parent = ReadNode(tenantGuid, graphGuid, edge.From);
                        if (parent != null) yield return parent;
                        else Logging.Log(SeverityEnum.Warn, "node " + edge.From + " referenced in graph " + graphGuid + " but does not exist");
                    }

                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Node> GetChildren(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable allEdges = Query(SelectConnectedEdgesQuery(tenantGuid, graphGuid, nodeGuid, null, edgeFilter, skip, order));
                if (allEdges == null || allEdges.Rows.Count < 1) break;

                for (int i = 0; i < allEdges.Rows.Count; i++)
                {
                    Edge edge = EdgeFromDataRow(allEdges.Rows[i]);
                    if (edge.From.Equals(nodeGuid))
                    {
                        Node child = ReadNode(tenantGuid, graphGuid, edge.To);
                        if (child != null) yield return child;
                        else Logging.Log(SeverityEnum.Warn, "node " + edge.To + " referenced in graph " + graphGuid + " but does not exist");
                    }

                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Node> GetNeighbors(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            List<Guid> visited = new List<Guid>();

            while (true)
            {
                DataTable allEdges = Query(SelectConnectedEdgesQuery(tenantGuid, graphGuid, nodeGuid, null, edgeFilter, skip, order));
                if (allEdges == null || allEdges.Rows.Count < 1) break;

                for (int i = 0; i < allEdges.Rows.Count; i++)
                {
                    Edge edge = EdgeFromDataRow(allEdges.Rows[i]);
                    if (edge.From.Equals(nodeGuid))
                    {
                        if (visited.Contains(edge.To))
                        {
                            skip++;
                            continue;
                        }
                        else
                        {
                            Node neighbor = ReadNode(tenantGuid, graphGuid, edge.To);
                            if (neighbor != null)
                            {
                                visited.Add(edge.To);
                                yield return neighbor;
                            }
                            else Logging.Log(SeverityEnum.Warn, "node " + edge.From + " referenced in graph " + graphGuid + " but does not exist");
                            skip++;
                        }
                    }
                    if (edge.To.Equals(nodeGuid))
                    {
                        if (visited.Contains(edge.From))
                        {
                            skip++;
                            continue;
                        }
                        else
                        {
                            Node neighbor = ReadNode(tenantGuid, graphGuid, edge.From);
                            if (neighbor != null)
                            {
                                visited.Add(edge.From);
                                yield return neighbor;
                            }
                            else Logging.Log(SeverityEnum.Warn, "node " + edge.From + " referenced in graph " + graphGuid + " but does not exist");
                            skip++;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<RouteDetail> GetRoutes(
            SearchTypeEnum searchType,
            Guid tenantGuid,
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Expr edgeFilter = null,
            Expr nodeFilter = null)
        {
            #region Retrieve-Objects

            Graph graph = ReadGraph(tenantGuid, graphGuid);
            if (graph == null) throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");

            Node fromNode = ReadNode(tenantGuid, graphGuid, fromNodeGuid);
            if (fromNode == null) throw new ArgumentException("No node with GUID '" + fromNodeGuid + "' exists in graph '" + graphGuid + "'");

            Node toNode = ReadNode(tenantGuid, graphGuid, toNodeGuid);
            if (toNode == null) throw new ArgumentException("No node with GUID '" + toNodeGuid + "' exists in graph '" + graphGuid + "'");

            #endregion

            #region Perform-Search

            RouteDetail routeDetail = new RouteDetail();

            if (searchType == SearchTypeEnum.DepthFirstSearch)
            {
                foreach (RouteDetail route in GetRoutesDfs(
                    tenantGuid,
                    graph,
                    fromNode,
                    toNode,
                    edgeFilter,
                    nodeFilter,
                    new List<Node> { fromNode },
                    new List<Edge>()))
                {
                    if (route != null) yield return route;
                }
            }
            else
            {
                throw new ArgumentException("Unknown search type '" + searchType.ToString() + ".");
            }

            #endregion
        }

        #endregion

        #region Batch

        /// <inheritdoc />
        public override ExistenceResult BatchExistence(Guid tenantGuid, Guid graphGuid, ExistenceRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (!req.ContainsExistenceRequest()) throw new ArgumentException("Supplied existence request contains no valid existence filters.");

            ValidateTenantExists(tenantGuid);
            ValidateGraphExists(tenantGuid, graphGuid);

            Graph graph = ReadGraph(tenantGuid, graphGuid);
            if (graph == null) throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");

            ExistenceResult resp = new ExistenceResult();

            #region Nodes

            if (req.Nodes != null)
            {
                resp.ExistingNodes = new List<Guid>();
                resp.MissingNodes = new List<Guid>();

                string nodesQuery = BatchExistsNodesQuery(tenantGuid, graphGuid, req.Nodes);
                DataTable nodesResult = Query(nodesQuery);
                if (nodesResult != null && nodesResult.Rows != null && nodesResult.Rows.Count > 0)
                {
                    foreach (DataRow row in nodesResult.Rows)
                    {
                        if (row["exists"] != null && row["exists"] != DBNull.Value)
                        {
                            int exists = Convert.ToInt32(row["exists"]);
                            if (exists == 1)
                                resp.ExistingNodes.Add(Guid.Parse(row["guid"].ToString()));
                            else
                                resp.MissingNodes.Add(Guid.Parse(row["guid"].ToString()));
                        }
                    }
                }
            }

            #endregion

            #region Edges

            if (req.Edges != null)
            {
                resp.ExistingEdges = new List<Guid>();
                resp.MissingEdges = new List<Guid>();

                string edgesQuery = BatchExistsEdgesQuery(tenantGuid, graphGuid, req.Edges);
                DataTable edgesResult = Query(edgesQuery);
                if (edgesResult != null && edgesResult.Rows != null && edgesResult.Rows.Count > 0)
                {
                    foreach (DataRow row in edgesResult.Rows)
                    {
                        if (row["exists"] != null && row["exists"] != DBNull.Value)
                        {
                            int exists = Convert.ToInt32(row["exists"]);
                            if (exists == 1)
                                resp.ExistingEdges.Add(Guid.Parse(row["guid"].ToString()));
                            else
                                resp.MissingEdges.Add(Guid.Parse(row["guid"].ToString()));
                        }
                    }
                }
            }

            #endregion

            #region Edges-Between

            if (req.EdgesBetween != null)
            {
                resp.ExistingEdgesBetween = new List<EdgeBetween>();
                resp.MissingEdgesBetween = new List<EdgeBetween>();

                string betweenQuery = BatchExistsEdgesBetweenQuery(tenantGuid, graphGuid, req.EdgesBetween);
                DataTable betweenResult = Query(betweenQuery);
                if (betweenResult != null && betweenResult.Rows != null && betweenResult.Rows.Count > 0)
                {
                    foreach (DataRow row in betweenResult.Rows)
                    {
                        if (row["exists"] != null && row["exists"] != DBNull.Value)
                        {
                            int exists = Convert.ToInt32(row["exists"]);
                            if (exists == 1)
                                resp.ExistingEdgesBetween.Add(new EdgeBetween
                                {
                                    From = Guid.Parse(row["fromguid"].ToString()),
                                    To = Guid.Parse(row["toguid"].ToString())
                                });
                            else
                                resp.MissingEdgesBetween.Add(new EdgeBetween
                                {
                                    From = Guid.Parse(row["fromguid"].ToString()),
                                    To = Guid.Parse(row["toguid"].ToString())
                                });
                        }
                    }
                }
            }

            #endregion

            return resp;
        }

        #endregion

        #region Edges

        /// <inheritdoc />
        public override IEnumerable<Edge> GetConnectedEdges(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectConnectedEdgesQuery(tenantGuid, graphGuid, nodeGuid, tags, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> GetEdgesFrom(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Dictionary<string, string> tags,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectEdgesFromQuery(tenantGuid, graphGuid, nodeGuid, tags, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> GetEdgesTo(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectEdgesToQuery(tenantGuid, graphGuid, nodeGuid, tags, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> GetEdgesBetween(
            Guid tenantGuid,
            Guid graphGuid,
            Guid fromNodeGuid,
            Guid toNodeGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectEdgesBetweenQuery(tenantGuid, graphGuid, fromNodeGuid, toNodeGuid, tags, edgeFilter, skip, order));
                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        yield return EdgeFromDataRow(result.Rows[i]);
                        skip++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override Edge CreateEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            ValidateTags(edge.Tags);
            ValidateTenantExists(edge.TenantGUID);
            ValidateGraphExists(edge.TenantGUID, edge.GraphGUID);

            string insertQuery = InsertEdgeQuery(edge);
            DataTable createResult = null;
            Edge created = null;
            Edge existing = null;

            lock (_CreateLock)
            {
                existing = ReadEdge(edge.TenantGUID, edge.GraphGUID, edge.GUID);
                if (existing != null) return existing;
                createResult = Query(insertQuery, true);
            }

            created = EdgeFromDataRow(createResult.Rows[0]);

            if (edge.Tags != null && edge.Tags is Dictionary<string, string> dict)
            {
                List<TagMetadata> tags = new List<TagMetadata>();
                foreach (KeyValuePair<string, string> tag in dict)
                {
                    tags.Add(new TagMetadata
                    {
                        TenantGUID = edge.TenantGUID,
                        GraphGUID = edge.GraphGUID,
                        EdgeGUID = edge.GUID,
                        Key = tag.Key,
                        Value = tag.Value
                    });

                    CreateMultipleTags(edge.TenantGUID, edge.GUID, tags);
                }
            }

            created.Tags = edge.Tags;
            return created;
        }

        /// <inheritdoc />
        public override Edge CreateEdge(Guid tenantGuid, Guid graphGuid, Guid guid, Guid fromGuid, Guid toGuid, string name, int cost = 0, object data = null, Dictionary<string, string> tags = null)
        {
            return CreateEdge(new Edge
            {
                GUID = guid,
                TenantGUID = tenantGuid,
                GraphGUID = graphGuid,
                From = fromGuid,
                To = toGuid,
                Name = name,
                Cost = cost,
                Data = data,
                Tags = tags
            });
        }

        /// <inheritdoc />
        public override List<Edge> CreateMultipleEdges(Guid tenantGuid, Guid graphGuid, List<Edge> edges)
        {
            if (edges == null || edges.Count < 1) return null;
            ValidateTenantExists(tenantGuid);
            ValidateGraphExists(tenantGuid, graphGuid);

            List<TagMetadata> tags = new List<TagMetadata>();

            foreach (Edge edge in edges)
            {
                ValidateTags(edge.Tags);
                edge.TenantGUID = tenantGuid;
                edge.GraphGUID = graphGuid;
                if (edge.Tags != null) tags.AddRange(TagMetadata.FromDictionary(tenantGuid, graphGuid, edge.GUID, null, (edge.Tags as Dictionary<string, string>)));
            }

            ExistenceRequest req = new ExistenceRequest();
            req.Edges = edges.Select(n => n.GUID).ToList();
            req.Nodes = edges.Select(n => n.From).Concat(edges.Select(n => n.To)).Distinct().ToList();

            ExistenceResult result = BatchExistence(tenantGuid, graphGuid, req);
            if (result.ExistingEdges != null && result.ExistingEdges.Count > 0)
                throw new InvalidOperationException("The requested edge GUIDs already exist: " + string.Join(",", result.ExistingEdges));

            if (result.MissingNodes != null && result.MissingNodes.Count > 0)
                throw new KeyNotFoundException("The specified node GUIDs do not exist: " + string.Join(",", result.MissingNodes));

            string createQuery = InsertMultipleEdgesQuery(tenantGuid, edges);
            string retrieveQuery = SelectMultipleEdgesQuery(tenantGuid, edges.Select(n => n.GUID).ToList());
            DataTable createResult = null;
            DataTable retrieveResult = null;

            lock (_CreateLock)
            {
                createResult = Query(createQuery, true);
                retrieveResult = Query(retrieveQuery, true);
            }

            List<Edge> created = EdgesFromDataTable(retrieveResult);

            if (tags != null && tags.Count > 0)
            {
                List<TagMetadata> createdTags = CreateMultipleTags(tenantGuid, graphGuid, tags);
                foreach (Edge edge in created)
                {
                    edge.Tags = createdTags.Where(t =>
                               t.TenantGUID == tenantGuid &&
                               t.GraphGUID == graphGuid &&
                               t.EdgeGUID == edge.GUID)
                           .ToDictionary(
                               t => t.Key,
                               t => t.Value,
                               StringComparer.OrdinalIgnoreCase
                           );
                }
            }

            return created;
        }

        /// <inheritdoc />
        public override IEnumerable<Edge> ReadEdges(
            Guid tenantGuid,
            Guid graphGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending,
            int skip = 0)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

            while (true)
            {
                DataTable result = Query(SelectEdgesQuery(tenantGuid, graphGuid, tags, edgeFilter, skip, order));
                if (result == null || result.Rows.Count < 1) break;

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    Edge edge = EdgeFromDataRow(result.Rows[i]);
                    List<TagMetadata> allTags = ReadTags(tenantGuid, graphGuid, null, edge.GUID, null, null).ToList();
                    if (allTags != null) edge.Tags = TagMetadata.ToDictionary(allTags);
                    yield return edge;
                    skip++;
                }
            }
        }

        /// <inheritdoc />
        public override Edge ReadEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            DataTable result = Query(SelectEdgeQuery(tenantGuid, graphGuid, edgeGuid));
            if (result != null && result.Rows.Count == 1)
            {
                Edge edge = EdgeFromDataRow(result.Rows[0]);
                List<TagMetadata> tags = ReadTags(tenantGuid, graphGuid, null, edgeGuid, null, null).ToList();
                if (tags != null) edge.Tags = TagMetadata.ToDictionary(tags);
                return edge;
            }
            return null;
        }

        /// <inheritdoc />
        public override Edge UpdateEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            ValidateTags(edge.Tags);
            ValidateTenantExists(edge.TenantGUID);
            ValidateGraphExists(edge.TenantGUID, edge.GraphGUID);
            Edge updated = EdgeFromDataRow(Query(UpdateEdgeQuery(edge), true).Rows[0]);
            DeleteEdgeTags(edge.TenantGUID, edge.GraphGUID, edge.GUID);
            if (edge.Tags != null)
            {
                CreateMultipleTags(
                    edge.TenantGUID,
                    edge.GraphGUID,
                    TagMetadata.FromDictionary(
                        edge.TenantGUID,
                        edge.GraphGUID,
                        null,
                        edge.GUID,
                        edge.Tags as Dictionary<string, string>));
                updated.Tags = edge.Tags;
            }
            return updated;
        }

        /// <inheritdoc />
        public override void DeleteEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            Query(DeleteEdgeQuery(tenantGuid, graphGuid, edgeGuid), true);
            DeleteTags(tenantGuid, graphGuid, null, new List<Guid> { edgeGuid });
        }

        /// <inheritdoc />
        public override void DeleteEdges(Guid tenantGuid, Guid graphGuid)
        {
            ValidateGraphExists(tenantGuid, graphGuid);
            List<Edge> edges = ReadEdges(tenantGuid, graphGuid).ToList();
            Query(DeleteEdgesQuery(tenantGuid, graphGuid), true);
            if (edges != null && edges.Count > 0) DeleteTags(tenantGuid, graphGuid, null, edges.Select(e => e.GUID).ToList());
        }

        /// <inheritdoc />
        public override void DeleteEdges(Guid tenantGuid, Guid graphGuid, List<Guid> edgeGuids)
        {
            if (edgeGuids == null || edgeGuids.Count < 1) return;
            ValidateGraphExists(tenantGuid, graphGuid);
            Query(DeleteEdgesQuery(tenantGuid, graphGuid, edgeGuids), true);
            DeleteTags(tenantGuid, graphGuid, null, edgeGuids);
        }

        /// <inheritdoc />
        public override void DeleteEdgeTags(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            DeleteTags(tenantGuid, graphGuid, null, new List<Guid> { edgeGuid });
        }

        /// <inheritdoc />
        public override bool ExistsEdge(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            if (ReadEdge(tenantGuid, graphGuid, edgeGuid) != null) return true;
            return false;
        }

        #endregion

        #endregion

        #region Private-Methods

        #region General

        private string Sanitize(string val)
        {
            if (String.IsNullOrEmpty(val)) return val;

            string ret = "";

            //
            // null, below ASCII range, above ASCII range
            //
            for (int i = 0; i < val.Length; i++)
            {
                if (((int)(val[i]) == 10) ||      // Preserve carriage return
                    ((int)(val[i]) == 13))        // and line feed
                {
                    ret += val[i];
                }
                else if ((int)(val[i]) < 32)
                {
                    continue;
                }
                else
                {
                    ret += val[i];
                }
            }

            //
            // double dash
            //
            int doubleDash = 0;
            while (true)
            {
                doubleDash = ret.IndexOf("--");
                if (doubleDash < 0)
                {
                    break;
                }
                else
                {
                    ret = ret.Remove(doubleDash, 2);
                }
            }

            //
            // open comment
            // 
            int openComment = 0;
            while (true)
            {
                openComment = ret.IndexOf("/*");
                if (openComment < 0) break;
                else
                {
                    ret = ret.Remove(openComment, 2);
                }
            }

            //
            // close comment
            //
            int closeComment = 0;
            while (true)
            {
                closeComment = ret.IndexOf("*/");
                if (closeComment < 0) break;
                else
                {
                    ret = ret.Remove(closeComment, 2);
                }
            }

            //
            // in-string replacement
            //
            ret = ret.Replace("'", "''");
            return ret;
        }

        private DataTable Query(string query, bool isTransaction = false)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(query);
            if (query.Length > MaxStatementLength) throw new ArgumentException("Query exceeds maximum statement length of " + MaxStatementLength + " characters.");

            DataTable result = new DataTable();

            if (isTransaction)
            {
                query = query.Trim();
                query = "BEGIN TRANSACTION; " + query + " END TRANSACTION;";
            }

            if (Logging.LogQueries) Logging.Log(SeverityEnum.Debug, "query: " + query);

            lock (_QueryLock)
            {
                using (SqliteConnection conn = new SqliteConnection(_ConnectionString))
                {
                    try
                    {
                        conn.Open();

                        using (SqliteCommand cmd = new SqliteCommand(query, conn))
                        {
                            using (SqliteDataReader rdr = cmd.ExecuteReader())
                            {
                                result.Load(rdr);
                            }
                        }

                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        if (isTransaction)
                        {
                            using (SqliteCommand cmd = new SqliteCommand("ROLLBACK;", conn))
                                cmd.ExecuteNonQuery();
                        }

                        e.Data.Add("IsTransaction", isTransaction);
                        e.Data.Add("Query", query);
                        throw;
                    }
                }
            }

            if (Logging.LogResults) Logging.Log(SeverityEnum.Debug, "result: " + query + ": " + (result != null ? result.Rows.Count + " rows" : "(null)"));
            return result;
        }

        private void CreateTablesAndIndices()
        {
            List<string> queries = new List<string>();

            #region Tenants

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'tenants' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "name VARCHAR(128), "
                + "active INT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_guid' ON 'tenants' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_name' ON 'tenants' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_createdutc' ON 'tenants' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_lastupdateutc' ON 'tenants' ('lastupdateutc' ASC);");

            #endregion

            #region Users

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'users' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "firstname VARCHAR(64), "
                + "lastname VARCHAR(64), "
                + "email VARCHAR(128), "
                + "password VARCHAR(128), "
                + "active INT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_guid' ON 'users' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_tenantguid' ON 'users' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_email' ON 'users' (email ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_password' ON 'users' (password ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_createdutc' ON 'users' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_lastupdateutc' ON 'users' ('lastupdateutc' ASC);");

            #endregion

            #region Credentials

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'creds' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "userguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(64), "
                + "bearertoken VARCHAR(64), "
                + "active INT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_guid' ON 'creds' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_tenantguid' ON 'creds' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_userguid' ON 'creds' (userguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_bearertoken' ON 'creds' (bearertoken ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_createdutc' ON 'creds' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_lastupdateutc' ON 'creds' ('lastupdateutc' ASC);");

            #endregion

            #region Tags

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'tags' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64) NOT NULL, "
                + "nodeguid VARCHAR(64), "
                + "edgeguid VARCHAR(64), "
                + "tagkey VARCHAR(256), "
                + "tagvalue TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_guid' ON 'tags' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_tenantguid' ON 'tags' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_graphguid' ON 'tags' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_nodeguid' ON 'tags' (nodeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_edgeguid' ON 'tags' (edgeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_tagkey' ON 'tags' (tagkey ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_tagvalue' ON 'tags' (tagvalue ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_createdutc' ON 'tags' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_lastupdateutc' ON 'tags' ('lastupdateutc' ASC);");

            #endregion

            #region Graphs

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'graphs' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "data TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_guid' ON 'graphs' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_tenantguid' ON 'graphs' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_name' ON 'graphs' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_createdutc' ON 'graphs' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_lastupdateutc' ON 'graphs' ('lastupdateutc' ASC);");

            if (IndexData)
                queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_data' ON 'graphs' ('data' ASC);");

            #endregion

            #region Nodes

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'nodes' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "data TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_guid' ON 'nodes' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_tenantguid' ON 'nodes' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_graphguid' ON 'nodes' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_name' ON 'nodes' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_createdutc' ON 'nodes' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_lastupdateutc' ON 'nodes' ('lastupdateutc' ASC);");

            if (IndexData)
                queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_data' ON 'nodes' (data ASC);");

            #endregion

            #region Edges

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'edges' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "fromguid VARCHAR(64) NOT NULL, "
                + "toguid VARCHAR(64) NOT NULL, "
                + "cost INT NOT NULL, "
                + "data TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_guid' ON 'edges' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_tenantguid' ON 'edges' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_graphguid' ON 'edges' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_name' ON 'edges' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_fromguid' ON 'edges' (fromguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_toguid' ON 'edges' (toguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_createdutc' ON 'edges' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_lastupdateutc' ON 'edges' ('lastupdateutc' ASC);");

            if (IndexData)
                queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_data' ON 'edges' (data ASC);");

            #endregion

            #region Run-Queries

            foreach (string query in queries)
                Query(query);

            #endregion
        }

        private string GetDataRowStringValue(DataRow row, string column)
        {
            if (row[column] != null
                && row[column] != DBNull.Value)
            {
                return row[column].ToString();
            }
            return null;
        }

        private object GetDataRowJsonValue(DataRow row, string column)
        {
            if (row[column] != null
                && row[column] != DBNull.Value)
            {
                return Serializer.DeserializeJson<object>(row[column].ToString());
            }
            return null;
        }

        private bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        private List<object> ObjectToList(object obj)
        {
            if (obj == null) return null;
            List<object> ret = new List<object>();
            var enumerator = ((IEnumerable)obj).GetEnumerator();
            while (enumerator.MoveNext())
            {
                ret.Add(enumerator.Current);
            }
            return ret;
        }

        private string EnumerationOrderToClause(EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            switch (order)
            {
                case EnumerationOrderEnum.CostAscending:
                    return "cost ASC";
                case EnumerationOrderEnum.CostDescending:
                    return "cost DESC";
                case EnumerationOrderEnum.CreatedAscending:
                    return "createdutc ASC";
                case EnumerationOrderEnum.CreatedDescending:
                    return "createdutc DESC";
                case EnumerationOrderEnum.GuidAscending:
                    return "id ASC";
                case EnumerationOrderEnum.GuidDescending:
                    return "id DESC";
                case EnumerationOrderEnum.NameAscending:
                    return "name ASC";
                case EnumerationOrderEnum.NameDescending:
                    return "name DESC";
                default:
                    throw new ArgumentException("Unsupported enumeration order '" + order.ToString() + "'.");
            }
        }

        private string ExpressionToWhereClause(string table, Expr expr)
        {
            if (expr == null) return null;
            if (expr.Left == null) return null;

            string clause = "";

            if (expr.Left is Expr)
            {
                clause += ExpressionToWhereClause(table, (Expr)expr.Left) + " ";
            }
            else
            {
                if (!(expr.Left is string))
                {
                    throw new ArgumentException("Left term must be of type Expression or String");
                }

                clause += "json_extract(" + table + ".data, '$." + Sanitize(expr.Left.ToString()) + "') ";
            }

            switch (expr.Operator)
            {
                #region Process-By-Operators

                case OperatorEnum.And:
                    #region And

                    if (expr.Right == null) return null;
                    clause += "AND ";

                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table, (Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.Or:
                    #region Or

                    if (expr.Right == null) return null;
                    clause += "OR ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table, (Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.Equals:
                    #region Equals

                    if (expr.Right == null) return null;
                    clause += "= ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table, (Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.NotEquals:
                    #region NotEquals

                    if (expr.Right == null) return null;
                    clause += "<> ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table, (Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.In:
                    #region In

                    if (expr.Right == null) return null;
                    int inAdded = 0;
                    if (!IsList(expr.Right)) return null;
                    List<object> inTempList = ObjectToList(expr.Right);
                    clause += " IN (";
                    foreach (object currObj in inTempList)
                    {
                        if (currObj == null) continue;
                        if (inAdded > 0) clause += ",";
                        if (currObj is DateTime || currObj is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(currObj).ToString(TimestampFormat) + "'";
                        }
                        else if (currObj is int || currObj is long || currObj is decimal)
                        {
                            clause += currObj.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(currObj.ToString()) + "'";
                        }
                        inAdded++;
                    }
                    clause += ")";
                    break;

                #endregion

                case OperatorEnum.NotIn:
                    #region NotIn

                    if (expr.Right == null) return null;
                    int notInAdded = 0;
                    if (!IsList(expr.Right)) return null;
                    List<object> notInTempList = ObjectToList(expr.Right);
                    clause += " NOT IN (";
                    foreach (object currObj in notInTempList)
                    {
                        if (currObj == null) continue;
                        if (notInAdded > 0) clause += ",";
                        if (currObj is DateTime || currObj is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(currObj).ToString(TimestampFormat) + "'";
                        }
                        else if (currObj is int || currObj is long || currObj is decimal)
                        {
                            clause += currObj.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(currObj.ToString()) + "'";
                        }
                        notInAdded++;
                    }
                    clause += ")";
                    break;

                #endregion

                case OperatorEnum.Contains:
                    #region Contains

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause +=
                            "(" +
                            "'$." + Sanitize(expr.Left.ToString()) + "' LIKE ('%" + Sanitize(expr.Right.ToString()) + "') " +
                            "OR '$." + Sanitize(expr.Left.ToString()) + "' LIKE ('%" + Sanitize(expr.Right.ToString()) + "%') " +
                            "OR '$." + Sanitize(expr.Left.ToString()) + "' LIKE ('" + Sanitize(expr.Right.ToString()) + "%')" +
                            ")";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.ContainsNot:
                    #region ContainsNot

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause +=
                            "(" +
                            "'$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '%" + Sanitize(expr.Right.ToString()) + "' " +
                            "AND '$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '%" + Sanitize(expr.Right.ToString()) + "%' " +
                            "AND '$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '" + Sanitize(expr.Right.ToString()) + "%'" +
                            ")";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.StartsWith:
                    #region StartsWith

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + "' LIKE '" + Sanitize(expr.Right.ToString()) + "%')";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.StartsWithNot:
                    #region StartsWithNot

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + "' NOT LIKE '" + Sanitize(expr.Right.ToString()) + "%'";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.EndsWith:
                    #region EndsWith

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + " LIKE '%" + Sanitize(expr.Right.ToString()) + "')";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.EndsWithNot:
                    #region EndsWith

                    if (expr.Right == null) return null;
                    if (expr.Right is string)
                    {
                        clause += "('$." + Sanitize(expr.Left.ToString()) + " NOT LIKE '%" + Sanitize(expr.Right.ToString()) + "')";
                    }
                    else
                    {
                        return null;
                    }
                    break;

                #endregion

                case OperatorEnum.GreaterThan:
                    #region GreaterThan

                    if (expr.Right == null) return null;
                    clause += "> ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table, (Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'$." + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.GreaterThanOrEqualTo:
                    #region GreaterThanOrEqualTo

                    if (expr.Right == null) return null;
                    clause += ">= ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table, (Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.LessThan:
                    #region LessThan

                    if (expr.Right == null) return null;
                    clause += "< ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table, (Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.LessThanOrEqualTo:
                    #region LessThanOrEqualTo

                    if (expr.Right == null) return null;
                    clause += "<= ";
                    if (expr.Right is Expr)
                    {
                        clause += ExpressionToWhereClause(table,(Expr)expr.Right);
                    }
                    else
                    {
                        if (expr.Right is DateTime || expr.Right is DateTime?)
                        {
                            clause += "'" + Convert.ToDateTime(expr.Right).ToString(TimestampFormat) + "'";
                        }
                        else if (expr.Right is int || expr.Right is long || expr.Right is decimal)
                        {
                            clause += expr.Right.ToString();
                        }
                        else
                        {
                            clause += "'" + Sanitize(expr.Right.ToString()) + "'";
                        }
                    }
                    break;

                #endregion

                case OperatorEnum.IsNull:
                    #region IsNull

                    clause += " IS NULL";
                    break;

                #endregion

                case OperatorEnum.IsNotNull:
                    #region IsNotNull

                    clause += " IS NOT NULL";
                    break;

                    #endregion

                    #endregion
            }

            clause += " ";

            return clause;
        }

        private void ValidateTags(object tags)
        {
            if (tags == null) return;
            if (tags is Dictionary<string, string> dict)
                foreach (KeyValuePair<string, string> tag in dict)
                    if (String.IsNullOrEmpty(tag.Key)) throw new ArgumentException("The supplied tags contains a null or empty key.");
            else
                throw new ArgumentException("The supplied tags property does not contain a dictionary.");
        }

        private void ValidateTenantExists(Guid tenantGuid)
        {
            if (!ExistsTenant(tenantGuid))
                throw new ArgumentException("No tenant with GUID '" + tenantGuid + "' exists.");
        }

        private void ValidateUserExists(Guid tenantGuid, Guid userGuid)
        {
            if (!ExistsUser(tenantGuid, userGuid))
                throw new ArgumentException("No user with GUID '" + userGuid + "' exists.");
        }

        private void ValidateGraphExists(Guid tenantGuid, Guid graphGuid)
        {
            if (!ExistsGraph(tenantGuid, graphGuid))
                throw new ArgumentException("No graph with GUID '" + graphGuid + "' exists.");
        }

        private void ValidateNodeExists(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            if (!ExistsNode(tenantGuid, graphGuid, nodeGuid))
                throw new ArgumentException("No node with GUID '" + nodeGuid + "' exists.");
        }

        private void ValidateEdgeExists(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            if (!ExistsEdge(tenantGuid, graphGuid, edgeGuid))
                throw new ArgumentException("No edge with GUID '" + edgeGuid + "' exists.");
        }

        #endregion

        #region Tenants

        private string InsertTenantQuery(TenantMetadata tenant)
        {
            string ret =
                "INSERT INTO 'tenants' "
                + "VALUES ("
                + "'" + tenant.GUID + "',"
                + "'" + Sanitize(tenant.Name) + "',"
                + (tenant.Active ? "1" : "0") + ","
                + "'" + Sanitize(tenant.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitize(tenant.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string SelectTenantQuery(string name)
        {
            return "SELECT * FROM 'tenants' WHERE name = '" + Sanitize(name) + "';";
        }

        private string SelectTenantQuery(Guid guid)
        {
            return "SELECT * FROM 'tenants' WHERE guid = '" + guid.ToString() + "';";
        }

        private string SelectTenantsQuery(
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'tenants' WHERE guid IS NOT NULL "
                + "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateTenantQuery(TenantMetadata tenant)
        {
            return
                "UPDATE 'tenants' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitize(tenant.Name) + "',"
                + "active = '" + (tenant.Active ? "1" : "0") + " "
                + "WHERE guid = '" + tenant.GUID + "' "
                + "RETURNING *;";
        }

        private string DeleteTenantQuery(string name)
        {
            return "DELETE FROM 'tenants' WHERE name = '" + Sanitize(name) + "';";
        }

        private string DeleteTenantQuery(Guid guid)
        {
            return "DELETE FROM 'tenants' WHERE id = '" + guid + "';";
        }

        private string DeleteTenantUsersQuery(Guid guid)
        {
            return "DELETE FROM 'users' WHERE tenantguid = '" + guid + "';";
        }

        private string DeleteTenantCredentialsQuery(Guid guid)
        {
            return "DELETE FROM 'creds' WHERE tenantguid = '" + guid + "';";
        }

        private TenantMetadata TenantFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new TenantMetadata
            {
                GUID = Guid.Parse(row["guid"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                Active = (Convert.ToInt32(GetDataRowStringValue(row, "active")) > 0 ? true : false),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString()),
                LastUpdateUtc = DateTime.Parse(row["lastupdateutc"].ToString())
            };
        }

        #endregion

        #region Users

        private string InsertUserQuery(UserMaster user)
        {
            string ret =
                "INSERT INTO 'users' "
                + "VALUES ("
                + "'" + user.GUID + "',"
                + "'" + user.TenantGUID + "',"
                + "'" + Sanitize(user.FirstName) + "',"
                + "'" + Sanitize(user.LastName) + "',"
                + "'" + Sanitize(user.Email) + "',"
                + "'" + Sanitize(user.Password) + "',"
                + (user.Active ? "1" : "0") + ","
                + "'" + Sanitize(user.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitize(user.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string SelectUserQuery(Guid tenantGuid, string email)
        {
            return "SELECT * FROM 'users' WHERE tenantguid = '" + tenantGuid + "' AND email = '" + Sanitize(email) + "';";
        }

        private string SelectUserQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'users' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private string SelectUsersQuery(
            Guid? tenantGuid,
            string email,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'users' WHERE guid IS NOT NULL ";

            if (tenantGuid != null)
                ret += "AND tenantguid = '" + tenantGuid.Value.ToString() + "' ";

            if (!String.IsNullOrEmpty(email))
                ret += "AND email = '" + Sanitize(email) + "' ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateUserQuery(UserMaster user)
        {
            return
                "UPDATE 'users' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "firstname = '" + Sanitize(user.FirstName) + "',"
                + "lastname = '" + Sanitize(user.LastName) + "',"
                + "email = '" + Sanitize(user.Email) + "',"
                + "password = '" + Sanitize(user.Password) + "',"
                + "active = '" + (user.Active ? "1" : "0") + " "
                + "WHERE guid = '" + user.GUID + "' "
                + "RETURNING *;";
        }

        private string DeleteUserQuery(string name)
        {
            return "DELETE FROM 'users' WHERE name = '" + Sanitize(name) + "';";
        }

        private string DeleteUserQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'users' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private string DeleteUserCredentialsQuery(Guid guid)
        {
            return "DELETE FROM 'creds' WHERE userguid = '" + guid + "';";
        }

        private UserMaster UserFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new UserMaster
            {
                GUID = Guid.Parse(row["guid"].ToString()),
                FirstName = GetDataRowStringValue(row, "firstname"),
                LastName = GetDataRowStringValue(row, "lastname"),
                Email = GetDataRowStringValue(row, "email"),
                Password = GetDataRowStringValue(row, "password"),
                Active = (Convert.ToInt32(GetDataRowStringValue(row, "active")) > 0 ? true : false),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString()),
                LastUpdateUtc = DateTime.Parse(row["lastupdateutc"].ToString())
            };
        }

        #endregion

        #region Credentials

        private string InsertCredentialQuery(Credential cred)
        {
            string ret =
                "INSERT INTO 'creds' "
                + "VALUES ("
                + "'" + cred.GUID + "',"
                + "'" + cred.TenantGUID + "',"
                + "'" + cred.UserGUID + "',"
                + "'" + Sanitize(cred.Name) + "',"
                + "'" + Sanitize(cred.BearerToken) + "',"
                + (cred.Active ? "1" : "0") + ","
                + "'" + Sanitize(cred.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitize(cred.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string SelectCredentialQuery(string bearerToken)
        {
            return "SELECT * FROM 'creds' WHERE bearertoken = '" + Sanitize(bearerToken) + "';";
        }

        private string SelectCredentialQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'creds' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private string SelectCredentialsQuery(
            Guid? tenantGuid,
            Guid? userGuid,
            string bearerToken,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'creds' WHERE guid IS NOT NULL ";

            if (tenantGuid != null)
                ret += "AND tenantguid = '" + tenantGuid.Value.ToString() + "' ";

            if (userGuid != null)
                ret += "AND userGuid = '" + userGuid.Value.ToString() + "' ";

            if (!String.IsNullOrEmpty(bearerToken))
                ret += "AND bearertoken = '" + Sanitize(bearerToken) + "' ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateCredentialQuery(Credential cred)
        {
            return
                "UPDATE 'creds' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitize(cred.Name) + "',"
                + "active = '" + (cred.Active ? "1" : "0") + " "
                + "WHERE guid = '" + cred.GUID + "' "
                + "RETURNING *;";
        }

        private string DeleteCredentialQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'creds' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private Credential CredentialFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new Credential
            {
                GUID = Guid.Parse(row["guid"].ToString()),
                TenantGUID = Guid.Parse(row["tenantguid"].ToString()),
                UserGUID = Guid.Parse(row["userguid"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                BearerToken = GetDataRowStringValue(row, "bearertoken"),
                Active = (Convert.ToInt32(GetDataRowStringValue(row, "active")) > 0 ? true : false),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString()),
                LastUpdateUtc = DateTime.Parse(row["lastupdateutc"].ToString())
            };
        }

        #endregion

        #region Tags

        private string InsertTagQuery(TagMetadata tag)
        {
            string ret =
                "INSERT INTO 'tags' "
                + "VALUES ("
                + "'" + tag.GUID + "',"
                + "'" + tag.TenantGUID + "',"
                + "'" + tag.GraphGUID + "',"
                + (tag.NodeGUID != null ? "'" + tag.NodeGUID.Value + "'" : "NULL") + ","
                + (tag.EdgeGUID != null ? "'" + tag.EdgeGUID.Value + "'" : "NULL") + ","
                + "'" + Sanitize(tag.Key) + "',"
                + (!String.IsNullOrEmpty(tag.Value) ? ("'" + Sanitize(tag.Value) + "'") : "NULL") + ","
                + "'" + Sanitize(tag.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitize(tag.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string InsertMultipleTagsQuery(Guid tenantGuid, Guid graphGuid, List<TagMetadata> tags)
        {
            string ret =
                "INSERT INTO 'tags' "
                + "VALUES ";

            for (int i = 0; i < tags.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "(";
                ret += "'" + tags[i].GUID + "',"
                    + "'" + tenantGuid + "',"
                    + "'" + tags[i].GraphGUID + "',"
                    + (tags[i].NodeGUID != null ? "'" + tags[i].NodeGUID.Value + "'," : "NULL,")
                    + (tags[i].EdgeGUID != null ? "'" + tags[i].EdgeGUID.Value + "'," : "NULL,")
                    + "'" + Sanitize(tags[i].Key) + "',"
                    + (!String.IsNullOrEmpty(tags[i].Value) ? "'" + Sanitize(tags[i].Value) + "'," : "NULL,")
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "'";
                ret += ")";
            }

            ret += ";";
            return ret;
        }

        private string SelectMultipleTagsQuery(Guid tenantGuid, List<Guid> guids)
        {
            string ret = "SELECT * FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' AND guid IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private string SelectTagQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private string SelectTagsQuery(
            Guid? tenantGuid,
            Guid? graphGuid,
            Guid? nodeGuid,
            Guid? edgeGuid,
            string key,
            string val,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'tags' WHERE guid IS NOT NULL ";

            if (tenantGuid != null)
                ret += "AND tenantguid = '" + tenantGuid.Value.ToString() + "' ";

            if (graphGuid != null)
                ret += "AND graphguid = '" + graphGuid.Value.ToString() + "' ";

            if (nodeGuid != null)
                ret += "AND nodeguid = '" + nodeGuid.Value.ToString() + "' ";

            if (edgeGuid != null)
                ret += "AND edgeguid = '" + edgeGuid.Value.ToString() + "' ";

            if (!String.IsNullOrEmpty(key))
                ret += "AND tagkey = '" + Sanitize(key) + "' ";

            if (!String.IsNullOrEmpty(val))
                ret += "AND tagvalue = '" + Sanitize(val) + "' ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateTagQuery(TagMetadata tag)
        {
            return
                "UPDATE 'tags' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "nodeguid = " + (tag.NodeGUID != null ? ("'" + tag.NodeGUID.Value + "'") : "NULL") + ","
                + "edgeguid = " + (tag.EdgeGUID != null ? ("'" + tag.EdgeGUID.Value + "'") : "NULL") + ","
                + "tagkey = '" + Sanitize(tag.Key) + "',"
                + "tagvalue = " + (!String.IsNullOrEmpty(tag.Value) ? ("'" + Sanitize(tag.Value) + "'") : "NULL") + " "
                + "WHERE guid = '" + tag.GUID + "' "
                + "RETURNING *;";
        }

        private string DeleteTagQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private string DeleteTagsQuery(Guid tenantGuid, Guid? graphGuid, List<Guid> nodeGuids, List<Guid> edgeGuids)
        {
            string ret = "DELETE FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' ";
            
            if (graphGuid != null) ret += "AND graphguid = '" + graphGuid + "' ";

            if (nodeGuids != null && nodeGuids.Count > 0)
            {
                string nodeGuidsStr = string.Join(",", nodeGuids.Select(g => $"'{g}'"));
                ret += "AND nodeguid IN (" + nodeGuidsStr + ") ";
            }

            if (edgeGuids != null && edgeGuids.Count > 0)
            {
                string edgeGuidsStr = string.Join(",", edgeGuids.Select(g => $"'{g}'"));
                ret += "AND edgeguid IN (" + edgeGuidsStr + ") ";
            }

            return ret;
        }

        private string DeleteTagsForGraphOnlyQuery(Guid tenantGuid, Guid graphGuid)
        {
            string ret = "DELETE * FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + graphGuid + "' "
                + "AND nodeguid IS NULL AND edgeguid IS NULL;";
            return ret;
        }

        private TagMetadata TagFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new TagMetadata
            {
                GUID = Guid.Parse(row["guid"].ToString()),
                TenantGUID = Guid.Parse(row["tenantguid"].ToString()),
                GraphGUID = Guid.Parse(row["graphguid"].ToString()),
                NodeGUID = (!String.IsNullOrEmpty(GetDataRowStringValue(row, "nodeguid")) ? Guid.Parse(row["nodeguid"].ToString()) : null),
                EdgeGUID = (!String.IsNullOrEmpty(GetDataRowStringValue(row, "nodeguid")) ? Guid.Parse(row["nodeguid"].ToString()) : null),
                Key = GetDataRowStringValue(row, "tagkey"),
                Value = GetDataRowStringValue(row, "value"),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString()),
                LastUpdateUtc = DateTime.Parse(row["lastupdateutc"].ToString())
            };
        }

        private List<TagMetadata> TagsFromDataTable(DataTable table)
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;

            List<TagMetadata> ret = new List<TagMetadata>();

            foreach (DataRow row in table.Rows)
                ret.Add(TagFromDataRow(row));

            return ret;
        }

        #endregion

        #region Graphs

        private string InsertGraphQuery(Graph graph)
        {
            string ret =
                "INSERT INTO 'graphs' "
                + "VALUES ("
                + "'" + graph.GUID + "',"
                + "'" + graph.TenantGUID + "',"
                + "'" + Sanitize(graph.Name) + "',";

            if (graph.Data == null) ret += "null,";
            else ret += "'" + Sanitize(Serializer.SerializeJson(graph.Data, false)) + "',";

            ret +=
                "'" + Sanitize(graph.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitize(graph.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string SelectGraphQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'graphs' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private string SelectGraphsQuery(
            Guid tenantGuid,
            Dictionary<string, string> tags = null,
            Expr graphFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'graphs' ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON graphs.guid = tags.graphguid "
                    + "AND graphs.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "graphs.tenantguid = '" + tenantGuid + "' ";

            if (tags != null && tags.Count > 0)
            {
                foreach (KeyValuePair<string, string> tag in tags)
                {
                    ret += "AND tags.tagkey = '" + Sanitize(tag.Key) + "' ";
                    if (!String.IsNullOrEmpty(tag.Value)) ret += "AND tags.tagvalue = '" + Sanitize(tag.Value) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (graphFilter != null) ret += "AND " + ExpressionToWhereClause("graphs", graphFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateGraphQuery(Graph graph)
        {
            return
                "UPDATE 'graphs' "
                + "SET name = '" + Sanitize(graph.Name) + "',"
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "' "
                + "WHERE guid = '" + graph.GUID + "' "
                + "AND tenantguid = '" + graph.TenantGUID + "' "
                + "RETURNING *;";
        }

        private string DeleteGraphQuery(Guid tenantGuid, string name)
        {
            return "DELETE FROM 'graphs' WHERE tenantguid = '" + tenantGuid + "' AND name = '" + Sanitize(name) + "';";
        }

        private string DeleteGraphQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'graphs' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        private string DeleteGraphEdgesQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + guid + "';";
        }

        private string DeleteGraphNodesQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'nodes' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + guid + "';";
        }

        private Graph GraphFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new Graph
            {
                TenantGUID = Guid.Parse(row["tenantguid"].ToString()),
                GUID = Guid.Parse(row["guid"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                Data = GetDataRowJsonValue(row, "data"),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString()),
                LastUpdateUtc = DateTime.Parse(row["lastupdateutc"].ToString())
            };
        }

        #endregion

        #region Nodes

        private string InsertNodeQuery(Node node)
        {
            string ret =
                "INSERT INTO 'nodes' "
                + "VALUES ("
                + "'" + node.GUID + "',"
                + "'" + node.TenantGUID + "',"
                + "'" + node.GraphGUID + "',"
                + "'" + Sanitize(node.Name) + "',";

            if (node.Data == null) ret += "null,";
            else ret += "'" + Sanitize(Serializer.SerializeJson(node.Data, false)) + "',";

            ret +=
                "'" + Sanitize(node.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitize(node.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string InsertMultipleNodesQuery(Guid tenantGuid, List<Node> nodes)
        {
            string ret =
                "INSERT INTO 'nodes' "
                + "VALUES ";

            for (int i = 0; i < nodes.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "(";
                ret += "'" + nodes[i].GUID + "',"
                    + "'" + tenantGuid + "',"
                    + "'" + nodes[i].GraphGUID + "',"
                    + "'" + Sanitize(nodes[i].Name) + "',";

                if (nodes[i].Data == null) ret += "null,";
                else ret += "'" + Sanitize(Serializer.SerializeJson(nodes[i].Data, false)) + "',";
                ret += 
                    "'" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "'"
                    + ")";
            }

            ret += ";";
            return ret;
        }

        private string SelectMultipleNodesQuery(Guid tenantGuid, List<Guid> guids)
        {
            string ret = "SELECT * FROM 'nodes' WHERE tenantguid = '" + tenantGuid + "' AND guid IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private string SelectNodeQuery(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            return "SELECT * FROM 'nodes' WHERE "
                + "guid = '" + nodeGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND graphguid = '" + graphGuid + "';";
        }

        private string SelectNodesQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Dictionary<string, string> tags = null,
            Expr nodeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'nodes' ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON nodes.guid = tags.nodeguid "
                    + "AND nodes.graphguid = tags.graphguid "
                    + "AND nodes.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "nodes.tenantguid = '" + tenantGuid + "' "
                + "AND nodes.graphguid = '" + graphGuid + "' ";

            if (tags != null && tags.Count > 0)
            {
                foreach (KeyValuePair<string, string> tag in tags)
                {
                    ret += "AND tags.tagkey = '" + Sanitize(tag.Key) + "' ";
                    if (!String.IsNullOrEmpty(tag.Value)) ret += "AND tags.tagvalue = '" + Sanitize(tag.Value) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (nodeFilter != null) ret += "AND " + ExpressionToWhereClause("nodes", nodeFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateNodeQuery(Node node)
        {
            string ret =
                "UPDATE 'nodes' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitize(node.Name) + "',";

            if (node.Data == null) ret += "data = null ";
            else ret += "data = '" + Sanitize(Serializer.SerializeJson(node.Data, false)) + "' ";

            ret +=
                "WHERE guid = '" + node.GUID + "' "
                + "RETURNING *;";

            return ret;
        }

        private string DeleteNodeQuery(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            return
                "DELETE FROM 'nodes' WHERE "
                + "guid = '" + nodeGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND graphguid = '" + graphGuid + "';";
        }

        private string DeleteNodesQuery(Guid tenantGuid, Guid graphGuid)
        {
            return
                "DELETE FROM 'nodes' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + graphGuid + "';";
        }

        private string DeleteNodesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            string ret =
                "DELETE FROM 'nodes' WHERE graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND guid IN (";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(nodeGuids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private string DeleteNodeEdgesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            string ret =
                "DELETE FROM 'edges' WHERE graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND (";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) ret += "OR ";
                ret += "(fromguid = '" + Sanitize(nodeGuids[i].ToString()) + "' OR toguid = '" + Sanitize(nodeGuids[i].ToString()) + "')";
            }

            ret += ")";
            return ret;
        }

        private string BatchExistsNodesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            string query = "WITH temp(guid) AS (VALUES ";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) query += ",";
                query += "('" + nodeGuids[i].ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.guid, CASE WHEN nodes.guid IS NOT NULL THEN 1 ELSE 0 END as \"exists\" "
                + "FROM temp "
                + "LEFT JOIN nodes ON temp.guid = nodes.guid "
                + "AND nodes.graphguid = '" + graphGuid + "' "
                + "AND nodes.tenantguid = '" + tenantGuid + "';";

            return query;
        }

        private string BatchExistsEdgesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> edgeGuids)
        {
            string query = "WITH temp(guid) AS (VALUES ";

            for (int i = 0; i < edgeGuids.Count; i++)
            {
                if (i > 0) query += ",";
                query += "('" + edgeGuids[i].ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.guid, CASE WHEN edges.guid IS NOT NULL THEN 1 ELSE 0 END as \"exists\" "
                + "FROM temp "
                + "LEFT JOIN edges ON temp.guid = edges.guid "
                + "AND edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "';";

            return query;
        }

        private string BatchExistsEdgesBetweenQuery(Guid tenantGuid, Guid graphGuid, List<EdgeBetween> edgesBetween)
        {
            string query = "WITH temp(fromguid, toguid) AS (VALUES ";

            for (int i = 0; i < edgesBetween.Count; i++)
            {
                EdgeBetween curr = edgesBetween[i];
                if (i > 0) query += ",";
                query += "('" + curr.From.ToString() + "','" + curr.To.ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.fromguid, temp.toguid, CASE WHEN edges.fromguid IS NOT NULL THEN 1 ELSE 0 END AS \"exists\" "
                + "FROM temp "
                + "LEFT JOIN edges ON temp.fromguid = edges.fromguid "
                + "AND temp.toguid = edges.toguid "
                + "AND edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "';";

            return query;
        }

        private Node NodeFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new Node
            {
                GUID = Guid.Parse(row["guid"].ToString()),
                TenantGUID = Guid.Parse(row["tenantguid"].ToString()),
                GraphGUID = Guid.Parse(row["graphguid"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                Data = GetDataRowJsonValue(row, "data"),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString()),
                LastUpdateUtc = DateTime.Parse(row["lastupdateutc"].ToString())
            };
        }

        private List<Node> NodesFromDataTable(DataTable table)
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;

            List<Node> ret = new List<Node>();

            foreach (DataRow row in table.Rows)
                ret.Add(NodeFromDataRow(row));

            return ret;
        }

        #endregion

        #region Edges

        private string InsertEdgeQuery(Edge edge)
        {
            string ret =
                "INSERT INTO 'edges' "
                + "VALUES ("
                + "'" + edge.GUID + "',"
                + "'" + edge.TenantGUID + "',"
                + "'" + edge.GraphGUID + "',"
                + "'" + Sanitize(edge.Name) + "',"
                + "'" + edge.From.ToString() + "',"
                + "'" + edge.To.ToString() + "',"
                + "'" + edge.Cost.ToString() + "',";

            if (edge.Data == null) ret += "null,";
            else ret += "'" + Sanitize(Serializer.SerializeJson(edge.Data, false)) + "',";

            ret +=
                "'" + Sanitize(edge.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitize(edge.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        private string InsertMultipleEdgesQuery(Guid tenantGuid, List<Edge> edges)
        {
            string ret =
                "INSERT INTO 'edges' "
                + "VALUES ";

            for (int i = 0; i < edges.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "("
                    + "'" + edges[i].GUID + "',"
                    + "'" + tenantGuid + ","
                    + "'" + edges[i].GraphGUID + "',"
                    + "'" + Sanitize(edges[i].Name) + "',"
                    + "'" + edges[i].From.ToString() + "',"
                    + "'" + edges[i].To.ToString() + "',"
                    + "'" + edges[i].Cost.ToString() + "',";

                if (edges[i].Data == null) ret += "null,";
                else ret += "'" + Sanitize(Serializer.SerializeJson(edges[i].Data, false)) + "',";

                ret +=
                    "'" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "'"
                    + ")";
            }

            ret += ";";
            return ret;
        }

        private string SelectMultipleEdgesQuery(Guid tenantGuid, List<Guid> guids)
        {
            string ret = "SELECT * FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND guid IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private string SelectEdgeQuery(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            return
                "SELECT * FROM 'edges' WHERE "
                + "graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND guid = '" + edgeGuid + "';";
        }

        private string SelectEdgesQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' ";

            if (tags != null && tags.Count > 0)
            {
                foreach (KeyValuePair<string, string> tag in tags)
                {
                    ret += "AND tags.tagkey = '" + Sanitize(tag.Key) + "' ";
                    if (!String.IsNullOrEmpty(tag.Value)) ret += "AND tags.tagvalue = '" + Sanitize(tag.Value) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause("edges", edgeFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectConnectedEdgesQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'edges' ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.tenantguid = '" + tenantGuid + "' AND "
                + "edges.graphguid = '" + graphGuid + "' AND "
                + "("
                + "edges.fromguid = '" + nodeGuid + "' "
                + "OR edges.toguid = '" + nodeGuid + "'"
                + ") ";

            if (tags != null && tags.Count > 0)
            {
                foreach (KeyValuePair<string, string> tag in tags)
                {
                    ret += "AND tags.tagkey = '" + Sanitize(tag.Key) + "' ";
                    if (!String.IsNullOrEmpty(tag.Value)) ret += "AND tags.tagvalue = '" + Sanitize(tag.Value) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause("edges", edgeFilter);

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectEdgesFromQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' "
                + "AND edges.fromguid = '" + nodeGuid + "' ";

            if (tags != null && tags.Count > 0)
            {
                foreach (KeyValuePair<string, string> tag in tags)
                {
                    ret += "AND tags.tagkey = '" + Sanitize(tag.Key) + "' ";
                    if (!String.IsNullOrEmpty(tag.Value)) ret += "AND tags.tagvalue = '" + Sanitize(tag.Value) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause("edges", edgeFilter) + " ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectEdgesToQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' "
                + "AND edges.toguid = '" + nodeGuid + "' ";

            if (tags != null && tags.Count > 0)
            {
                foreach (KeyValuePair<string, string> tag in tags)
                {
                    ret += "AND tags.tagkey = '" + Sanitize(tag.Key) + "' ";
                    if (!String.IsNullOrEmpty(tag.Value)) ret += "AND tags.tagvalue = '" + Sanitize(tag.Value) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause("edges", edgeFilter) + " ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string SelectEdgesBetweenQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid from,
            Guid to,
            Dictionary<string, string> tags = null,
            Expr edgeFilter = null,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' "
                + "AND edges.fromguid = '" + from + "' "
                + "AND edges.toguid = '" + to + "' ";

            if (tags != null && tags.Count > 0)
            {
                foreach (KeyValuePair<string, string> tag in tags)
                {
                    ret += "AND tags.tagkey = '" + Sanitize(tag.Key) + "' ";
                    if (!String.IsNullOrEmpty(tag.Value)) ret += "AND tags.tagvalue = '" + Sanitize(tag.Value) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + ExpressionToWhereClause("edges", edgeFilter) + " ";

            ret +=
                "ORDER BY " + EnumerationOrderToClause(order) + " "
                + "LIMIT " + SelectBatchSize + " OFFSET " + skip + ";";

            return ret;
        }

        private string UpdateEdgeQuery(Edge edge)
        {
            string ret =
                "UPDATE 'edges' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitize(edge.Name) + "',"
                + "cost = '" + edge.Cost.ToString() + "',";

            if (edge.Data == null) ret += "data = null ";
            else ret += "data = '" + Sanitize(Serializer.SerializeJson(edge.Data, false)) + "' ";

            ret +=
                "WHERE guid = '" + edge.GUID + "' "
                + "RETURNING *;";

            return ret;
        }

        private string DeleteEdgeQuery(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            return
                "DELETE FROM 'edges' WHERE "
                + "graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND guid = '" + edgeGuid + "';";
        }

        private string DeleteEdgesQuery(Guid tenantGuid, Guid graphGuid)
        {
            return
                "DELETE FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + graphGuid + "';";
        }

        private string DeleteEdgesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> edgeGuids)
        {
            string ret =
                "DELETE FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + graphGuid + "' "
                + "AND guid IN (";

            for (int i = 0; i < edgeGuids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitize(edgeGuids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        private List<Edge> EdgesFromDataTable(DataTable table)
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;

            List<Edge> ret = new List<Edge>();

            foreach (DataRow row in table.Rows)
                ret.Add(EdgeFromDataRow(row));

            return ret;
        }

        private Edge EdgeFromDataRow(DataRow row)
        {
            if (row == null) return null;

            return new Edge
            {
                GUID = Guid.Parse(row["guid"].ToString()),
                TenantGUID = Guid.Parse(row["tenantguid"].ToString()),
                GraphGUID = Guid.Parse(row["graphguid"].ToString()),
                Name = GetDataRowStringValue(row, "name"),
                From = Guid.Parse(row["fromguid"].ToString()),
                To = Guid.Parse(row["toguid"].ToString()),
                Cost = Convert.ToInt32(row["cost"].ToString()),
                Data = GetDataRowJsonValue(row, "data"),
                CreatedUtc = DateTime.Parse(row["createdutc"].ToString()),
                LastUpdateUtc = DateTime.Parse(row["lastupdateutc"].ToString())
            };
        }

        #endregion

        #region Traversal-and-Routing

        private IEnumerable<RouteDetail> GetRoutesDfs(
            Guid tenantGuid,
            Graph graph,
            Node start,
            Node end,
            Expr edgeFilter,
            Expr nodeFilter,
            List<Node> visitedNodes,
            List<Edge> visitedEdges)
        {
            #region Get-Edges

            List<Edge> edges = GetEdgesFrom(tenantGuid, graph.GUID, start.GUID, null, edgeFilter, EnumerationOrderEnum.CreatedDescending).ToList();

            #endregion

            #region Process-Each-Edge

            for (int i = 0; i < edges.Count; i++)
            {
                Edge nextEdge = edges[i];

                #region Retrieve-Next-Node

                Node nextNode = ReadNode(tenantGuid, graph.GUID, nextEdge.To);
                if (nextNode == null)
                {
                    Logging.Log(SeverityEnum.Warn, "node " + nextEdge.To + " referenced in graph " + graph.GUID + " but does not exist");
                    continue;
                }

                #endregion

                #region Check-for-End

                if (nextNode.GUID.Equals(end.GUID))
                {
                    RouteDetail routeDetail = new RouteDetail();
                    routeDetail.Edges = new List<Edge>(visitedEdges);
                    routeDetail.Edges.Add(nextEdge);
                    yield return routeDetail;
                    continue;
                }

                #endregion

                #region Check-for-Cycles

                if (visitedNodes.Any(n => n.GUID.Equals(nextEdge.To)))
                {
                    continue; // cycle
                }

                #endregion

                #region Recursion-and-Variables

                List<Node> childVisitedNodes = new List<Node>(visitedNodes);
                List<Edge> childVisitedEdges = new List<Edge>(visitedEdges);

                childVisitedNodes.Add(nextNode);
                childVisitedEdges.Add(nextEdge);

                IEnumerable<RouteDetail> recurse = GetRoutesDfs(tenantGuid, graph, nextNode, end, edgeFilter, nodeFilter, childVisitedNodes, childVisitedEdges);
                foreach (RouteDetail route in recurse)
                {
                    if (route != null) yield return route;
                }

                #endregion
            }

            #endregion
        }

        #endregion

        #endregion

#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
    }
}
